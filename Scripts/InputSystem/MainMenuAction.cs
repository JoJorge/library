namespace InputSystem
{
    /// <summary>
    /// 主選單情境的輸入動作列舉。
    /// </summary>
    [InputActionEnum(InputContext.MainMenu)]
    public enum MainMenuAction
    {
        /// <summary>
        /// 導航動作。
        /// </summary>
        Navigate,

        /// <summary>
        /// 確認動作。
        /// </summary>
        Confirm,

        /// <summary>
        /// 取消動作。
        /// </summary>
        Cancel,
    }
}
