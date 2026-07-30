namespace FsmSystem.Tests
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// 單元測試：驗證 FsmManager 邊界條件與多 Machine 管理情境。
    /// </summary>
    /// <remarks>
    /// <para><strong>Validates: Requirements 3.1, 3.2, 6.3</strong></para>
    /// </remarks>
    [TestFixture]
    public class FsmManagerEdgeCaseTests
    {
        /// <summary>
        /// 驗證 CreateMachine 傳入 null initialState 時拋出 ArgumentNullException（Requirements 6.3）。
        /// </summary>
        [Test]
        public void CreateMachineWithNullInitialStateThrowsArgumentNullException()
        {
            var manager = new FsmManager();

            Assert.Throws<ArgumentNullException>(() => manager.CreateMachine<TestMachine>(null));
        }

        /// <summary>
        /// 驗證多個 Machine 獨立運行互不干擾（Requirements 3.2）。
        /// 各 Machine 擁有不同的 State，啟動並更新後各自的計數器應獨立。
        /// </summary>
        [Test]
        public void MultipleMachinesRunIndependently()
        {
            var manager = new FsmManager();

            var stateA = new CountingState();
            var stateB = new CountingState();
            var stateC = new CountingState();

            var machineA = manager.CreateMachine<TestMachine>(stateA);
            var machineB = manager.CreateMachine<TestMachine>(stateB);
            var machineC = manager.CreateMachine<TestMachine>(stateC);

            machineA.StartMachine();
            machineB.StartMachine();
            machineC.StartMachine();

            // 執行 3 次 Update
            manager.Update();
            manager.Update();
            manager.Update();

            // 各 Machine 的 State 應獨立
            Assert.AreNotSame(machineA.CurrentState, machineB.CurrentState, "Machine A 與 B 應擁有不同的 State 實例");
            Assert.AreNotSame(machineB.CurrentState, machineC.CurrentState, "Machine B 與 C 應擁有不同的 State 實例");

            // 每個 State 應各自收到 3 次 Update 呼叫
            Assert.AreEqual(3, stateA.UpdateCount, "Machine A 的 State 應被 Update 3 次");
            Assert.AreEqual(3, stateB.UpdateCount, "Machine B 的 State 應被 Update 3 次");
            Assert.AreEqual(3, stateC.UpdateCount, "Machine C 的 State 應被 Update 3 次");

            // 各自的 StartCount 應為 1（啟動時呼叫）
            Assert.AreEqual(1, stateA.StartCount, "Machine A 的 State 應僅呼叫一次 Start");
            Assert.AreEqual(1, stateB.StartCount, "Machine B 的 State 應僅呼叫一次 Start");
            Assert.AreEqual(1, stateC.StartCount, "Machine C 的 State 應僅呼叫一次 Start");
        }

        /// <summary>
        /// 驗證 Machine 故障隔離：一個 Machine 在 Update 中拋出例外不影響其他 Machine 更新。
        /// </summary>
        [Test]
        public void MachineFaultIsolation()
        {
            var manager = new FsmManager();

            var stateNormal1 = new CountingState();
            var stateThrowing = new ThrowingState(LifecycleMethod.Update);
            var stateNormal3 = new CountingState();

            var machine1 = manager.CreateMachine<TestMachine>(stateNormal1);
            var machine2 = manager.CreateMachine<TestMachine>(stateThrowing);
            var machine3 = manager.CreateMachine<TestMachine>(stateNormal3);

            machine1.StartMachine();
            machine2.StartMachine();
            machine3.StartMachine();

            // machine2 的 ThrowingState 會在第一次 Update 時拋出 InvalidOperationException
            // FsmManager 應隔離例外，machine1 與 machine3 仍應收到 Update
            Assert.DoesNotThrow(() => manager.Update());

            Assert.AreEqual(1, stateNormal1.UpdateCount, "Machine1 應正常收到 Update 呼叫");
            Assert.AreEqual(1, stateNormal3.UpdateCount, "Machine3 應正常收到 Update 呼叫（不受 Machine2 例外影響）");
            Assert.IsTrue(stateThrowing.HasThrown, "Machine2 的 ThrowingState 應已拋出例外");
        }

        /// <summary>
        /// 驗證同時存在多個 Machine 無上限限制（Requirements 3.1）。
        /// 建立大量 Machine 並驗證全部被正確註冊與更新。
        /// </summary>
        [Test]
        public void MultipleSimultaneousMachinesNoLimit()
        {
            var manager = new FsmManager();
            const int machineCount = 100;
            var states = new CountingState[machineCount];

            for (int i = 0; i < machineCount; i++)
            {
                states[i] = new CountingState();
                var machine = manager.CreateMachine<TestMachine>(states[i]);
                machine.StartMachine();
            }

            // 執行一次 Update
            manager.Update();

            // 驗證所有 Machine 都收到 Update
            for (int i = 0; i < machineCount; i++)
            {
                Assert.AreEqual(
                    1,
                    states[i].UpdateCount,
                    $"Machine[{i}] 的 State 應收到一次 Update 呼叫");
            }
        }
    }
}
