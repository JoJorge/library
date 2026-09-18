namespace DevSystem
{
    /// <summary>
    /// DevSystem 的生命週期就緒狀態。
    /// </summary>
    public enum DevSystemState
    {
        /// <summary>
        /// 尚未初始化，功能 API 一律拒絕呼叫。
        /// </summary>
        Uninitialized,

        /// <summary>
        /// 已完成初始化，功能 API 可正常運作。
        /// </summary>
        Ready,
    }
}
