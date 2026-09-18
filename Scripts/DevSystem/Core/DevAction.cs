using InputSystem;

namespace DevSystem
{
    /// <summary>
    /// 開發者專用的輸入動作列舉。所有成員皆以 Dev 前綴標示，
    /// 並透過 [InputActionEnum] 與特定 InputContext 關聯，供衝突檢查範圍解析使用。
    /// </summary>
    [InputActionEnum(InputContext.Gameplay)]
    public enum DevAction
    {
        /// <summary>
        /// 切換開發者面板顯示的動作。
        /// </summary>
        DevTogglePanel,

        /// <summary>
        /// 重新載入當前場景的動作。
        /// </summary>
        DevReloadScene,
    }
}
