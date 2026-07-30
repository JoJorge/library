namespace FsmSystem.Tests
{
    using FsCheck;
    using FsCheck.NUnit;
    using NUnit.Framework;

    /// <summary>
    /// 屬性導向測試（Property-Based Tests），使用 FsCheck 驗證 Machine 狀態生命週期的正確性。
    /// </summary>
    /// <remarks>
    /// <para><strong>Validates: Requirements 1.1, 1.2, 1.3, 1.8</strong></para>
    /// </remarks>
    [TestFixture]
    public class MachineLifecyclePropertyTests
    {
        // Feature: fsm-system, Property 1: Start Invoked Exactly Once

        /// <summary>
        /// Property 1: Start Invoked Exactly Once —
        /// 對於任何 Machine 與任何 State，當 Machine 進入該 State 時，
        /// Start 被呼叫恰好一次，無論後續執行多少次 Update/FixedUpdate。
        /// </summary>
        /// <returns>若 Start 僅被呼叫一次則屬性成立。</returns>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.1, 1.8</strong></para>
        /// </remarks>
        [Property]
        [Category("Feature: fsm-system, Property 1: Start Invoked Exactly Once")]
        public Property StartInvokedExactlyOnce()
        {
            return Prop.ForAll(
                Gen.Choose(0, 100).ToArbitrary(),
                updateCount =>
                {
                    var state = new CountingState();
                    var machine = new TestMachine(state);
                    machine.StartMachine();

                    for (int i = 0; i < updateCount; i++)
                    {
                        machine.Update();
                        machine.FixedUpdate();
                    }

                    return (state.StartCount == 1)
                        .Label("Start should be called exactly once");
                });
        }

        // Feature: fsm-system, Property 2: Update/FixedUpdate 1-to-1 Call Mapping

        /// <summary>
        /// Property 2: Update/FixedUpdate 1-to-1 Call Mapping —
        /// 若 Machine.Update() 被呼叫 N 次，State.Update() 也恰好被呼叫 N 次；
        /// FixedUpdate 同理，呼叫 M 次即執行 M 次。
        /// </summary>
        /// <returns>若呼叫次數一一對應則屬性成立。</returns>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.2, 1.3</strong></para>
        /// </remarks>
        [Property]
        [Category("Feature: fsm-system, Property 2: Update/FixedUpdate 1-to-1 Call Mapping")]
        public Property UpdateOneToOneCallMapping()
        {
            return Prop.ForAll(
                Gen.Choose(0, 100).ToArbitrary(),
                Gen.Choose(0, 100).ToArbitrary(),
                (updateCount, fixedUpdateCount) =>
                {
                    var state = new CountingState();
                    var machine = new TestMachine(state);
                    machine.StartMachine();

                    for (int i = 0; i < updateCount; i++)
                    {
                        machine.Update();
                    }

                    for (int i = 0; i < fixedUpdateCount; i++)
                    {
                        machine.FixedUpdate();
                    }

                    return (state.UpdateCount == updateCount)
                        .Label($"State.Update count ({state.UpdateCount}) should equal Machine.Update calls ({updateCount})")
                        .And((state.FixedUpdateCount == fixedUpdateCount)
                            .Label($"State.FixedUpdate count ({state.FixedUpdateCount}) should equal Machine.FixedUpdate calls ({fixedUpdateCount})"));
                });
        }
    }
}
