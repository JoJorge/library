namespace Utils.EventSystem
{
    /// <summary>
    /// 定義所有可用的事件識別碼，作為事件系統的唯一事件識別方式。
    /// 以列舉識別事件可於編譯期驗證事件名稱正確性，避免字串拼寫錯誤。
    /// 新增事件類型僅需在此 enum 中新增成員並重新編譯，無需修改 EventManager 原始碼。
    /// </summary>
    public enum EventName
    {
        /// <summary>
        /// 未指定事件，作為預設值。
        /// </summary>
        None = 0,

        #region -1~-4 Examples(for tests)
        /// <summary>
        /// 測試事件1（範例，實際專案依需求擴充）。
        /// </summary>
        Test1 = -1,

        /// <summary>
        /// 測試事件2（範例，實際專案依需求擴充）。
        /// </summary>
        Test2 = -2,

        /// <summary>
        /// 測試事件3（範例，實際專案依需求擴充）。
        /// </summary>
        Test3 = -3,

        /// <summary>
        /// 測試事件4（範例，實際專案依需求擴充）。
        /// </summary>
        Test4 = -4,
        #endregion
    }
}
