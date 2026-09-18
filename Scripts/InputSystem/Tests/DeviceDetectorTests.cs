namespace InputSystem.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    using UnityInputSystem = UnityEngine.InputSystem.InputSystem;

    /// <summary>
    /// DeviceDetector 的單元測試，驗證裝置偵測與回呼機制行為正確。
    /// </summary>
    [TestFixture]
    public class DeviceDetectorTests : InputTestFixture
    {
        private DeviceDetector _detector;

        public override void Setup()
        {
            base.Setup();
            this._detector = new DeviceDetector();
            this._detector.Init();
        }

        public override void TearDown()
        {
            this._detector.Release();
            base.TearDown();
        }

        /// <summary>
        /// 初始裝置類型應為 KeyboardMouse（Req 1.8）。
        /// </summary>
        [Test]
        public void InitialDeviceType_IsKeyboardMouse()
        {
            var detector = new DeviceDetector();
            Assert.AreEqual(InputDeviceType.KeyboardMouse, detector.CurrentDeviceType);
        }

        /// <summary>
        /// 鍵盤輸入後裝置類型應切換為 KeyboardMouse（Req 1.1）。
        /// </summary>
        [Test]
        public void KeyboardInput_SwitchesToKeyboardMouse()
        {
            var keyboard = UnityInputSystem.AddDevice<Keyboard>();
            var gamepad = UnityInputSystem.AddDevice<Gamepad>();

            // 先切到 Gamepad
            Press(gamepad.buttonSouth);
            Assert.AreEqual(InputDeviceType.Gamepad, this._detector.CurrentDeviceType);

            InputDeviceType? callbackResult = null;
            this._detector.RegisterDeviceChangedCallback(type => callbackResult = type);

            // 按鍵盤按鍵
            Press(keyboard.spaceKey);

            Assert.AreEqual(InputDeviceType.KeyboardMouse, this._detector.CurrentDeviceType);
            Assert.AreEqual(InputDeviceType.KeyboardMouse, callbackResult);
        }

        /// <summary>
        /// 滑鼠移動超過閥值後裝置類型應切換為 KeyboardMouse（Req 1.1）。
        /// </summary>
        [Test]
        public void MouseInput_SwitchesToKeyboardMouse()
        {
            var mouse = UnityInputSystem.AddDevice<Mouse>();
            var gamepad = UnityInputSystem.AddDevice<Gamepad>();

            // 先切到 Gamepad
            Press(gamepad.buttonSouth);
            Assert.AreEqual(InputDeviceType.Gamepad, this._detector.CurrentDeviceType);

            // 滑鼠移動距離超過閥值（預設 0.1）
            Set(mouse.delta, new Vector2(5f, 5f));

            Assert.AreEqual(InputDeviceType.KeyboardMouse, this._detector.CurrentDeviceType);
        }

        /// <summary>
        /// 搖桿按鍵輸入後裝置類型應切換為 Gamepad（Req 1.2）。
        /// </summary>
        [Test]
        public void GamepadButtonInput_SwitchesToGamepad()
        {
            var gamepad = UnityInputSystem.AddDevice<Gamepad>();

            InputDeviceType? callbackResult = null;
            this._detector.RegisterDeviceChangedCallback(type => callbackResult = type);

            Press(gamepad.buttonSouth);

            Assert.AreEqual(InputDeviceType.Gamepad, this._detector.CurrentDeviceType);
            Assert.AreEqual(InputDeviceType.Gamepad, callbackResult);
        }

        /// <summary>
        /// 搖桿類比軸超過閥值後裝置類型應切換為 Gamepad（Req 1.2）。
        /// </summary>
        [Test]
        public void GamepadStickInput_SwitchesToGamepad()
        {
            var gamepad = UnityInputSystem.AddDevice<Gamepad>();

            // 左搖桿位移超過閥值（預設 0.2）
            Set(gamepad.leftStick, new Vector2(0.5f, 0f));

            Assert.AreEqual(InputDeviceType.Gamepad, this._detector.CurrentDeviceType);
        }

        /// <summary>
        /// 回呼應依註冊順序觸發（Req 1.3）。
        /// </summary>
        [Test]
        public void CallbacksInvokedInRegistrationOrder()
        {
            var gamepad = UnityInputSystem.AddDevice<Gamepad>();
            var invokeOrder = new List<int>();

            this._detector.RegisterDeviceChangedCallback(_ => invokeOrder.Add(1));
            this._detector.RegisterDeviceChangedCallback(_ => invokeOrder.Add(2));
            this._detector.RegisterDeviceChangedCallback(_ => invokeOrder.Add(3));

            // 觸發裝置切換
            Press(gamepad.buttonSouth);

            Assert.AreEqual(new List<int> { 1, 2, 3 }, invokeOrder);
        }

        /// <summary>
        /// 取消註冊後不再收到回呼（Req 1.5）。
        /// </summary>
        [Test]
        public void UnregisteredCallbackNotInvoked()
        {
            var gamepad = UnityInputSystem.AddDevice<Gamepad>();
            int callCount = 0;

            void Callback(InputDeviceType type) => callCount++;

            this._detector.RegisterDeviceChangedCallback(Callback);
            this._detector.UnregisterDeviceChangedCallback(Callback);

            // 觸發裝置切換
            Press(gamepad.buttonSouth);

            Assert.AreEqual(0, callCount);
        }
    }
}
