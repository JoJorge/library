namespace InputSystem.Tests
{
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// 輸入系統整合測試，驗證子系統協作的完整端到端行為。
    /// </summary>
    [TestFixture]
    public class InputSystemIntegrationTests
    {
        private InputSystemManager _manager;
        private InputActionAsset _asset;
        private IBindingStorage _storage;

        /// <summary>
        /// 測試初始化：重置 Singleton 並建立 mock storage。
        /// </summary>
        [SetUp]
        public void Setup()
        {
            InputSystemTestHelper.ResetSingleton();
            this._manager = InputSystemManager.Instance;

            this._storage = Substitute.For<IBindingStorage>();
            this._storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            this._storage.Save(Arg.Any<InputDeviceType>(), Arg.Any<string>()).Returns(true);
        }

        /// <summary>
        /// 測試清除：重置 Singleton 並銷毀 InputActionAsset。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            InputSystemTestHelper.ResetSingleton();

            if (this._asset != null)
            {
                Object.DestroyImmediate(this._asset);
                this._asset = null;
            }
        }

        /// <summary>
        /// 完整初始化流程：傳入有效 InputActionAsset，系統到達 Ready 狀態且所有子系統正確初始化（Req 6.1, 6.2）。
        /// </summary>
        [Test]
        public void FullInitialization_ReachesReadyState_WithAllSubsystems()
        {
            this._asset = CreateTestAsset();

            this._manager.Init(this._asset, this._storage);

            Assert.AreEqual(SystemState.Ready, this._manager.SystemState);
            Assert.IsNotNull(this._manager.DeviceDetector);
            Assert.IsNotNull(this._manager.BindingManager);
            Assert.IsNotNull(this._manager.ConflictChecker);
        }

        /// <summary>
        /// 完整初始化流程：載入已儲存的 BindingProfile 並套用至 InputActionAsset（Req 6.2）。
        /// </summary>
        [Test]
        public void FullInitialization_LoadsSavedBindingProfile()
        {
            this._asset = CreateTestAsset();

            // 先產生有效的 override JSON
            var tempManager = new BindingManager();
            var tempStorage = Substitute.For<IBindingStorage>();
            tempStorage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            string capturedJson = null;
            tempStorage.Save(Arg.Any<InputDeviceType>(), Arg.Any<string>())
                .Returns(true)
                .AndDoes(ci => capturedJson = ci.ArgAt<string>(1));

            tempManager.Init(this._asset, tempStorage);
            tempManager.ApplyBinding(GameplayAction.Jump, "<Keyboard>/a", InputDeviceType.KeyboardMouse);
            tempManager.SaveBindings(InputDeviceType.KeyboardMouse);
            tempManager.Release();

            // 清除 override 回到預設
            this._asset.RemoveAllBindingOverrides();

            // 設定 storage 回傳先前儲存的 JSON
            this._storage.Exists(InputDeviceType.KeyboardMouse).Returns(true);
            this._storage.Load(InputDeviceType.KeyboardMouse).Returns(capturedJson);

            // 透過 InputSystemManager 完整初始化
            this._manager.Init(this._asset, this._storage);

            var paths = this._manager.BindingManager.GetBindingPaths(
                GameplayAction.Jump, InputDeviceType.KeyboardMouse);
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("<Keyboard>/a", paths[0]);
        }

        /// <summary>
        /// 情境切換：切換 Context 後目標 Handler 啟用、原 Handler 停用，互斥保證（Req 5.2）。
        /// </summary>
        [Test]
        public void SwitchContext_ActivatesTarget_DeactivatesPrevious()
        {
            this._asset = CreateTestAsset();
            var gameplayHandler = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            var mainMenuHandler = InputSystemTestHelper.CreateMockHandler(InputContext.MainMenu);

            this._manager.RegisterContextHandler(gameplayHandler);
            this._manager.RegisterContextHandler(mainMenuHandler);
            this._manager.Init(this._asset, this._storage);

            // 切換到 Gameplay
            bool result1 = this._manager.SwitchContext(InputContext.Gameplay);
            Assert.IsTrue(result1);
            Assert.IsTrue(gameplayHandler.IsActive);
            Assert.IsFalse(mainMenuHandler.IsActive);
            Assert.AreEqual(InputContext.Gameplay, this._manager.GetActiveContext());

            // 切換到 MainMenu
            bool result2 = this._manager.SwitchContext(InputContext.MainMenu);
            Assert.IsTrue(result2);
            Assert.IsFalse(gameplayHandler.IsActive);
            Assert.IsTrue(mainMenuHandler.IsActive);
            Assert.AreEqual(InputContext.MainMenu, this._manager.GetActiveContext());
        }

        /// <summary>
        /// 綁定修改後即時反映：ApplyBinding 後同一幀內 GetBindingPaths 回傳更新後路徑（Req 2.3）。
        /// </summary>
        [Test]
        public void ApplyBinding_ImmediatelyReflectedInQuery()
        {
            this._asset = CreateTestAsset();
            var gameplayHandler = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            this._manager.RegisterContextHandler(gameplayHandler);
            this._manager.Init(this._asset, this._storage);

            // 確認初始綁定為 <Keyboard>/space
            var pathsBefore = this._manager.BindingManager.GetBindingPaths(
                GameplayAction.Jump, InputDeviceType.KeyboardMouse);
            Assert.AreEqual(1, pathsBefore.Count);
            Assert.AreEqual("<Keyboard>/space", pathsBefore[0]);

            // 套用新綁定
            this._manager.BindingManager.ApplyBinding(
                GameplayAction.Jump, "<Keyboard>/a", InputDeviceType.KeyboardMouse);

            // 立即查詢應反映新綁定
            var pathsAfter = this._manager.BindingManager.GetBindingPaths(
                GameplayAction.Jump, InputDeviceType.KeyboardMouse);
            Assert.AreEqual(1, pathsAfter.Count);
            Assert.AreEqual("<Keyboard>/a", pathsAfter[0]);
        }

        /// <summary>
        /// 綁定修改整合衝突檢查：ApplyBinding 產生衝突時觸發 OnConflictDetected 事件（Req 2.3, 3.3）。
        /// </summary>
        [Test]
        public void ApplyBinding_WithConflict_TriggersConflictEvent()
        {
            this._asset = CreateTestAssetWithMultipleActions();
            var gameplayHandler = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            this._manager.RegisterContextHandler(gameplayHandler);
            this._manager.Init(this._asset, this._storage);

            bool conflictFired = false;
            this._manager.ConflictChecker.OnConflictDetected += (action, conflicts) =>
            {
                conflictFired = true;
            };

            // Jump 綁定到 Attack 已有的路徑，應觸發衝突
            this._manager.BindingManager.ApplyBinding(
                GameplayAction.Jump, "<Keyboard>/x", InputDeviceType.KeyboardMouse);

            Assert.IsTrue(conflictFired);
        }

        private static InputActionAsset CreateTestAsset()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var gameplayMap = asset.AddActionMap("Gameplay");
            gameplayMap.AddAction("Jump", InputActionType.Button)
                .AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
            gameplayMap.AddAction("Move", InputActionType.Value)
                .AddBinding("<Gamepad>/leftStick", groups: "Gamepad");

            var mainMenuMap = asset.AddActionMap("MainMenu");
            mainMenuMap.AddAction("Navigate", InputActionType.Value)
                .AddBinding("<Keyboard>/upArrow", groups: "Keyboard&Mouse");
            mainMenuMap.AddAction("Confirm", InputActionType.Button)
                .AddBinding("<Keyboard>/enter", groups: "Keyboard&Mouse");

            return asset;
        }

        private static InputActionAsset CreateTestAssetWithMultipleActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var gameplayMap = asset.AddActionMap("Gameplay");
            gameplayMap.AddAction("Jump", InputActionType.Button)
                .AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
            gameplayMap.AddAction("Attack", InputActionType.Button)
                .AddBinding("<Keyboard>/x", groups: "Keyboard&Mouse");

            return asset;
        }
    }
}
