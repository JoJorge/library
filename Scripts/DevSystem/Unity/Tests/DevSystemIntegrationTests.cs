namespace DevSystem.Unity.Tests
{
    using System.Collections.Generic;
    using System.Reflection;
    using global::InputSystem;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UIElements;
    using Utils.Singleton;

    /// <summary>
    /// DevSystem 整合測試（任務 9.4）。以真實的 <see cref="DevSystemManager"/>、<see cref="DevPanel"/>
    /// 與真實的 <see cref="InputSystemManager.Instance"/>（含真實 <see cref="BindingManager"/>／
    /// <see cref="ConflictChecker"/>）串接，驗證完整生命週期、按鍵綁定的衝突檢查與套用、
    /// 面板多層導覽與釋放清理。
    /// </summary>
    /// <remarks>
    /// 這些測試在 <c>DEV_MODE</c> 已定義的組態（Edit Mode）下執行；其斷言涵蓋各 API 的實際行為分支。
    /// 每個測試於 <see cref="Setup"/>／<see cref="TearDown"/> 以反射重置 <see cref="DevSystemManager"/>
    /// 與 <see cref="InputSystemManager"/> 兩個 Singleton 的基底 <c>_instance</c> 欄位，確保狀態隔離。
    /// _Requirements: 5.1, 6.2, 6.3, 10.1, 10.3_
    /// </remarks>
    [TestFixture]
    public class DevSystemIntegrationTests
    {
        /// <summary>與 <see cref="DevAction.DevTogglePanel"/> 對應的預設鍵盤綁定路徑。</summary>
        private const string TogglePanelPath = "<Keyboard>/f1";

        /// <summary>與 <see cref="DevAction.DevReloadScene"/> 對應的預設鍵盤綁定路徑。</summary>
        private const string ReloadScenePath = "<Keyboard>/f2";

        private InputActionAsset _asset;

        [SetUp]
        public void Setup()
        {
            ResetSingleton<DevSystemManager>();
            ResetSingleton<InputSystemManager>();
        }

        [TearDown]
        public void TearDown()
        {
            // 釋放 DevSystem 與 InputSystem 以清理其持有的資源，再重置 Singleton。
            DevSystemManager.Instance.Release();
            InputSystemManager.Instance.Release();

            ResetSingleton<DevSystemManager>();
            ResetSingleton<InputSystemManager>();

            if (this._asset != null)
            {
                Object.DestroyImmediate(this._asset);
                this._asset = null;
            }
        }

        /// <summary>
        /// 完整 <see cref="DevSystemManager.Init"/> 流程：讀設定、建面板、狀態轉 Ready，
        /// 且兩個內建分頁（Dev Keys／Log Settings）掛載為面板頂層節點（需求 10.1）。
        /// </summary>
        [Test]
        public void Init_FullFlow_ReadyWithBuiltInTabsMounted()
        {
            this.InitInputSystem();

            DevSystemManager manager = DevSystemManager.Instance;

            var definition = new PanelDefinition(new List<PanelNodeDefinition>());
            bool initialized = manager.Init(definition);

            Assert.IsTrue(initialized, "Init 應回傳 true");
            Assert.AreEqual(DevSystemState.Ready, manager.State, "Init 後狀態應為 Ready");

            // 讀設定：Logger 建立並載入分類開關（涵蓋所有 LogCategory）。
            Assert.IsNotNull(manager.Logger, "Logger 應已建立");
            Assert.IsNotNull(manager.Logger.GetCategories(), "分類開關設定應已載入");

            // 建面板：KeyBinder 取用真實 InputSystem 的 BindingManager／ConflictChecker。
            Assert.IsNotNull(manager.KeyBinder, "KeyBinder 應已建立");
            Assert.IsNotNull(manager.Panel, "Panel 應已建立");

            // 兩個內建分頁掛載為面板頂層節點（順序：Dev Keys、Log Settings）。
            IReadOnlyList<PanelNode> tabs = manager.Panel.GetTabs();
            Assert.GreaterOrEqual(tabs.Count, 2, "頂層至少應含兩個內建分頁");
            Assert.AreEqual("Dev Keys", tabs[0].Title);
            Assert.AreEqual("Log Settings", tabs[1].Title);
            Assert.IsInstanceOf<DevKeyListTab>(tabs[0].Tab);
            Assert.IsInstanceOf<DebugLogSettingsTab>(tabs[1].Tab);
        }

        /// <summary>
        /// 呼叫端提供的自訂節點應接於兩個內建分頁之後，維持定義的清單順序（需求 6.3、10.1）。
        /// </summary>
        [Test]
        public void Init_WithCallerNodes_AppendsAfterBuiltInTabsInOrder()
        {
            this.InitInputSystem();

            DevSystemManager manager = DevSystemManager.Instance;

            var definition = new PanelDefinition(new List<PanelNodeDefinition>
            {
                new PanelNodeDefinition("Custom A", new StubTab("Custom A"), null),
                new PanelNodeDefinition("Custom B", new StubTab("Custom B"), null),
            });

            manager.Init(definition);

            IReadOnlyList<PanelNode> tabs = manager.Panel.GetTabs();
            Assert.AreEqual(4, tabs.Count);
            Assert.AreEqual("Dev Keys", tabs[0].Title);
            Assert.AreEqual("Log Settings", tabs[1].Title);
            Assert.AreEqual("Custom A", tabs[2].Title);
            Assert.AreEqual("Custom B", tabs[3].Title);
        }

        /// <summary>
        /// 與真實 <see cref="ConflictChecker"/> 整合：無衝突的 Dev 按鍵註冊應通過衝突檢查、
        /// 透過真實 <see cref="BindingManager"/> 套用綁定並記錄註冊（需求 5.1、10.3）。
        /// </summary>
        [Test]
        public void RegisterDevKey_NoConflict_AppliesBindingAndRecordsRegistration()
        {
            this.InitInputSystem();
            DevSystemManager manager = DevSystemManager.Instance;
            manager.Init(new PanelDefinition(new List<PanelNodeDefinition>()));

            DevKeyBindResult result = manager.KeyBinder.RegisterDevKey(
                DevAction.DevTogglePanel, TogglePanelPath, "Toggle developer panel");

            Assert.IsTrue(result.Success, "無衝突註冊應成功");
            Assert.AreEqual(DevKeyBindFailure.None, result.FailureReason);
            Assert.IsEmpty(result.Conflicts);

            // 綁定實際套用至真實 BindingManager：查詢該 Action 的綁定路徑應含目標路徑。
            IReadOnlyList<string> paths = InputSystemManager.Instance.BindingManager
                .GetBindingPaths(DevAction.DevTogglePanel, InputDeviceType.KeyboardMouse);
            Assert.Contains(TogglePanelPath, new List<string>(paths), "綁定應已套用至輸入系統");

            // 註冊已記錄：GetRegistrations 應含該筆。
            IReadOnlyList<DevKeyRegistration> registrations = manager.KeyBinder.GetRegistrations();
            Assert.AreEqual(1, registrations.Count);
            Assert.AreEqual(DevAction.DevTogglePanel, registrations[0].Action);
            Assert.AreEqual(TogglePanelPath, registrations[0].BindingPath);
        }

        /// <summary>
        /// 與真實 <see cref="ConflictChecker"/> 整合：第二個 Dev 按鍵綁定至已被佔用（相同 Context／DeviceType）
        /// 的路徑時，衝突檢查應偵測到衝突並拒絕綁定，且不寫入註冊（需求 5.1）。
        /// </summary>
        [Test]
        public void RegisterDevKey_ConflictingPath_RejectedWithConflictsAndNotRecorded()
        {
            // 資產中兩個 Dev 動作的預設綁定皆設為相同路徑，構成同 Context／DeviceType 的衝突來源。
            this.InitInputSystem(togglePath: TogglePanelPath, reloadPath: TogglePanelPath);
            DevSystemManager manager = DevSystemManager.Instance;
            manager.Init(new PanelDefinition(new List<PanelNodeDefinition>()));

            DevKeyBindResult result = manager.KeyBinder.RegisterDevKey(
                DevAction.DevTogglePanel, TogglePanelPath, "Toggle developer panel");

            Assert.IsFalse(result.Success, "與既有綁定衝突時應被拒絕");
            Assert.AreEqual(DevKeyBindFailure.Conflict, result.FailureReason);
            Assert.IsNotEmpty(result.Conflicts, "衝突清單應攜帶衝突對象");

            // 被拒絕的綁定不應寫入註冊。
            Assert.IsEmpty(manager.KeyBinder.GetRegistrations(), "衝突被拒後不應記錄註冊");
        }

        /// <summary>
        /// 面板多層導覽：選取容器節點顯示其子分頁選擇器；選取葉節點顯示其
        /// <see cref="IDevPanelTab.BuildContent"/> 內容並觸發 <see cref="IDevPanelTab.OnShown"/>（需求 6.2、6.3）。
        /// </summary>
        [Test]
        public void PanelNavigation_MultiLevelTree_SelectsContainerAndLeaf()
        {
            this.InitInputSystem();
            DevSystemManager manager = DevSystemManager.Instance;

            var leafTab = new StubTab("Leaf");
            var definition = new PanelDefinition(new List<PanelNodeDefinition>
            {
                new PanelNodeDefinition(
                    "Container",
                    null,
                    new List<PanelNodeDefinition>
                    {
                        new PanelNodeDefinition("Leaf", leafTab, null),
                    }),
            });

            manager.Init(definition);

            // 「Container」為第三個頂層節點（前兩個為內建分頁），且推導為容器節點。
            IReadOnlyList<PanelNode> tabs = manager.Panel.GetTabs();
            PanelNode container = tabs[2];
            Assert.AreEqual("Container", container.Title);
            Assert.AreEqual(PanelNodeKind.SubTabContainer, container.Kind);
            Assert.AreEqual(1, container.Depth, "頂層節點深度為 1");

            // 子節點為葉，深度為 2，順序與定義一致。
            PanelNode leaf = container.Children[0];
            Assert.AreEqual("Leaf", leaf.Title);
            Assert.AreEqual(PanelNodeKind.Leaf, leaf.Kind);
            Assert.AreEqual(2, leaf.Depth);
            Assert.AreSame(leafTab, leaf.Tab);

            // 選取葉節點應建立其內容並觸發 OnShown（透過反射存取私有 SelectNode 導覽入口）。
            InvokeSelectNode(manager.Panel, leaf);
            Assert.IsTrue(leafTab.ContentBuilt, "選取葉節點應呼叫 BuildContent");
            Assert.IsTrue(leafTab.Shown, "選取葉節點應呼叫 OnShown");
        }

        /// <summary>
        /// <see cref="DevSystemManager.Release"/> 清理：釋放後子元件參考清空、按鍵註冊全數清除、
        /// 狀態轉為未就緒（需求 10.3）。
        /// </summary>
        [Test]
        public void Release_AfterInitAndRegistration_ClearsStateAndRegistrations()
        {
            this.InitInputSystem();
            DevSystemManager manager = DevSystemManager.Instance;
            manager.Init(new PanelDefinition(new List<PanelNodeDefinition>()));

            manager.KeyBinder.RegisterDevKey(DevAction.DevTogglePanel, TogglePanelPath, "Toggle");
            Assert.AreEqual(1, manager.KeyBinder.GetRegistrations().Count, "前置：應有一筆註冊");

            bool released = manager.Release();

            Assert.IsTrue(released, "完整清除應回傳 true");
            Assert.AreEqual(DevSystemState.Uninitialized, manager.State, "Release 後狀態應為未就緒");
            Assert.IsNull(manager.Panel, "面板參考應清空");
            Assert.IsNull(manager.KeyBinder, "按鍵綁定器參考應清空");
            Assert.IsNull(manager.Logger, "日誌元件參考應清空");
        }

        /// <summary>
        /// 透過反射將 <see cref="Singleton{T}"/> 的基底靜態 <c>_instance</c> 欄位重置為 null，確保各測試狀態隔離。
        /// </summary>
        /// <typeparam name="T">繼承自 <see cref="Singleton{T}"/> 的子類別型別。</typeparam>
        private static void ResetSingleton<T>()
            where T : Singleton<T>
        {
            FieldInfo field = typeof(Singleton<T>)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, null);
        }

        /// <summary>
        /// 透過反射呼叫 <see cref="DevPanel"/> 的私有導覽入口 <c>SelectNode</c>，模擬使用者點選面板節點。
        /// </summary>
        /// <param name="panel">目標面板。</param>
        /// <param name="node">要選取的節點。</param>
        private static void InvokeSelectNode(DevPanel panel, PanelNode node)
        {
            MethodInfo selectNode = typeof(DevPanel)
                .GetMethod("SelectNode", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(selectNode, "DevPanel 應具備私有導覽入口 SelectNode");
            selectNode.Invoke(panel, new object[] { node });
        }

        /// <summary>
        /// 建立含 Gameplay action map 的真實 <see cref="InputActionAsset"/>，其動作名稱與 <see cref="DevAction"/>
        /// 成員一致（<see cref="ActionEnumResolver"/> 以 Enum.ToString() 解析），並以真實 <see cref="BindingManager"/>／
        /// <see cref="ConflictChecker"/> 初始化 <see cref="InputSystemManager.Instance"/>。
        /// 同時註冊一個 Gameplay <see cref="AbsContextHandler"/>，使衝突檢查於該 Context 生效（非未註冊的 null 情況）。
        /// </summary>
        /// <param name="togglePath">DevTogglePanel 動作的預設綁定路徑。</param>
        /// <param name="reloadPath">DevReloadScene 動作的預設綁定路徑。</param>
        private void InitInputSystem(
            string togglePath = TogglePanelPath, string reloadPath = ReloadScenePath)
        {
            this._asset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap map = this._asset.AddActionMap("Gameplay");
            map.AddAction(DevAction.DevTogglePanel.ToString(), InputActionType.Button)
                .AddBinding(togglePath, groups: "Keyboard&Mouse");
            map.AddAction(DevAction.DevReloadScene.ToString(), InputActionType.Button)
                .AddBinding(reloadPath, groups: "Keyboard&Mouse");

            InputSystemManager inputSystem = InputSystemManager.Instance;

            // 註冊 Gameplay context handler：DevAction 標註 [InputActionEnum(InputContext.Gameplay)]，
            // 衝突檢查需在該 Context 有已註冊 handler 才能判定（否則 CheckConflict 回傳 null）。
            AbsContextHandler gameplayHandler = Substitute.For<AbsContextHandler>();
            gameplayHandler.Context.Returns(InputContext.Gameplay);
            inputSystem.RegisterContextHandler(gameplayHandler);

            IBindingStorage storage = Substitute.For<IBindingStorage>();
            storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            inputSystem.Init(this._asset, storage);
        }

        /// <summary>
        /// 測試用的 <see cref="IDevPanelTab"/> 樁，記錄 <see cref="BuildContent"/> 與 <see cref="OnShown"/> 是否被呼叫。
        /// </summary>
        private sealed class StubTab : IDevPanelTab
        {
            private readonly string _title;

            public StubTab(string title)
            {
                this._title = title;
            }

            public bool ContentBuilt { get; private set; }

            public bool Shown { get; private set; }

            public string Title => this._title;

            public VisualElement BuildContent()
            {
                this.ContentBuilt = true;
                return new VisualElement();
            }

            public void OnShown()
            {
                this.Shown = true;
            }

            public void OnUpdate()
            {
            }
        }
    }
}
