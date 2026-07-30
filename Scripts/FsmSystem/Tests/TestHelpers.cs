namespace FsmSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using FsCheck;

    /// <summary>
    /// 測試用的具體 Machine 子類別，供所有測試檔案共用。
    /// </summary>
    internal class TestMachine : Machine<TestMachine>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMachine"/> class.
        /// </summary>
        /// <param name="initialState">初始 State 實例。</param>
        internal TestMachine(IState<TestMachine> initialState)
            : base(initialState)
        {
        }
    }

    /// <summary>
    /// 追蹤所有生命週期方法呼叫次數的 Mock State 實作。
    /// </summary>
    internal class CountingState : IState<TestMachine>
    {
        /// <summary>
        /// Gets Start 方法被呼叫的次數。
        /// </summary>
        public int StartCount { get; private set; }

        /// <summary>
        /// Gets Update 方法被呼叫的次數。
        /// </summary>
        public int UpdateCount { get; private set; }

        /// <summary>
        /// Gets FixedUpdate 方法被呼叫的次數。
        /// </summary>
        public int FixedUpdateCount { get; private set; }

        /// <summary>
        /// Gets End 方法被呼叫的次數。
        /// </summary>
        public int EndCount { get; private set; }

        /// <inheritdoc/>
        public void Start() => StartCount++;

        /// <inheritdoc/>
        public void Update() => UpdateCount++;

        /// <inheritdoc/>
        public void FixedUpdate() => FixedUpdateCount++;

        /// <inheritdoc/>
        public void End() => EndCount++;
    }

    /// <summary>
    /// 追蹤呼叫順序的 Mock State，使用共享的 log 清單記錄事件。
    /// </summary>
    internal class OrderTrackingState : IState<TestMachine>
    {
        /// <summary>
        /// 共享的呼叫日誌。
        /// </summary>
        private readonly List<string> _log;

        /// <summary>
        /// 此 State 的名稱標識。
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTrackingState"/> class.
        /// </summary>
        /// <param name="log">共享呼叫日誌清單。</param>
        /// <param name="name">此 State 的名稱標識。</param>
        internal OrderTrackingState(List<string> log, string name)
        {
            _log = log;
            _name = name;
        }

        /// <summary>
        /// Gets 此 State 的名稱標識。
        /// </summary>
        public string Name => _name;

        /// <inheritdoc/>
        public void Start() => _log.Add($"{_name}.Start");

        /// <inheritdoc/>
        public void Update() => _log.Add($"{_name}.Update");

        /// <inheritdoc/>
        public void FixedUpdate() => _log.Add($"{_name}.FixedUpdate");

        /// <inheritdoc/>
        public void End() => _log.Add($"{_name}.End");
    }

    /// <summary>
    /// 表示哪個生命週期方法應該拋出例外。
    /// </summary>
    internal enum LifecycleMethod
    {
        /// <summary>在 Start 方法中拋出。</summary>
        Start,

        /// <summary>在 Update 方法中拋出。</summary>
        Update,

        /// <summary>在 FixedUpdate 方法中拋出。</summary>
        FixedUpdate,

        /// <summary>在 End 方法中拋出。</summary>
        End,
    }

    /// <summary>
    /// 在指定的生命週期方法中拋出例外的可配置 Mock State 實作。
    /// </summary>
    internal class ThrowingState : IState<TestMachine>
    {
        /// <summary>
        /// 要拋出例外的生命週期方法。
        /// </summary>
        private readonly LifecycleMethod _throwAt;

        /// <summary>
        /// 記錄例外拋出後仍被呼叫的方法。
        /// </summary>
        private readonly List<string> _callsAfterException;

        /// <summary>
        /// 是否已拋出過例外。
        /// </summary>
        private bool _hasThrown;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingState"/> class.
        /// </summary>
        /// <param name="throwAt">指定在哪個生命週期方法中拋出例外。</param>
        internal ThrowingState(LifecycleMethod throwAt)
        {
            _throwAt = throwAt;
            _callsAfterException = new List<string>();
        }

        /// <summary>
        /// Gets 例外拋出後仍被呼叫的方法記錄。
        /// </summary>
        public IReadOnlyList<string> CallsAfterException => _callsAfterException;

        /// <summary>
        /// Gets a value indicating whether 是否已拋出過例外。
        /// </summary>
        public bool HasThrown => _hasThrown;

        /// <inheritdoc/>
        public void Start()
        {
            if (_hasThrown)
            {
                _callsAfterException.Add("Start");
                return;
            }

            if (_throwAt == LifecycleMethod.Start)
            {
                _hasThrown = true;
                throw new InvalidOperationException("Exception in Start");
            }
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (_hasThrown)
            {
                _callsAfterException.Add("Update");
                return;
            }

            if (_throwAt == LifecycleMethod.Update)
            {
                _hasThrown = true;
                throw new InvalidOperationException("Exception in Update");
            }
        }

        /// <inheritdoc/>
        public void FixedUpdate()
        {
            if (_hasThrown)
            {
                _callsAfterException.Add("FixedUpdate");
                return;
            }

            if (_throwAt == LifecycleMethod.FixedUpdate)
            {
                _hasThrown = true;
                throw new InvalidOperationException("Exception in FixedUpdate");
            }
        }

        /// <inheritdoc/>
        public void End()
        {
            if (_hasThrown)
            {
                _callsAfterException.Add("End");
                return;
            }

            if (_throwAt == LifecycleMethod.End)
            {
                _hasThrown = true;
                throw new InvalidOperationException("Exception in End");
            }
        }
    }

    /// <summary>
    /// 追蹤 End 呼叫後是否仍收到 Update/FixedUpdate 的 State。
    /// </summary>
    internal class PostEndTrackingState : IState<TestMachine>
    {
        /// <summary>
        /// Gets a value indicating whether End 方法是否已被呼叫。
        /// </summary>
        internal bool EndCalled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether End 呼叫後是否仍收到 Update 或 FixedUpdate。
        /// </summary>
        internal bool ReceivedCallAfterEnd { get; private set; }

        /// <inheritdoc/>
        public void Start()
        {
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (EndCalled)
            {
                ReceivedCallAfterEnd = true;
            }
        }

        /// <inheritdoc/>
        public void FixedUpdate()
        {
            if (EndCalled)
            {
                ReceivedCallAfterEnd = true;
            }
        }

        /// <inheritdoc/>
        public void End()
        {
            EndCalled = true;
        }
    }

    /// <summary>
    /// 在 End 方法中嘗試呼叫 TransitionTo 的 State。
    /// </summary>
    internal class TransitionDuringEndState : IState<TestMachine>
    {
        /// <summary>
        /// 所屬 Machine 實例。
        /// </summary>
        private readonly TestMachine _machine;

        /// <summary>
        /// 轉換目標 State。
        /// </summary>
        private readonly IState<TestMachine> _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionDuringEndState"/> class.
        /// </summary>
        /// <param name="machine">所屬 Machine 實例。</param>
        /// <param name="target">轉換目標 State。</param>
        internal TransitionDuringEndState(TestMachine machine, IState<TestMachine> target)
        {
            _machine = machine;
            _target = target;
        }

        /// <summary>
        /// Gets a value indicating whether 是否已嘗試在 End 中呼叫 TransitionTo。
        /// </summary>
        internal bool TransitionAttempted { get; private set; }

        /// <inheritdoc/>
        public void Start()
        {
        }

        /// <inheritdoc/>
        public void Update()
        {
        }

        /// <inheritdoc/>
        public void FixedUpdate()
        {
        }

        /// <inheritdoc/>
        public void End()
        {
            TransitionAttempted = true;
            _machine.TransitionTo(_target);
        }
    }

    /// <summary>
    /// 在 Start 方法中嘗試呼叫 TransitionTo 的 State。
    /// </summary>
    internal class TransitionDuringStartState : IState<TestMachine>
    {
        /// <summary>
        /// 所屬 Machine 實例。
        /// </summary>
        private readonly TestMachine _machine;

        /// <summary>
        /// 轉換目標 State。
        /// </summary>
        private readonly IState<TestMachine> _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionDuringStartState"/> class.
        /// </summary>
        /// <param name="machine">所屬 Machine 實例。</param>
        /// <param name="target">轉換目標 State。</param>
        internal TransitionDuringStartState(TestMachine machine, IState<TestMachine> target)
        {
            _machine = machine;
            _target = target;
        }

        /// <summary>
        /// Gets a value indicating whether 是否已嘗試在 Start 中呼叫 TransitionTo。
        /// </summary>
        internal bool TransitionAttempted { get; private set; }

        /// <inheritdoc/>
        public void Start()
        {
            TransitionAttempted = true;
            _machine.TransitionTo(_target);
        }

        /// <inheritdoc/>
        public void Update()
        {
        }

        /// <inheritdoc/>
        public void FixedUpdate()
        {
        }

        /// <inheritdoc/>
        public void End()
        {
        }
    }

    /// <summary>
    /// 在 Update 期間觸發 TransitionTo 至目標 State 的測試用 State。
    /// </summary>
    internal class TransitionOnUpdateState : IState<TestMachine>
    {
        /// <summary>
        /// 轉換目標 State。
        /// </summary>
        private readonly IState<TestMachine> _target;

        /// <summary>
        /// 是否已觸發過轉換。
        /// </summary>
        private bool _hasTransitioned;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionOnUpdateState"/> class.
        /// </summary>
        /// <param name="target">轉換目標 State。</param>
        internal TransitionOnUpdateState(IState<TestMachine> target)
        {
            _target = target;
        }

        /// <summary>
        /// Gets or sets 所屬 Machine 實例（延遲設定以解決循環依賴）。
        /// </summary>
        public TestMachine Machine { get; set; }

        /// <inheritdoc/>
        public void Start()
        {
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (!_hasTransitioned)
            {
                _hasTransitioned = true;
                Machine.TransitionTo(_target);
            }
        }

        /// <inheritdoc/>
        public void FixedUpdate()
        {
        }

        /// <inheritdoc/>
        public void End()
        {
        }
    }

    /// <summary>
    /// 在 FixedUpdate 期間觸發 TransitionTo 至目標 State 的測試用 State。
    /// </summary>
    internal class TransitionOnFixedUpdateState : IState<TestMachine>
    {
        /// <summary>
        /// 轉換目標 State。
        /// </summary>
        private readonly IState<TestMachine> _target;

        /// <summary>
        /// 是否已觸發過轉換。
        /// </summary>
        private bool _hasTransitioned;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionOnFixedUpdateState"/> class.
        /// </summary>
        /// <param name="target">轉換目標 State。</param>
        internal TransitionOnFixedUpdateState(IState<TestMachine> target)
        {
            _target = target;
        }

        /// <summary>
        /// Gets or sets 所屬 Machine 實例（延遲設定以解決循環依賴）。
        /// </summary>
        public TestMachine Machine { get; set; }

        /// <inheritdoc/>
        public void Start()
        {
        }

        /// <inheritdoc/>
        public void Update()
        {
        }

        /// <inheritdoc/>
        public void FixedUpdate()
        {
            if (!_hasTransitioned)
            {
                _hasTransitioned = true;
                Machine.TransitionTo(_target);
            }
        }

        /// <inheritdoc/>
        public void End()
        {
        }
    }

    /// <summary>
    /// 不做任何動作的被動 State，用於佔位或作為轉換目標。
    /// </summary>
    internal class PassiveState : IState<TestMachine>
    {
        /// <summary>
        /// Gets a value indicating whether Start 方法是否已被呼叫。
        /// </summary>
        public bool StartCalled { get; private set; }

        /// <inheritdoc/>
        public void Start() => StartCalled = true;

        /// <inheritdoc/>
        public void Update()
        {
        }

        /// <inheritdoc/>
        public void FixedUpdate()
        {
        }

        /// <inheritdoc/>
        public void End()
        {
        }
    }

    /// <summary>
    /// 表示 Machine 可執行的操作類型。
    /// </summary>
    internal enum MachineOp
    {
        /// <summary>呼叫 StartMachine。</summary>
        Start,

        /// <summary>呼叫 StopMachine。</summary>
        Stop,

        /// <summary>呼叫 Update。</summary>
        Update,

        /// <summary>呼叫 FixedUpdate。</summary>
        FixedUpdate,

        /// <summary>呼叫 TransitionTo（使用新的 PassiveState）。</summary>
        Transition,
    }

    /// <summary>
    /// FsCheck 自訂產生器：產生隨機的 Machine 操作序列。
    /// </summary>
    internal static class ArbitraryMachineOps
    {
        /// <summary>
        /// 產生隨機的 <see cref="MachineOp"/> 操作序列。
        /// </summary>
        /// <returns>操作序列的 Arbitrary 實例。</returns>
        public static Arbitrary<MachineOp[]> Generate()
        {
            var gen = Gen.ArrayOf(
                Gen.Sized(size =>
                    Gen.Choose(0, size < 5 ? 4 : 4)
                        .Select(i => (MachineOp)i)));

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 產生指定長度範圍的 <see cref="MachineOp"/> 操作序列。
        /// </summary>
        /// <param name="minLength">最小長度。</param>
        /// <param name="maxLength">最大長度。</param>
        /// <returns>操作序列的 Arbitrary 實例。</returns>
        public static Arbitrary<MachineOp[]> Generate(int minLength, int maxLength)
        {
            var opGen = Gen.Elements(
                MachineOp.Start,
                MachineOp.Stop,
                MachineOp.Update,
                MachineOp.FixedUpdate,
                MachineOp.Transition);

            var gen = Gen.Choose(minLength, maxLength)
                .SelectMany(length => Gen.ArrayOf(length, opGen));

            return gen.ToArbitrary();
        }
    }

    /// <summary>
    /// FsCheck 自訂產生器：產生隨機的 State 轉換序列。
    /// </summary>
    internal static class ArbitraryStateSequence
    {
        /// <summary>
        /// 產生隨機數量的 <see cref="PassiveState"/> 實例序列，代表轉換目標。
        /// </summary>
        /// <returns>State 序列的 Arbitrary 實例。</returns>
        public static Arbitrary<IState<TestMachine>[]> Generate()
        {
            var gen = Gen.Choose(1, 20)
                .Select(count =>
                {
                    var states = new IState<TestMachine>[count];
                    for (int i = 0; i < count; i++)
                    {
                        states[i] = new PassiveState();
                    }

                    return states;
                });

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 產生附帶呼叫日誌追蹤功能的 State 轉換序列。
        /// </summary>
        /// <param name="log">共享呼叫日誌清單。</param>
        /// <returns>OrderTrackingState 序列的 Arbitrary 實例。</returns>
        public static Arbitrary<OrderTrackingState[]> GenerateWithTracking(List<string> log)
        {
            var gen = Gen.Choose(1, 20)
                .Select(count =>
                {
                    var states = new OrderTrackingState[count];
                    for (int i = 0; i < count; i++)
                    {
                        states[i] = new OrderTrackingState(log, $"S{i}");
                    }

                    return states;
                });

            return gen.ToArbitrary();
        }
    }

    /// <summary>
    /// FsCheck 自訂產生器：產生隨機正整數，代表 Update/FixedUpdate 呼叫次數。
    /// </summary>
    internal static class ArbitraryUpdateCount
    {
        /// <summary>
        /// 產生介於 1 到 200 之間的隨機正整數。
        /// </summary>
        /// <returns>正整數的 Arbitrary 實例。</returns>
        public static Arbitrary<int> Generate()
        {
            return Gen.Choose(1, 200).ToArbitrary();
        }

        /// <summary>
        /// 產生指定範圍內的隨機正整數。
        /// </summary>
        /// <param name="min">最小值（含）。</param>
        /// <param name="max">最大值（含）。</param>
        /// <returns>正整數的 Arbitrary 實例。</returns>
        public static Arbitrary<int> Generate(int min, int max)
        {
            return Gen.Choose(min, max).ToArbitrary();
        }
    }

    /// <summary>
    /// FsCheck 自訂產生器：產生隨機的 Machine 數量，用於多實例測試。
    /// </summary>
    internal static class ArbitraryMachineCount
    {
        /// <summary>
        /// 產生介於 2 到 10 之間的隨機 Machine 數量。
        /// </summary>
        /// <returns>Machine 數量的 Arbitrary 實例。</returns>
        public static Arbitrary<int> Generate()
        {
            return Gen.Choose(2, 10).ToArbitrary();
        }

        /// <summary>
        /// 產生指定範圍內的隨機 Machine 數量。
        /// </summary>
        /// <param name="min">最小數量（含）。</param>
        /// <param name="max">最大數量（含）。</param>
        /// <returns>Machine 數量的 Arbitrary 實例。</returns>
        public static Arbitrary<int> Generate(int min, int max)
        {
            return Gen.Choose(min, max).ToArbitrary();
        }
    }

    /// <summary>
    /// FsCheck 自訂產生器：產生在隨機生命週期方法中拋出例外的 State。
    /// </summary>
    internal static class ArbitraryLifecycleException
    {
        /// <summary>
        /// 產生隨機的 <see cref="LifecycleMethod"/> 值，代表要拋出例外的方法。
        /// </summary>
        /// <returns>LifecycleMethod 的 Arbitrary 實例。</returns>
        public static Arbitrary<LifecycleMethod> Generate()
        {
            return Gen.Elements(
                LifecycleMethod.Start,
                LifecycleMethod.Update,
                LifecycleMethod.FixedUpdate,
                LifecycleMethod.End).ToArbitrary();
        }

        /// <summary>
        /// 產生隨機的 <see cref="ThrowingState"/> 實例。
        /// </summary>
        /// <returns>ThrowingState 的 Arbitrary 實例。</returns>
        public static Arbitrary<ThrowingState> GenerateState()
        {
            return Gen.Elements(
                LifecycleMethod.Start,
                LifecycleMethod.Update,
                LifecycleMethod.FixedUpdate,
                LifecycleMethod.End)
                .Select(method => new ThrowingState(method))
                .ToArbitrary();
        }
    }
}
