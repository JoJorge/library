namespace InputSystem.Tests
{
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;

    using UnityInputSystem = UnityEngine.InputSystem.InputSystem;

    /// <summary>
    /// DeviceDetector 的屬性導向測試（Property-Based Testing）。
    /// 使用 FsCheck 生成隨機輸入序列，驗證裝置偵測行為的正確性。
    /// </summary>
    [TestFixture]
    public class DeviceDetectorPropertyTests
    {
        /// <summary>
        /// Property 1: Device Type Stability — 同裝置連續輸入不重複觸發事件。
        /// 對於任意長度的輸入事件序列，若所有事件均來自同一裝置類型，
        /// 則 DeviceDetector 的裝置切換事件觸發次數應為零。
        /// **Validates: Requirements 1.7**
        /// </summary>
        [Test]
        public void SameDeviceTypeInputs_NeverTriggersDeviceChangedCallback()
        {
            var countArb = Arb.From(Gen.Choose(1, 500));
            var deviceTypeArb = Arb.From(Gen.Elements(InputDeviceType.KeyboardMouse, InputDeviceType.Gamepad));

            Prop.ForAll(countArb, deviceTypeArb, (count, deviceType) =>
            {
                var fixture = new InputTestFixture();
                fixture.Setup();
                try
                {
                    var detector = new DeviceDetector();
                    detector.Init();

                    // 初始裝置為 KeyboardMouse；若測試 Gamepad 需先送一次切換使狀態匹配
                    if (deviceType == InputDeviceType.Gamepad)
                    {
                        var setupGamepad = UnityInputSystem.AddDevice<Gamepad>();
                        UnityInputSystem.QueueStateEvent(setupGamepad, new GamepadState { buttons = 1 });
                        UnityInputSystem.Update();
                    }

                    int callbackCount = 0;
                    detector.RegisterDeviceChangedCallback(_ => callbackCount++);

                    // 連續送出同一裝置類型的輸入事件
                    if (deviceType == InputDeviceType.KeyboardMouse)
                    {
                        var keyboard = UnityInputSystem.AddDevice<Keyboard>();
                        for (int i = 0; i < count; i++)
                        {
                            UnityInputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                            UnityInputSystem.Update();
                            UnityInputSystem.QueueStateEvent(keyboard, new KeyboardState());
                            UnityInputSystem.Update();
                        }
                    }
                    else
                    {
                        var gamepad = UnityInputSystem.AddDevice<Gamepad>();
                        for (int i = 0; i < count; i++)
                        {
                            UnityInputSystem.QueueStateEvent(gamepad, new GamepadState { buttons = 1 });
                            UnityInputSystem.Update();
                            UnityInputSystem.QueueStateEvent(gamepad, new GamepadState());
                            UnityInputSystem.Update();
                        }
                    }

                    detector.Release();
                    return (callbackCount == 0).Label(
                        $"deviceType={deviceType}, count={count}, callbackCount={callbackCount} (expected 0)");
                }
                finally
                {
                    fixture.TearDown();
                }
            }).QuickCheckThrowOnFailure();
        }
    }
}
