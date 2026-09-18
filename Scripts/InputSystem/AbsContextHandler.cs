namespace InputSystem
{
    using UnityEngine.InputSystem;

    /// <summary>
    /// 情境處理器抽象類別，定義特定輸入情境下的啟用與停用行為。
    /// 各情境分別實作此類別，透過 ActionMap 的啟用與停用實現情境切換。
    /// </summary>
    public abstract class AbsContextHandler
    {
        /// <summary>
        /// input action asset
        /// </summary>
        protected InputActionAsset _asset;

        /// <summary>
        /// sripted action class, if you have generated action script, you can use this to access the actions.
        /// </summary>
        protected IInputActionCollection2 _actions;

        /// <summary>
        /// Gets 此處理器對應的輸入情境。
        /// </summary>
        public abstract InputContext Context { get; }

        /// <summary>
        /// Gets a value indicating whether 此處理器目前是否處於啟用狀態。
        /// </summary>
        public virtual bool IsActive { get; protected set; }

        /// <summary>
        /// 啟用此情境處理器，開始監聽對應 ActionMap 中的輸入動作並發布事件。
        /// </summary>
        /// <param name="inputAsset">輸入設定檔。</param>
        /// <param name="actions">腳本化的輸入動作集合。</param>
        public virtual void Activate(InputActionAsset inputAsset, IInputActionCollection2 actions = null)
        {
            _asset = inputAsset;
            _actions = actions;
            IsActive = true;
        }

        /// <summary>
        /// 停用此情境處理器，立即停止發布所有輸入事件。
        /// </summary>
        public virtual void Deactivate()
        {
            IsActive = false;
            _asset = null;
            _actions = null;
        }
    }
}
