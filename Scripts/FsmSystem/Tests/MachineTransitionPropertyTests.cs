namespace FsmSystem.Tests
{
    using System.Collections.Generic;
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;

    /// <summary>
    /// 屬性導向測試（Property-Based Tests），驗證 <see cref="Machine{TOwner}"/> 狀態轉換的正確性屬性。
    /// </summary>
    [TestFixture]
    public class MachineTransitionPropertyTests
    {
        // Feature: fsm-system, Property 3: Transition Sequence Ordering

        /// <summary>
        /// Property 3: Transition Sequence Ordering — 對於任何有效轉換，順序嚴格為：
        /// old State.End() → 更新 CurrentState → new State.Start()，無交錯。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.4, 1.5</strong></para>
        /// </remarks>
        public void TransitionSequenceOrdering()
        {
            Prop.ForAll(
                Gen.Choose(1, 20).ToArbitrary(),
                transitionCount =>
                {
                    var log = new List<string>();
                    var states = new List<OrderTrackingState>();

                    for (int i = 0; i <= transitionCount; i++)
                    {
                        states.Add(new OrderTrackingState(log, $"S{i}"));
                    }

                    var machine = new TestMachine(states[0]);
                    machine.StartMachine();

                    for (int i = 1; i <= transitionCount; i++)
                    {
                        machine.TransitionTo(states[i]);
                    }

                    // 驗證每次轉換的順序：S{i}.End 必須在 S{i+1}.Start 之前
                    bool orderCorrect = true;

                    for (int i = 0; i < transitionCount; i++)
                    {
                        int endIdx = log.IndexOf($"S{i}.End");
                        int startIdx = log.IndexOf($"S{i + 1}.Start");

                        if (endIdx < 0 || startIdx < 0 || endIdx >= startIdx)
                        {
                            orderCorrect = false;
                            break;
                        }
                    }

                    return orderCorrect;
                }).QuickCheckThrowOnFailure();
        }

        // Feature: fsm-system, Property 4: End Terminates Lifecycle

        /// <summary>
        /// Property 4: End Terminates Lifecycle — 在 State 的 End 被呼叫後，
        /// 該 State 不再接收 Update 或 FixedUpdate 呼叫。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.4, 2.2</strong></para>
        /// </remarks>
        public void EndTerminatesLifecycle()
        {
            Prop.ForAll(
                Gen.Choose(1, 50).ToArbitrary(),
                updatesAfterTransition =>
                {
                    var oldState = new PostEndTrackingState();
                    var newState = new PassiveState();
                    var machine = new TestMachine(oldState);
                    machine.StartMachine();

                    // 從 oldState 轉換出去
                    machine.TransitionTo(newState);

                    // 繼續更新 Machine
                    for (int i = 0; i < updatesAfterTransition; i++)
                    {
                        machine.Update();
                        machine.FixedUpdate();
                    }

                    return oldState.EndCalled && !oldState.ReceivedCallAfterEnd;
                }).QuickCheckThrowOnFailure();
        }

        // Feature: fsm-system, Property 6: Guarded Transition Rejection

        /// <summary>
        /// Property 6: Guarded Transition Rejection（End 守衛）—
        /// 在 End 回呼執行期間呼叫 TransitionTo 應被忽略。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 2.4</strong></para>
        /// </remarks>
        [Test]
        public void TransitionDuringEndIsIgnored()
        {
            var initialState = new PassiveState();
            var realMachine = new TestMachine(initialState);
            realMachine.StartMachine();

            // 轉換至一個會在 End 中嘗試 TransitionTo 的 State
            var secondTarget = new PassiveState();
            var transitionDuringEnd = new TransitionDuringEndState(realMachine, secondTarget);
            realMachine.TransitionTo(transitionDuringEnd);

            // 再次轉換離開 — transitionDuringEnd 的 End 方法會嘗試 TransitionTo(secondTarget)
            var finalState = new PassiveState();
            realMachine.TransitionTo(finalState);

            // 巢狀轉換應被忽略，最終狀態應為 finalState
            Assert.That(realMachine.CurrentState, Is.EqualTo(finalState));
        }

        /// <summary>
        /// Property 6: Guarded Transition Rejection（Start 守衛）—
        /// 在 Start 方法執行期間呼叫 TransitionTo 應被忽略。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 2.7</strong></para>
        /// </remarks>
        [Test]
        public void TransitionDuringStartIsIgnored()
        {
            var initialState = new PassiveState();
            var machine = new TestMachine(initialState);
            machine.StartMachine();

            var targetState = new PassiveState();
            var transitionDuringStart = new TransitionDuringStartState(machine, targetState);
            machine.TransitionTo(transitionDuringStart);

            // transitionDuringStart 在其 Start 中嘗試呼叫 TransitionTo，應被忽略
            Assert.That(machine.CurrentState, Is.EqualTo(transitionDuringStart));
            Assert.That(transitionDuringStart.TransitionAttempted, Is.True);
        }

        // Feature: fsm-system, Property 7: No Automatic Transitions

        /// <summary>
        /// Property 7: No Automatic Transitions — 若 State 從未呼叫 TransitionTo，
        /// 則無論 Update/FixedUpdate 呼叫多少次，CurrentState 都不會改變。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 2.5</strong></para>
        /// </remarks>
        public void NoAutomaticTransitions()
        {
            Prop.ForAll(
                Gen.Choose(1, 200).ToArbitrary(),
                updateCount =>
                {
                    var state = new PassiveState();
                    var machine = new TestMachine(state);
                    machine.StartMachine();

                    for (int i = 0; i < updateCount; i++)
                    {
                        machine.Update();
                        machine.FixedUpdate();
                    }

                    return machine.CurrentState == state;
                }).QuickCheckThrowOnFailure();
        }
    }
}
