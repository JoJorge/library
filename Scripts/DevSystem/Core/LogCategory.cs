namespace DevSystem
{
    /// <summary>
    /// 除錯日誌的分類列舉，用於逐分類的日誌開關與格式化前綴。
    /// 新增分類時直接擴充此列舉成員即可。
    /// </summary>
    public enum LogCategory
    {
        /// <summary>
        /// 一般用途分類，亦為未定義分類值時的固定預設分類。
        /// </summary>
        General,

        /// <summary>
        /// 網路相關日誌分類。
        /// </summary>
        Network,

        /// <summary>
        /// 遊戲玩法相關日誌分類。
        /// </summary>
        Gameplay,

        /// <summary>
        /// 使用者介面相關日誌分類。
        /// </summary>
        UI,
    }
}
