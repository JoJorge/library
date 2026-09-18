namespace FsmSystem
{
    using System;

    /// <summary>
    /// An independent FSM instance that holds only the current state reference.
    /// State instances are provided by the caller.
    /// </summary>
    /// <typeparam name="TOwner">
    /// The concrete Machine subclass type (CRTP pattern for type-safe transitions).
    /// </typeparam>
    public abstract class Machine<TOwner> : MachineBase
        where TOwner : Machine<TOwner>
    {
        /// <summary>
        /// 當前正在執行的 State 實例。
        /// </summary>
        private IState<TOwner> _currentState;

        /// <summary>
        /// 建立時傳入的初始 State 實例，StartMachine 時進入此 State。
        /// </summary>
        private IState<TOwner> _initialState;

        /// <summary>
        /// 轉換中旗標，防止 End/Start 期間的巢狀轉換。
        /// </summary>
        private bool _isTransitioning;

        /// <summary>
        /// Start 執行中旗標，防止 Start 中觸發轉換。
        /// </summary>
        private bool _isInStart;

        /// <summary>
        /// Initializes a new instance of the <see cref="Machine{TOwner}"/> class.
        /// </summary>
        /// <param name="initialState">The initial state to enter when the Machine starts.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="initialState"/> is <c>null</c>.
        /// </exception>
        internal Machine(IState<TOwner> initialState)
        {
            _initialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            Status = MachineStatus.Created;
        }

        /// <summary>
        /// Gets the current state of this Machine.
        /// </summary>
        public IState<TOwner> CurrentState => _currentState;

        /// <summary>
        /// Starts the Machine, entering the initial state.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the Machine has been destroyed.
        /// </exception>
        public void StartMachine()
        {
            if (Status == MachineStatus.Destroyed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (Status != MachineStatus.Created && Status != MachineStatus.Stopped)
            {
                return;
            }

            Status = MachineStatus.Running;
            _currentState = _initialState;
            _isInStart = true;
            try
            {
                _currentState.Start();
            }
            finally
            {
                _isInStart = false;
            }
        }

        /// <summary>
        /// Stops the Machine, exiting the current state.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the Machine has been destroyed.
        /// </exception>
        public override void StopMachine()
        {
            if (Status == MachineStatus.Destroyed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (Status != MachineStatus.Running)
            {
                return;
            }

            _currentState.End();
            _currentState = null;
            Status = MachineStatus.Stopped;
        }

        /// <summary>
        /// Transitions from the current state to the specified next state.
        /// Ignored if a transition is already in progress or if called during Start.
        /// </summary>
        /// <param name="nextState">The target state to transition to.</param>
        public void TransitionTo(IState<TOwner> nextState)
        {
            if (_isTransitioning)
            {
                return;
            }

            if (_isInStart)
            {
                return;
            }

            _isTransitioning = true;
            try
            {
                _currentState.End();
                _currentState = nextState;
                _isInStart = true;
                try
                {
                    _currentState.Start();
                }
                finally
                {
                    _isInStart = false;
                }
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        /// <summary>
        /// Called by FsmManager each frame to drive the current state's Update logic.
        /// </summary>
        internal override void Update()
        {
            if (Status == MachineStatus.Running && _currentState != null)
            {
                _currentState.Update();
            }
        }

        /// <summary>
        /// Called by FsmManager each fixed-time step to drive the current state's FixedUpdate logic.
        /// </summary>
        internal override void FixedUpdate()
        {
            if (Status == MachineStatus.Running && _currentState != null)
            {
                _currentState.FixedUpdate();
            }
        }
    }
}
