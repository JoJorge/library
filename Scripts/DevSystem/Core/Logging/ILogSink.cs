namespace DevSystem
{
    /// <summary>
    /// 日誌輸出的底層抽象，將已格式化的日誌字串寫入實際的輸出目的地。
    /// </summary>
    /// <remarks>
    /// 實作者負責承接 <see cref="DevLogger"/> 傳入的完整日誌字串。當底層輸出系統
    /// 失敗或不可用時，實作應回傳 <see langword="false"/> 或拋出例外；
    /// <see cref="DevLogger"/> 會於兩種情況下皆抑制該筆日誌而不外拋。
    /// </remarks>
    public interface ILogSink
    {
        /// <summary>
        /// 將一筆已格式化的日誌字串寫入輸出目的地。
        /// </summary>
        /// <param name="formatted">已依 Log_Message_Format 格式化完成的日誌字串。</param>
        /// <returns>成功寫入時回傳 <see langword="true"/>；失敗時回傳 <see langword="false"/>。</returns>
        bool Write(string formatted);
    }
}
