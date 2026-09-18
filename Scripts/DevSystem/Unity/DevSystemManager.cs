using System.Collections.Generic;
using InputSystem;
using UnityEngine;
using Utils.Singleton;

namespace DevSystem.Unity
{
    /// <summary>
    /// DevSystem 的頂層管理器，繼承 <see cref="Singleton{T}"/> 確保全域唯一存取（需求 10.2）。
    /// 負責初始化與釋放日誌、按鍵綁定與面板子系統，並維護就緒狀態守衛。
    /// </summary>
    /// <remarks>
    /// 所有功能 API 在非就緒狀態下拒絕呼叫（需求 10.5）；<see cref="Init"/> 具冪等性（需求 10.6）；
    /// <see cref="Release"/> 需完整清除，任一部分無法清除即回傳 false，但狀態仍轉為未就緒（需求 10.4、10.7）。
    /// 所有具回傳值的方法與 <see cref="State"/> 皆以 <c>DEV_MODE</c> 條件編譯符號閘控，未定義時回傳 <c>default</c>（需求 1）。
    /// </remarks>
    public class DevSystemManager : Singleton<DevSystemManager>
    {
        /// <summary>
        /// 預設 <see cref="DevLogToggleConfig"/> 資產於 Resources 下的名稱（需求 10.1 讀取 Log_Toggle_Config）。
        /// </summary>
        private const string LogToggleConfigResourceName = "Log_Toggle_Config";

        private DevSystemState _state = DevSystemState.Uninitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevSystemManager"/> class.
        /// </summary>
        protected DevSystemManager()
        {
        }

        /// <summary>
        /// Gets 目前的系統就緒狀態（需求 10.5）。
        /// </summary>
        public DevSystemState State
        {
            get
            {
#if DEV_MODE
                return this._state;
#else
                return default;
#endif
            }
        }

        /// <summary>Gets 分類日誌元件（未就緒時為 null）。</summary>
        public DevLogger Logger { get; private set; }

        /// <summary>Gets 開發者按鍵綁定元件（未就緒時為 null）。</summary>
        public DevKeyBinder KeyBinder { get; private set; }

        /// <summary>Gets 開發者面板（未就緒時為 null）。</summary>
        public DevPanel Panel { get; private set; }

        /// <summary>
        /// 初始化 DevSystem：讀取日誌開關設定、建立面板（掛入內建分頁）、取用輸入系統綁定管理器並標記為就緒。
        /// 已就緒時再次呼叫為冪等 no-op，不重複建立面板或重讀設定，維持既有狀態（需求 10.1、10.6）。
        /// </summary>
        /// <param name="definition">面板宣告式結構定義。</param>
        /// <returns>初始化成功回傳 true。</returns>
        public bool Init(PanelDefinition definition)
        {
#if DEV_MODE
            // 需求 10.6：已就緒時為冪等 no-op，不重複建立面板或重讀設定。
            if (this._state == DevSystemState.Ready)
            {
                return true;
            }

            // 需求 10.1：讀取 Log_Toggle_Config 並建立分類日誌元件。
            this.Logger = new DevLogger(new UnityDebugLogSink());
            this.Logger.LoadToggleConfig(new DevLogToggleConfigLoader(this.LoadOrCreateToggleConfig()));

            // 需求 10.3：按鍵綁定元件取用 InputSystemManager 的 BindingManager / ConflictChecker，不重新實作綁定/衝突邏輯。
            InputSystemManager inputSystem = InputSystemManager.Instance;
            this.KeyBinder = new DevKeyBinder(inputSystem.BindingManager, inputSystem.ConflictChecker);

            // 需求 10.1：建立面板，並掛入兩個內建分頁（Dev Keys、Log Settings）。
            this.Panel = new DevPanel();
            this.Panel.Build(this.WithBuiltInTabs(definition));

            // 需求 10.1：標記為就緒。
            this._state = DevSystemState.Ready;
            return true;
#else
            return default;
#endif
        }

        /// <summary>
        /// 釋放 DevSystem：釋放面板資源、清除所有 <see cref="DevKeyRegistration"/>（需求 10.3、10.4）。
        /// 未就緒時為 no-op（需求 10.7）；任一部分無法完整清除時回傳 false，但狀態仍轉為未就緒（需求 10.4）。
        /// </summary>
        /// <returns>完整清除回傳 true；未就緒的 no-op 亦回傳 true；任一部分未清除回傳 false。</returns>
        public bool Release()
        {
#if DEV_MODE
            // 需求 10.7：未就緒時為 no-op，維持 Uninitialized。
            if (this._state != DevSystemState.Ready)
            {
                return true;
            }

            // 需求 10.3、10.4：釋放面板並清除所有按鍵註冊；任一部分未完整清除即回傳 false。
            bool released = true;
            released &= this.Panel == null || this.Panel.Release();
            released &= this.KeyBinder == null || this.KeyBinder.ClearAll();

            this.Panel = null;
            this.KeyBinder = null;
            this.Logger = null;

            // 需求 10.4：無論是否完整清除，狀態皆轉為未就緒。
            this._state = DevSystemState.Uninitialized;
            return released;
#else
            return default;
#endif
        }

#if DEV_MODE
        /// <summary>
        /// 讀取名為 Log_Toggle_Config 的 <see cref="DevLogToggleConfig"/> 資產；查無時回退為記憶體內新實例，
        /// 使 <see cref="Init"/> 不因缺少資產而失敗（其啟用清單不落地，開發者於 Editor Inspector 手動設定）。
        /// </summary>
        /// <returns>日誌開關設定 ScriptableObject。</returns>
        private DevLogToggleConfig LoadOrCreateToggleConfig()
        {
            DevLogToggleConfig config = Resources.Load<DevLogToggleConfig>(LogToggleConfigResourceName);
            return config != null ? config : ScriptableObject.CreateInstance<DevLogToggleConfig>();
        }

        /// <summary>
        /// 於呼叫端提供的面板定義頂層前置兩個內建分頁節點（Dev Keys、Log Settings），供 <see cref="DevPanel.Build"/> 建樹使用。
        /// </summary>
        /// <param name="definition">呼叫端提供的面板定義（可為 null）。</param>
        /// <returns>已含兩個內建分頁的新面板定義。</returns>
        private PanelDefinition WithBuiltInTabs(PanelDefinition definition)
        {
            var nodes = new List<PanelNodeDefinition>
            {
                new PanelNodeDefinition("Dev Keys", new DevKeyListTab(this.KeyBinder), null),
                new PanelNodeDefinition("Log Settings", new DebugLogSettingsTab(this.Logger), null),
            };

            if (definition?.Nodes != null)
            {
                nodes.AddRange(definition.Nodes);
            }

            return new PanelDefinition(nodes);
        }
#endif
    }
}
