namespace InputSystem
{
    /// <summary>
    /// 綁定儲存介面，抽象化按鍵綁定的持久化機制。
    /// 實作可基於 PlayerPrefs、檔案系統或雲端存檔。
    /// </summary>
    public interface IBindingStorage
    {
        /// <summary>
        /// 儲存指定裝置類型的綁定設定 JSON 字串。
        /// </summary>
        /// <param name="deviceType">目標裝置類型。</param>
        /// <param name="json">綁定設定的 JSON 字串。</param>
        /// <returns>儲存成功回傳 <c>true</c>；失敗回傳 <c>false</c>。</returns>
        bool Save(InputDeviceType deviceType, string json);

        /// <summary>
        /// 載入指定裝置類型的綁定設定 JSON 字串。
        /// </summary>
        /// <param name="deviceType">目標裝置類型。</param>
        /// <returns>已儲存的 JSON 字串；若無資料則回傳 <c>null</c>。</returns>
        string Load(InputDeviceType deviceType);

        /// <summary>
        /// 檢查指定裝置類型是否有已儲存的綁定設定。
        /// </summary>
        /// <param name="deviceType">目標裝置類型。</param>
        /// <returns>存在已儲存資料回傳 <c>true</c>；否則回傳 <c>false</c>。</returns>
        bool Exists(InputDeviceType deviceType);
    }
}
