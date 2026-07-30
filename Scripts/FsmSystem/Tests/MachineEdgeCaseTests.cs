namespace FsmSystem.Tests
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// 單元測試：驗證 Machine 邊界條件與特殊操作情境。
    /// </summary>
    /// <remarks>
    /// <para><strong>Validates: Requirements 1.6, 2.6, 4.3, 4.5</strong></para>
    /// </remarks>
    [TestFixture]
    public class MachineEdgeCaseTests
    {
        /// <summary>
        /// 驗證初始 State 進入時不呼叫 End（Requirements 1.6）。
        /// StartMachine 進入初始 State 時只應呼叫 Start，不應呼叫 End。
        /// </summary>
        [Test]
        public void InitialStateEntryDoesNotCallEnd()
        {
            var state = new CountingState();
            var machine = new TestMachine(state);

            machine.StartMachine();

            Assert.AreEqual(1, state.StartCount, "初始 State 應被呼叫一次 Start");
            Assert.AreEqual(0, state.EndCount, "初始 State 進入時不應呼叫 End");
        }

        /// <summary>
        /// 驗證 TransitionTo 在 Update 中可成功呼叫（Requirements 2.6）。
        /// State 在 Update 方法中呼叫 TransitionTo 時，Machine 應成功轉換至目標 State。
        /// </summary>
        [Test]
        public void TransitionToInUpdateSucceeds()
        {
            var targetState = new CountingState();
            var transitionState = new TransitionOnUpdateState(targetState);
            var machine = new TestMachine(transitionState);
            transitionState.Machine = machine;

            machine.StartMachine();

            // 觸發 Update，TransitionOnUpdateState 會在 Update 中呼叫 TransitionTo
            machine.Update();

            Assert.AreSame(targetState, machine.CurrentState, "Update 中呼叫 TransitionTo 應成功切換至目標 State");
            Assert.AreEqual(1, targetState.StartCount, "目標 State 應被呼叫一次 Start");
        }

        /// <summary>
        /// 驗證 TransitionTo 在 FixedUpdate 中可成功呼叫（Requirements 2.6）。
        /// State 在 FixedUpdate 方法中呼叫 TransitionTo 時，Machine 應成功轉換至目標 State。
        /// </summary>
        [Test]
        public void TransitionToInFixedUpdateSucceeds()
        {
            var targetState = new CountingState();
            var transitionState = new TransitionOnFixedUpdateState(targetState);
            var machine = new TestMachine(transitionState);
            transitionState.Machine = machine;

            machine.StartMachine();

            // 觸發 FixedUpdate，TransitionOnFixedUpdateState 會在 FixedUpdate 中呼叫 TransitionTo
            machine.FixedUpdate();

            Assert.AreSame(targetState, machine.CurrentState, "FixedUpdate 中呼叫 TransitionTo 應成功切換至目標 State");
            Assert.AreEqual(1, targetState.StartCount, "目標 State 應被呼叫一次 Start");
        }

        /// <summary>
        /// 驗證對已 Destroyed 的 Machine 呼叫 StartMachine 拋出 ObjectDisposedException。
        /// </summary>
        [Test]
        public void DestroyedMachineStartThrowsObjectDisposedException()
        {
            var state = new CountingState();
            var machine = new TestMachine(state);
            machine.Status = MachineStatus.Destroyed;

            Assert.Throws<ObjectDisposedException>(() => machine.StartMachine());
        }

        /// <summary>
        /// 驗證對已 Destroyed 的 Machine 呼叫 StopMachine 拋出 ObjectDisposedException。
        /// </summary>
        [Test]
        public void DestroyedMachineStopThrowsObjectDisposedException()
        {
            var state = new CountingState();
            var machine = new TestMachine(state);
            machine.Status = MachineStatus.Destroyed;

            Assert.Throws<ObjectDisposedException>(() => machine.StopMachine());
        }

        /// <summary>
        /// 驗證 StartMachine 在 Running 時被忽略（Requirements 4.3）。
        /// 對已 Running 的 Machine 再次呼叫 StartMachine 應無副作用，
        /// State 的 Start 僅在首次被呼叫一次。
        /// </summary>
        [Test]
        public void StartMachineOnRunningMachineIsIgnored()
        {
            var state = new CountingState();
            var machine = new TestMachine(state);

            machine.StartMachine();
            Assert.AreEqual(MachineStatus.Running, machine.Status);
            Assert.AreEqual(1, state.StartCount, "首次 StartMachine 應呼叫 Start 一次");

            // 再次呼叫 StartMachine 應被忽略
            machine.StartMachine();

            Assert.AreEqual(MachineStatus.Running, machine.Status, "Status 應維持 Running");
            Assert.AreEqual(1, state.StartCount, "第二次 StartMachine 不應再次呼叫 Start");
        }

        /// <summary>
        /// 驗證 StopMachine 在非 Running 時被忽略（Requirements 4.5）。
        /// 對 Created 或 Stopped 的 Machine 呼叫 StopMachine 應無副作用。
        /// </summary>
        [Test]
        public void StopMachineOnNonRunningMachineIsIgnored()
        {
            var state = new CountingState();
            var machine = new TestMachine(state);

            // Machine 處於 Created 狀態，呼叫 StopMachine 應被忽略
            Assert.AreEqual(MachineStatus.Created, machine.Status);
            machine.StopMachine();
            Assert.AreEqual(MachineStatus.Created, machine.Status, "Created 狀態呼叫 StopMachine 應被忽略");
            Assert.AreEqual(0, state.EndCount, "Created 狀態 StopMachine 不應呼叫 End");

            // 啟動再停止，使 Machine 進入 Stopped 狀態
            machine.StartMachine();
            machine.StopMachine();
            Assert.AreEqual(MachineStatus.Stopped, machine.Status);

            int endCountAfterStop = state.EndCount;

            // 對 Stopped 的 Machine 再次呼叫 StopMachine 應被忽略
            machine.StopMachine();
            Assert.AreEqual(MachineStatus.Stopped, machine.Status, "Stopped 狀態再次呼叫 StopMachine 應被忽略");
            Assert.AreEqual(endCountAfterStop, state.EndCount, "Stopped 狀態 StopMachine 不應再次呼叫 End");
        }
    }
}
