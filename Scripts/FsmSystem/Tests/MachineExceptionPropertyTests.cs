namespace FsmSystem.Tests
{
    using System;
    using FsCheck;
    using FsCheck.NUnit;
    using NUnit.Framework;

    /// <summary>
    /// 屬性導向測試，驗證 Machine 例外傳播行為。
    /// </summary>
    [TestFixture]
    public class MachineExceptionPropertyTests
    {
        // Feature: fsm-system, Property 5: Exception Propagation Halts Lifecycle

        /// <summary>
        /// 驗證 Start 方法拋出例外時，例外傳播給呼叫端。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.7</strong></para>
        /// </remarks>
        [Test]
        [Category("Feature: fsm-system, Property 5: Exception Propagation Halts Lifecycle")]
        public void ExceptionInStartPropagatesAndHaltsLifecycle()
        {
            var throwingState = new ThrowingState(LifecycleMethod.Start);
            var machine = new TestMachine(throwingState);

            Assert.Throws<InvalidOperationException>(() => machine.StartMachine());
        }

        /// <summary>
        /// 驗證 Update 方法拋出例外時，例外傳播給呼叫端。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.7</strong></para>
        /// </remarks>
        [Test]
        [Category("Feature: fsm-system, Property 5: Exception Propagation Halts Lifecycle")]
        public void ExceptionInUpdatePropagatesAndHaltsLifecycle()
        {
            var throwingState = new ThrowingState(LifecycleMethod.Update);
            var machine = new TestMachine(throwingState);
            machine.StartMachine();

            Assert.Throws<InvalidOperationException>(() => machine.Update());
        }

        /// <summary>
        /// 驗證 FixedUpdate 方法拋出例外時，例外傳播給呼叫端。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.7</strong></para>
        /// </remarks>
        [Test]
        [Category("Feature: fsm-system, Property 5: Exception Propagation Halts Lifecycle")]
        public void ExceptionInFixedUpdatePropagatesAndHaltsLifecycle()
        {
            var throwingState = new ThrowingState(LifecycleMethod.FixedUpdate);
            var machine = new TestMachine(throwingState);
            machine.StartMachine();

            Assert.Throws<InvalidOperationException>(() => machine.FixedUpdate());
        }

        /// <summary>
        /// 驗證 TransitionTo 期間 End 方法拋出例外時，例外傳播且新 State 的 Start 不被呼叫。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.7</strong></para>
        /// </remarks>
        [Test]
        [Category("Feature: fsm-system, Property 5: Exception Propagation Halts Lifecycle")]
        public void ExceptionInEndDuringTransitionPropagatesAndDoesNotEnterNewState()
        {
            var throwingState = new ThrowingState(LifecycleMethod.End);
            var newState = new PassiveState();
            var machine = new TestMachine(throwingState);
            machine.StartMachine();

            Assert.Throws<InvalidOperationException>(() => machine.TransitionTo(newState));

            Assert.That(newState.StartCalled, Is.False);
        }

        /// <summary>
        /// 屬性測試：對於任意生命週期方法拋出例外的場景，例外必定傳播給呼叫端（不被吞掉）。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.7</strong></para>
        /// </remarks>
        [Property]
        [Category("Feature: fsm-system, Property 5: Exception Propagation Halts Lifecycle")]
        public Property ExceptionInAnyLifecycleMethodPropagates()
        {
            return Prop.ForAll(
                Arb.From(Gen.Elements(LifecycleMethod.Start, LifecycleMethod.Update, LifecycleMethod.FixedUpdate, LifecycleMethod.End)),
                lifecycleMethod =>
                {
                    var throwingState = new ThrowingState(lifecycleMethod);
                    var newState = new PassiveState();
                    var machine = new TestMachine(throwingState);

                    bool exceptionPropagated = false;

                    try
                    {
                        machine.StartMachine();

                        if (lifecycleMethod != LifecycleMethod.Start)
                        {
                            switch (lifecycleMethod)
                            {
                                case LifecycleMethod.Update:
                                    machine.Update();
                                    break;
                                case LifecycleMethod.FixedUpdate:
                                    machine.FixedUpdate();
                                    break;
                                case LifecycleMethod.End:
                                    machine.TransitionTo(newState);
                                    break;
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionPropagated = true;
                    }

                    return exceptionPropagated.Label(
                        $"Exception in {lifecycleMethod} must propagate to caller");
                });
        }

        /// <summary>
        /// 屬性測試：TransitionTo 期間 End 拋出例外時，新 State 的 Start 永遠不被呼叫。
        /// </summary>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.7</strong></para>
        /// </remarks>
        [Property]
        [Category("Feature: fsm-system, Property 5: Exception Propagation Halts Lifecycle")]
        public Property ExceptionInEndHaltsTransitionAndNewStateNotEntered()
        {
            return Prop.ForAll(
                Arb.From(Gen.Choose(1, 50)),
                attempts =>
                {
                    bool allBlocked = true;

                    for (int i = 0; i < attempts; i++)
                    {
                        var throwingState = new ThrowingState(LifecycleMethod.End);
                        var newState = new PassiveState();
                        var machine = new TestMachine(throwingState);
                        machine.StartMachine();

                        try
                        {
                            machine.TransitionTo(newState);
                        }
                        catch (InvalidOperationException)
                        {
                            // 預期行為
                        }

                        if (newState.StartCalled)
                        {
                            allBlocked = false;
                            break;
                        }
                    }

                    return allBlocked.Label(
                        "New State Start must never be called when End throws");
                });
        }
    }
}
