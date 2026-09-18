using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace InputSystem
{
    /// <summary>
    /// 裝置偵測器，負責即時偵測最後使用的輸入裝置來源。
    /// 透過 InputSystem.onEvent 監聽所有低階輸入事件。
    /// </summary>
    public class DeviceDetector
    {
        private readonly float _mouseMoveThreshold;
        private readonly float _gamepadAxisThreshold;
        private Action<InputDeviceType> _onDeviceChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDetector"/> class.
        /// </summary>
        /// <param name="mouseMoveThreshold">滑鼠移動距離閥值。</param>
        /// <param name="gamepadAxisThreshold">搖桿類比軸位移閥值。</param>
        public DeviceDetector(float mouseMoveThreshold = 0.1f, float gamepadAxisThreshold = 0.2f)
        {
            this._mouseMoveThreshold = mouseMoveThreshold;
            this._gamepadAxisThreshold = gamepadAxisThreshold;
        }

        /// <summary>
        /// Gets 目前偵測到的輸入裝置類型。
        /// </summary>
        public InputDeviceType CurrentDeviceType { get; private set; } = InputDeviceType.KeyboardMouse;

        /// <summary>
        /// 註冊裝置切換事件回呼。
        /// </summary>
        /// <param name="callback">切換時呼叫的回呼。</param>
        public void RegisterDeviceChangedCallback(Action<InputDeviceType> callback)
        {
            this._onDeviceChanged += callback;
        }

        /// <summary>
        /// 取消註冊裝置切換事件回呼。
        /// </summary>
        /// <param name="callback">要取消的回呼。</param>
        public void UnregisterDeviceChangedCallback(Action<InputDeviceType> callback)
        {
            this._onDeviceChanged -= callback;
        }

        /// <summary>
        /// 初始化裝置偵測器，訂閱 InputSystem.onEvent。
        /// </summary>
        public void Init()
        {
            UnityEngine.InputSystem.InputSystem.onEvent += this.OnInputEvent;
        }

        /// <summary>
        /// 釋放裝置偵測器，取消訂閱 InputSystem.onEvent。
        /// </summary>
        public void Release()
        {
            UnityEngine.InputSystem.InputSystem.onEvent -= this.OnInputEvent;
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            {
                return;
            }

            InputDeviceType? detectedType = this.DetermineDeviceType(eventPtr, device);
            if (!detectedType.HasValue)
            {
                return;
            }

            if (detectedType.Value == this.CurrentDeviceType)
            {
                return;
            }

            this.CurrentDeviceType = detectedType.Value;
            this._onDeviceChanged?.Invoke(this.CurrentDeviceType);
        }

        private InputDeviceType? DetermineDeviceType(InputEventPtr eventPtr, InputDevice device)
        {
            if (device is Keyboard)
            {
                return InputDeviceType.KeyboardMouse;
            }

            if (device is Mouse mouse)
            {
                return this.IsSignificantMouseInput(eventPtr, mouse)
                    ? InputDeviceType.KeyboardMouse
                    : null;
            }

            if (device is Gamepad gamepad)
            {
                return this.IsSignificantGamepadInput(eventPtr, gamepad)
                    ? InputDeviceType.Gamepad
                    : null;
            }

            return null;
        }

        private bool IsSignificantMouseInput(InputEventPtr eventPtr, Mouse mouse)
        {
            // 按鍵按下或滾輪滾動視為有效輸入
            if (mouse.leftButton.ReadValueFromEvent(eventPtr) > 0f ||
                mouse.rightButton.ReadValueFromEvent(eventPtr) > 0f ||
                mouse.middleButton.ReadValueFromEvent(eventPtr) > 0f)
            {
                return true;
            }

            var scroll = mouse.scroll.ReadValueFromEvent(eventPtr);
            if (scroll.sqrMagnitude > 0f)
            {
                return true;
            }

            // 滑鼠移動距離超過閥值
            var delta = mouse.delta.ReadValueFromEvent(eventPtr);
            return delta.sqrMagnitude > this._mouseMoveThreshold * this._mouseMoveThreshold;
        }

        private bool IsSignificantGamepadInput(InputEventPtr eventPtr, Gamepad gamepad)
        {
            // 按鍵按下
            if (gamepad.buttonSouth.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.buttonNorth.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.buttonEast.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.buttonWest.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.startButton.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.selectButton.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.leftShoulder.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.rightShoulder.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.leftTrigger.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.rightTrigger.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.dpad.up.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.dpad.down.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.dpad.left.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.dpad.right.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.leftStickButton.ReadValueFromEvent(eventPtr) > 0f ||
                gamepad.rightStickButton.ReadValueFromEvent(eventPtr) > 0f)
            {
                return true;
            }

            // 類比軸位移超過閥值
            var leftStick = gamepad.leftStick.ReadValueFromEvent(eventPtr);
            if (leftStick.sqrMagnitude > this._gamepadAxisThreshold * this._gamepadAxisThreshold)
            {
                return true;
            }

            var rightStick = gamepad.rightStick.ReadValueFromEvent(eventPtr);
            return rightStick.sqrMagnitude > this._gamepadAxisThreshold * this._gamepadAxisThreshold;
        }
    }
}
