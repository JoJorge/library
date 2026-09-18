namespace InputSystem
{
    /// <summary>
    /// 遊戲中情境的輸入動作列舉。
    /// </summary>
    [InputActionEnum(InputContext.Gameplay)]
    public enum GameplayAction
    {
        /// <summary>
        /// 移動動作。
        /// </summary>
        Move,

        /// <summary>
        /// 跳躍動作。
        /// </summary>
        Jump,

        /// <summary>
        /// 攻擊動作。
        /// </summary>
        Attack,

        /// <summary>
        /// 互動動作。
        /// </summary>
        Interact,
    }
}
