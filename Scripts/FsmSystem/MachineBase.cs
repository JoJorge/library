namespace FsmSystem
{
    /// <summary>
    /// Non-generic base class for Machine instances, enabling heterogeneous storage in FsmManager.
    /// </summary>
    public abstract class MachineBase
    {
        /// <summary>
        /// Gets or sets the lifecycle status of this Machine.
        /// </summary>
        public MachineStatus Status { get; internal set; }

        /// <summary>
        /// Stops the Machine, exiting the current state.
        /// </summary>
        public abstract void StopMachine();

        /// <summary>
        /// Called by FsmManager each frame to drive the current state's Update logic.
        /// </summary>
        internal abstract void Update();

        /// <summary>
        /// Called by FsmManager each fixed-time step to drive the current state's FixedUpdate logic.
        /// </summary>
        internal abstract void FixedUpdate();
    }
}
