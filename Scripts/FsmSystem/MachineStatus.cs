namespace FsmSystem
{
    /// <summary>
    /// Represents the lifecycle status of a Machine instance.
    /// </summary>
    public enum MachineStatus
    {
        /// <summary>Machine has been created but not yet started.</summary>
        Created,

        /// <summary>Machine is actively running and receiving updates.</summary>
        Running,

        /// <summary>Machine has been explicitly stopped.</summary>
        Stopped,

        /// <summary>Machine has been removed from FsmManager and is no longer usable.</summary>
        Destroyed,
    }
}
