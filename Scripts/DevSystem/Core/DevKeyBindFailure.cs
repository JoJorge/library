namespace DevSystem
{
    /// <summary>
    /// 開發者按鍵綁定的失敗原因。
    /// </summary>
    public enum DevKeyBindFailure
    {
        /// <summary>
        /// 無失敗，代表綁定成功。
        /// </summary>
        None,

        /// <summary>
        /// 請求缺少必要欄位，無法進行綁定。
        /// </summary>
        InvalidRequest,

        /// <summary>
        /// 綁定路徑為空或無法解析。
        /// </summary>
        InvalidPath,

        /// <summary>
        /// 指定的 DevAction 已被註冊。
        /// </summary>
        Duplicate,

        /// <summary>
        /// 與既有綁定發生衝突。
        /// </summary>
        Conflict,

        /// <summary>
        /// DevSystem 尚未就緒，拒絕綁定。
        /// </summary>
        NotReady,
    }
}
