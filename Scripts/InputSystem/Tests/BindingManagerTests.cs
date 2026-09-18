namespace InputSystem.Tests
{
    using System;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// BindingManager 的單元測試，驗證儲存失敗、載入無效 JSON、
    /// 自動載入、以及各種查詢邊界情境行為正確。
    /// </summary>
    [TestFixture]
    public class BindingManagerTests
    {
        private enum UntaggedAction
        {
            SomeAction,
        }

        private BindingManager _manager;
        private InputActionAsset _asset;
        private IBindingStorage _storage;

        [SetUp]
        public void Setup()
        {
            this._manager = new BindingManager();
            this._storage = Substitute.For<IBindingStorage>();
        }

        [TearDown]
        public void TearDown()
        {
            this._manager.Release();
            if (this._asset != null)
            {
                UnityEngine.Object.DestroyImmediate(this._asset);
                this._asset = null;
            }
        }

        /// <summary>
        /// 儲存失敗時，現有綁定應保留不變（Req 2.5）。
        /// </summary>
        [Test]
        public void SaveFailed_PreservesExistingBindings()
        {
            this._asset = CreateTestAssetWithJump();
            this._storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            this._storage.Save(Arg.Any<InputDeviceType>(), Arg.Any<string>()).Returns(false);

            this._manager.Init(this._asset, this._storage);
            this._manager.ApplyBinding(GameplayAction.Jump, "<Keyboard>/a", InputDeviceType.KeyboardMouse);

            bool result = this._manager.SaveBindings(InputDeviceType.KeyboardMouse);

            Assert.IsFalse(result);
            var paths = this._manager.GetBindingPaths(GameplayAction.Jump, InputDeviceType.KeyboardMouse);
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("<Keyboard>/a", paths[0]);
        }

        /// <summary>
        /// 載入無效 JSON 時，應回退至預設綁定（Req 2.7）。
        /// </summary>
        [Test]
        public void LoadInvalidJson_FallsBackToDefault()
        {
            this._asset = CreateTestAssetWithJump();
            this._storage.Exists(InputDeviceType.KeyboardMouse).Returns(true);
            this._storage.Load(InputDeviceType.KeyboardMouse).Returns("{{INVALID JSON}}");
            this._storage.Exists(InputDeviceType.Gamepad).Returns(false);

            this._manager.Init(this._asset, this._storage);

            var paths = this._manager.GetBindingPaths(GameplayAction.Jump, InputDeviceType.KeyboardMouse);
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("<Keyboard>/space", paths[0]);
        }

        /// <summary>
        /// 初始化時應自動載入已儲存的綁定設定（Req 2.8）。
        /// </summary>
        [Test]
        public void Init_AutoLoadsExistingBindings()
        {
            this._asset = CreateTestAssetWithJump();

            // 先用一個乾淨的 manager 產生有效的 override JSON
            string capturedJson = null;
            var setupStorage = Substitute.For<IBindingStorage>();
            setupStorage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            setupStorage.Save(Arg.Any<InputDeviceType>(), Arg.Any<string>())
                .Returns(true)
                .AndDoes(ci => capturedJson = ci.ArgAt<string>(1));

            var setupManager = new BindingManager();
            setupManager.Init(this._asset, setupStorage);
            setupManager.ApplyBinding(GameplayAction.Jump, "<Keyboard>/a", InputDeviceType.KeyboardMouse);
            setupManager.SaveBindings(InputDeviceType.KeyboardMouse);
            setupManager.Release();

            // 清除所有 override，回到預設狀態
            this._asset.RemoveAllBindingOverrides();

            // 配置 mock：啟動時 Exists 回傳 true，Load 回傳先前儲存的 JSON
            this._storage.Exists(InputDeviceType.KeyboardMouse).Returns(true);
            this._storage.Load(InputDeviceType.KeyboardMouse).Returns(capturedJson);
            this._storage.Exists(InputDeviceType.Gamepad).Returns(false);

            // 建立新的 manager 並初始化
            this._manager.Init(this._asset, this._storage);

            var paths = this._manager.GetBindingPaths(GameplayAction.Jump, InputDeviceType.KeyboardMouse);
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("<Keyboard>/a", paths[0]);
        }

        /// <summary>
        /// 查詢不存在的 Action 應回傳空集合且不拋出例外（Req 4.3）。
        /// </summary>
        [Test]
        public void QueryNonExistentAction_ReturnsEmpty()
        {
            // 建立沒有 Jump action 的 asset
            this._asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = this._asset.AddActionMap("Gameplay");
            map.AddAction("Attack", InputActionType.Button)
                .AddBinding("<Keyboard>/x", groups: "Keyboard&Mouse");

            this._storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            this._manager.Init(this._asset, this._storage);

            var paths = this._manager.GetBindingPaths(GameplayAction.Jump, InputDeviceType.KeyboardMouse);

            Assert.IsNotNull(paths);
            Assert.AreEqual(0, paths.Count);
        }

        /// <summary>
        /// Action 存在但指定裝置類型下無綁定時，應回傳空集合（Req 4.4）。
        /// </summary>
        [Test]
        public void QueryActionWithNoBindingForDevice_ReturnsEmpty()
        {
            // 建立只有 Gamepad 綁定的 Jump action
            this._asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = this._asset.AddActionMap("Gameplay");
            var jumpAction = map.AddAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Gamepad>/buttonSouth", groups: "Gamepad");

            this._storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            this._manager.Init(this._asset, this._storage);

            var paths = this._manager.GetBindingPaths(GameplayAction.Jump, InputDeviceType.KeyboardMouse);

            Assert.IsNotNull(paths);
            Assert.AreEqual(0, paths.Count);
        }

        /// <summary>
        /// 傳入未定義的 InputDeviceType 列舉值時，應回傳空集合且不拋出例外（Req 4.5）。
        /// </summary>
        [Test]
        public void QueryUndefinedDeviceType_ReturnsEmpty()
        {
            this._asset = CreateTestAssetWithJump();
            this._storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            this._manager.Init(this._asset, this._storage);

            var paths = this._manager.GetBindingPaths(GameplayAction.Jump, (InputDeviceType)99);

            Assert.IsNotNull(paths);
            Assert.AreEqual(0, paths.Count);
        }

        /// <summary>
        /// 傳入未標註 [InputActionEnum] 的列舉值時，應回傳空集合且不拋出例外（Req 4.3 variant）。
        /// </summary>
        [Test]
        public void QueryUntaggedEnum_ReturnsEmpty()
        {
            this._asset = CreateTestAssetWithJump();
            this._storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            this._manager.Init(this._asset, this._storage);

            var paths = this._manager.GetBindingPaths(UntaggedAction.SomeAction, InputDeviceType.KeyboardMouse);

            Assert.IsNotNull(paths);
            Assert.AreEqual(0, paths.Count);
        }

        private static InputActionAsset CreateTestAssetWithJump()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = asset.AddActionMap("Gameplay");
            var jumpAction = map.AddAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
            return asset;
        }
    }
}
