namespace InputSystem
{
    /// <summary>
    /// 輸入系統生命週期狀態。
    /// </summary>
    public enum SystemState
    {
        /// <summary>
        /// 系統尚未初始化。
        /// </summary>
        Uninitialized,

        /// <summary>
        /// 系統正在初始化中。
        /// </summary>
        Initializing,

        /// <summary>
        /// 系統已就緒，可正常運作。
        /// </summary>
        Ready,

        /// <summary>
        /// 系統已停用，拒絕所有操作。
        /// </summary>
        Disabled,
    }
}
