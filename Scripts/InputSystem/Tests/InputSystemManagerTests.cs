namespace InputSystem.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.TestTools;

    /// <summary>
    /// InputSystemManager 的單元測試，驗證情境註冊與切換相關行為。
    /// </summary>
    [TestFixture]
    public class InputSystemManagerTests
    {
        private InputSystemManager _manager;

        [SetUp]
        public void Setup()
        {
            InputSystemTestHelper.ResetSingleton();
            this._manager = InputSystemManager.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            InputSystemTestHelper.ResetSingleton();
        }

        /// <summary>
        /// 切換至未註冊的 Context 應回傳 false 且不改變啟用狀態（Req 5.8）。
        /// </summary>
        [Test]
        public void SwitchContext_UnregisteredContext_ReturnsFalse()
        {
            InputSystemTestHelper.SetSystemStateReady(this._manager);

            var gameplayHandler = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            this._manager.RegisterContextHandler(gameplayHandler);

            bool result = this._manager.SwitchContext(InputContext.MainMenu);

            Assert.IsFalse(result);
            Assert.IsNull(this._manager.GetActiveContext());
        }

        /// <summary>
        /// 重複註冊相同 Context 的 Handler 應回傳 false（Req 5.9）。
        /// </summary>
        [Test]
        public void RegisterContextHandler_DuplicateContext_ReturnsFalse()
        {
            var handler1 = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            var handler2 = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);

            bool first = this._manager.RegisterContextHandler(handler1);
            bool second = this._manager.RegisterContextHandler(handler2);

            Assert.IsTrue(first);
            Assert.IsFalse(second);

            // 驗證只有第一個 handler 生效：切換到 Gameplay 應成功
            InputSystemTestHelper.SetSystemStateReady(this._manager);
            bool switched = this._manager.SwitchContext(InputContext.Gameplay);
            Assert.IsTrue(switched);
        }

        /// <summary>
        /// 註冊達上限後，下一個 Handler 應被拒絕（Req 5.1）。
        /// </summary>
        [Test]
        public void RegisterContextHandler_MaxLimit()
        {
            int maxHandlers = (int)typeof(InputSystemManager)
                .GetField("MaxContextHandlers", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            int startContext = (int)Enum.GetValues(typeof(InputContext)).Cast<InputContext>().Max() + 1;

            for (int i = 0; i < maxHandlers; i++)
            {
                var handler = InputSystemTestHelper.CreateMockHandler((InputContext)(startContext + i));
                bool result = this._manager.RegisterContextHandler(handler);
                Assert.IsTrue(result, $"第 {i + 1} 個 handler 註冊應成功");
            }

            var overflowHandler = InputSystemTestHelper.CreateMockHandler((InputContext)(startContext + maxHandlers));
            LogAssert.Expect(LogType.Warning, $"[InputSystem] Context Handler 已達上限 ({maxHandlers})，拒絕註冊");
            bool rejected = this._manager.RegisterContextHandler(overflowHandler);
            Assert.IsFalse(rejected);
        }

        /// <summary>
        /// 初始化完成後所有 handler 皆停用、無啟用情境（Req 5.10）。
        /// </summary>
        [Test]
        public void AllHandlersDeactivated_AfterRegistration()
        {
            var mainMenuHandler = InputSystemTestHelper.CreateMockHandler(InputContext.MainMenu);
            var gameplayHandler = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            var pauseMenuHandler = InputSystemTestHelper.CreateMockHandler(InputContext.PauseMenu);

            this._manager.RegisterContextHandler(mainMenuHandler);
            this._manager.RegisterContextHandler(gameplayHandler);
            this._manager.RegisterContextHandler(pauseMenuHandler);

            Assert.IsFalse(mainMenuHandler.IsActive);
            Assert.IsFalse(gameplayHandler.IsActive);
            Assert.IsFalse(pauseMenuHandler.IsActive);
            Assert.IsNull(this._manager.GetActiveContext());
        }

        /// <summary>
        /// Asset 為 null 時系統應進入 Disabled 狀態（Req 6.5）。
        /// </summary>
        [Test]
        public void Init_NullAsset_EntersDisabledState()
        {
            var storage = Substitute.For<IBindingStorage>();

            LogAssert.Expect(LogType.Error, "[InputSystem] InputActionAsset 為 null，系統進入 Disabled 狀態");
            this._manager.Init(null, storage);
            Assert.AreEqual(SystemState.Disabled, this._manager.SystemState);
        }

        /// <summary>
        /// 系統處於 Disabled 狀態時，SwitchContext 應回傳 false 且不啟用任何 Context（Req 6.6）。
        /// </summary>
        [Test]
        public void SwitchContext_WhenDisabled_ReturnsFalse()
        {
            // 透過傳入 null asset 強制進入 Disabled 狀態
            LogAssert.Expect(LogType.Error, "[InputSystem] InputActionAsset 為 null，系統進入 Disabled 狀態");
            this._manager.Init(null, null);

            var gameplayHandler = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            this._manager.RegisterContextHandler(gameplayHandler);

            bool result = this._manager.SwitchContext(InputContext.Gameplay);

            Assert.IsFalse(result);
            Assert.IsNull(this._manager.GetActiveContext());
        }

        /// <summary>
        /// Release 應停用已啟用的 Handler、清除子系統參考並進入 Disabled 狀態（Req 6.4）。
        /// </summary>
        [Test]
        public void Release_DeactivatesAllHandlers_AndClearsState()
        {
            var gameplayHandler = InputSystemTestHelper.CreateMockHandler(InputContext.Gameplay);
            this._manager.RegisterContextHandler(gameplayHandler);

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var storage = Substitute.For<IBindingStorage>();
            storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);

            this._manager.Init(asset, storage);

            this._manager.SwitchContext(InputContext.Gameplay);
            Assert.IsTrue(gameplayHandler.IsActive);

            this._manager.Release();

            Assert.AreEqual(SystemState.Disabled, this._manager.SystemState);
            Assert.IsNull(this._manager.DeviceDetector);
            Assert.IsNull(this._manager.BindingManager);
            Assert.IsNull(this._manager.ConflictChecker);
            Assert.IsNull(this._manager.GetActiveContext());
            Assert.IsFalse(gameplayHandler.IsActive);

            UnityEngine.Object.DestroyImmediate(asset);
        }
    }
}
