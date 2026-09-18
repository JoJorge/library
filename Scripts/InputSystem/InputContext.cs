namespace InputSystem
{
    /// <summary>
    /// 輸入情境，代表遊戲中不同狀態的輸入處理範疇。
    /// </summary>
    public enum InputContext
    {
        /// <summary>
        /// 主選單情境。
        /// </summary>
        MainMenu,

        /// <summary>
        /// 遊戲中情境。
        /// </summary>
        Gameplay,

        /// <summary>
        /// 暫停選單情境。
        /// </summary>
        PauseMenu,
    }
}
