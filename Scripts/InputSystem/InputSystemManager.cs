namespace InputSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.UI;
    using Utils.Singleton;

    /// <summary>
    /// 輸入系統管理器，統籌所有輸入相關功能的核心管理類別。
    /// 繼承 Singleton 確保全域唯一存取。
    /// </summary>
    public class InputSystemManager : Singleton<InputSystemManager>
    {
        private const int MaxContextHandlers = 8;

        private readonly Dictionary<InputContext, AbsContextHandler> _contextHandlers
            = new Dictionary<InputContext, AbsContextHandler>();
        private IInputActionCollection2 _inputActions;
        private InputActionAsset _inputActionAsset;
        private InputContext? _activeContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSystemManager"/> class.
        /// </summary>
        protected InputSystemManager()
        {
        }

        /// <summary>
        /// Gets 系統目前狀態。
        /// </summary>
        public SystemState SystemState { get; private set; } = SystemState.Uninitialized;

        /// <summary>
        /// Gets 裝置偵測器實例。
        /// </summary>
        public DeviceDetector DeviceDetector { get; private set; }

        /// <summary>
        /// Gets 按鍵綁定管理器實例。
        /// </summary>
        public BindingManager BindingManager { get; private set; }

        /// <summary>
        /// Gets 衝突檢查器實例。
        /// </summary>
        public ConflictChecker ConflictChecker { get; private set; }

        /// <summary>
        /// 初始化輸入系統。載入 InputActionAsset 並初始化所有子系統。
        /// 初始化完成後系統狀態標記為 Ready；Asset 為 null 時進入 Disabled 狀態。
        /// </summary>
        /// <param name="asset">Unity InputActionAsset 資產。</param>
        /// <param name="storage">綁定儲存介面實作，可為 null（將略過綁定載入）。</param>
        /// <param name="actions">InputActionCollection2 實例，如有生成action腳本再引入。</param>
        public void Init(InputActionAsset asset, IBindingStorage storage, IInputActionCollection2 actions = null)
        {
            this.SystemState = SystemState.Initializing;

            // Req 6.5: Asset 載入失敗進入 Disabled 狀態
            if (asset == null)
            {
                Debug.LogError("[InputSystem] InputActionAsset 為 null，系統進入 Disabled 狀態");
                this.SystemState = SystemState.Disabled;
                return;
            }

            _inputActions = actions;
            _inputActionAsset = asset;

            // check UI inpu module
            var uiInputModule = GameObject.FindAnyObjectByType<InputSystemUIInputModule>();
            if (uiInputModule != null)
            {
                uiInputModule.actionsAsset = asset;
            }

            // default disable all action maps
            foreach (var map in _inputActionAsset.actionMaps)
            {
                map.Disable();
            }

            // 初始化 DeviceDetector
            this.DeviceDetector = new DeviceDetector();
            this.DeviceDetector.Init();

            // 初始化 BindingManager（Req 6.2: 載入已儲存的 BindingProfile）
            this.BindingManager = new BindingManager();
            this.BindingManager.Init(asset, storage);

            // 初始化 ConflictChecker
            this.ConflictChecker = new ConflictChecker();
            this.ConflictChecker.Init(asset, this._contextHandlers);

            // 串接 BindingManager 與 ConflictChecker（Req 3.3: 動態綁定後自動觸發衝突檢查）
            this.BindingManager.SetConflictChecker(this.ConflictChecker);

            // Req 6.1: 初始化完成後系統狀態標記為就緒
            this.SystemState = SystemState.Ready;
        }

        /// <summary>
        /// 釋放所有輸入系統資源，停用所有處理器並取消事件訂閱。
        /// 支援在 Initializing 狀態下呼叫（Req 6.7：釋放已完成配置的部分資源）。
        /// </summary>
        public void Release()
        {
            this.SystemState = SystemState.Disabled;

            // 停用所有已啟用的情境處理器
            if (this._activeContext.HasValue && this._contextHandlers.ContainsKey(this._activeContext.Value))
            {
                this._contextHandlers[this._activeContext.Value].Deactivate();
            }

            this._activeContext = null;

            // 釋放子系統（僅釋放已初始化的部分，支援 Req 6.7）
            if (this.DeviceDetector != null)
            {
                this.DeviceDetector.Release();
                this.DeviceDetector = null;
            }

            if (this.BindingManager != null)
            {
                this.BindingManager.Release();
                this.BindingManager = null;
            }

            if (this.ConflictChecker != null)
            {
                this.ConflictChecker.Release();
                this.ConflictChecker = null;
            }

            this._contextHandlers.Clear();
            this._inputActionAsset = null;
        }

        /// <summary>
        /// 註冊情境處理器。每個 InputContext 僅允許一個 Handler，由常數設定上限。
        /// </summary>
        /// <param name="handler">要註冊的情境處理器。</param>
        /// <returns>註冊成功回傳 true；handler 為 null、重複註冊或已達上限時回傳 false。</returns>
        public bool RegisterContextHandler(AbsContextHandler handler)
        {
            if (handler == null)
            {
                Debug.LogWarning("[InputSystem] 傳入的 handler 為 null");
                return false;
            }

            if (this._contextHandlers.Count >= MaxContextHandlers)
            {
                Debug.LogWarning($"[InputSystem] Context Handler 已達上限 ({MaxContextHandlers})，拒絕註冊");
                return false;
            }

            if (this._contextHandlers.ContainsKey(handler.Context))
            {
                Debug.LogWarning($"[InputSystem] Context {handler.Context} 已註冊對應的 Handler，拒絕重複註冊");
                return false;
            }

            this._contextHandlers[handler.Context] = handler;
            return true;
        }

        /// <summary>
        /// 切換啟用的情境。先停用當前啟用的 Handler，再啟用目標 Handler，確保同一時間僅一個啟用。
        /// </summary>
        /// <param name="target">目標情境。</param>
        /// <returns>切換成功回傳 true；系統未就緒或目標未註冊時維持現狀並回傳 false。</returns>
        public bool SwitchContext(InputContext target)
        {
            // Req 6.6: 系統未就緒時拒絕操作
            if (this.SystemState != SystemState.Ready)
            {
                Debug.LogWarning("[InputSystem] 系統尚未就緒，拒絕情境切換操作");
                return false;
            }

            if (!this._contextHandlers.ContainsKey(target))
            {
                Debug.LogWarning($"[InputSystem] 目標 Context {target} 未註冊，維持現狀");
                return false;
            }

            // 停用當前啟用的 Handler
            if (this._activeContext.HasValue && this._contextHandlers.ContainsKey(this._activeContext.Value))
            {
                this._contextHandlers[this._activeContext.Value].Deactivate();
            }

            // 啟用目標 Handler
            this._contextHandlers[target].Activate(_inputActionAsset, _inputActions);
            this._activeContext = target;
            return true;
        }

        /// <summary>
        /// 查詢目前啟用的情境。
        /// </summary>
        /// <returns>目前啟用的情境；若無啟用情境回傳 null。</returns>
        public InputContext? GetActiveContext()
        {
            return this._activeContext;
        }
    }
}
