using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    /// <summary>
    /// 按鍵綁定管理器，負責動態按鍵綁定與綁定資料的儲存管理。
    /// </summary>
    public class BindingManager
    {
        private InputActionAsset _inputActionAsset;
        private IBindingStorage _storage;
        private ConflictChecker _conflictChecker;
        private readonly Dictionary<InputDeviceType, BindingProfile> _profiles
            = new Dictionary<InputDeviceType, BindingProfile>();

        /// <summary>
        /// 初始化綁定管理器，載入資產與儲存介面，並自動載入已儲存的綁定設定。
        /// </summary>
        /// <param name="asset">Unity InputActionAsset 資產。</param>
        /// <param name="storage">綁定儲存介面實作。</param>
        public void Init(InputActionAsset asset, IBindingStorage storage)
        {
            this._inputActionAsset = asset;
            this._storage = storage;

            // 啟動時自動載入已儲存的綁定設定（Req 2.8）
            foreach (InputDeviceType deviceType in Enum.GetValues(typeof(InputDeviceType)))
            {
                if (this._storage.Exists(deviceType))
                {
                    string json = this._storage.Load(deviceType);
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            this._inputActionAsset.LoadBindingOverridesFromJson(json);
                            this._profiles[deviceType] = new BindingProfile(deviceType, json);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[InputSystem] 載入 {deviceType} 綁定設定失敗，使用預設值: {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 釋放綁定管理器資源。
        /// </summary>
        public void Release()
        {
            this._inputActionAsset = null;
            this._storage = null;
            this._conflictChecker = null;
            this._profiles.Clear();
        }

        /// <summary>
        /// 設定衝突檢查器參考，使動態綁定完成後自動觸發衝突通知。
        /// </summary>
        /// <param name="checker">衝突檢查器實例，可為 null（將略過衝突檢查）。</param>
        public void SetConflictChecker(ConflictChecker checker)
        {
            this._conflictChecker = checker;
        }

        /// <summary>
        /// 套用指定 Action 的綁定覆寫。同一幀內立即生效。
        /// </summary>
        /// <param name="action">動作列舉值（需標註 [InputActionEnum]）。</param>
        /// <param name="bindingPath">新的 Binding Path。</param>
        /// <param name="deviceType">目標裝置類型。</param>
        public virtual void ApplyBinding(Enum action, string bindingPath, InputDeviceType deviceType)
        {
            if (!this.ValidateAction(action))
            {
                return;
            }

            if (!this.ValidateDeviceType(deviceType))
            {
                return;
            }

            InputAction inputAction = this.FindAction(action);
            if (inputAction == null)
            {
                return;
            }

            string groupFilter = GetBindingGroup(deviceType);
            var bindings = inputAction.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                if (this.IsBindingMatchingDevice(bindings[i], deviceType, groupFilter))
                {
                    inputAction.ApplyBindingOverride(i, bindingPath);
                    this._conflictChecker?.NotifyConflict(action, bindingPath, deviceType);
                    return;
                }
            }
        }

        /// <summary>
        /// 查詢指定 Action 在指定裝置類型下所有綁定的顯示名稱。
        /// </summary>
        /// <param name="action">動作列舉值（需標註 [InputActionEnum]）。</param>
        /// <param name="deviceType">裝置類型。</param>
        /// <returns>顯示名稱清單；Action 無效或無綁定時回傳空集合。</returns>
        public IReadOnlyList<string> GetDisplayNames(Enum action, InputDeviceType deviceType)
        {
            if (!this.ValidateAction(action))
            {
                return Array.Empty<string>();
            }

            if (!this.ValidateDeviceType(deviceType))
            {
                return Array.Empty<string>();
            }

            InputAction inputAction = this.FindAction(action);
            if (inputAction == null)
            {
                return Array.Empty<string>();
            }

            string groupFilter = GetBindingGroup(deviceType);
            var result = new List<string>();
            var bindings = inputAction.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                if (this.IsBindingMatchingDevice(bindings[i], deviceType, groupFilter))
                {
                    string displayString = inputAction.GetBindingDisplayString(i);
                    if (!string.IsNullOrEmpty(displayString))
                    {
                        result.Add(displayString);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 查詢指定 Action 在指定裝置類型下所有綁定的路徑。
        /// </summary>
        /// <param name="action">動作列舉值（需標註 [InputActionEnum]）。</param>
        /// <param name="deviceType">裝置類型。</param>
        /// <returns>Binding Path 清單；Action 無效或無綁定時回傳空集合。</returns>
        public IReadOnlyList<string> GetBindingPaths(Enum action, InputDeviceType deviceType)
        {
            if (!this.ValidateAction(action))
            {
                return Array.Empty<string>();
            }

            if (!this.ValidateDeviceType(deviceType))
            {
                return Array.Empty<string>();
            }

            InputAction inputAction = this.FindAction(action);
            if (inputAction == null)
            {
                return Array.Empty<string>();
            }

            string groupFilter = GetBindingGroup(deviceType);
            var result = new List<string>();
            var bindings = inputAction.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                if (this.IsBindingMatchingDevice(bindings[i], deviceType, groupFilter))
                {
                    string effectivePath = bindings[i].overridePath ?? bindings[i].path;
                    if (!string.IsNullOrEmpty(effectivePath))
                    {
                        result.Add(effectivePath);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 儲存指定裝置類型的綁定設定至儲存媒體。
        /// </summary>
        /// <param name="deviceType">裝置類型。</param>
        /// <returns>儲存成功回傳 true；失敗回傳 false 且保留現有綁定。</returns>
        public bool SaveBindings(InputDeviceType deviceType)
        {
            if (this._inputActionAsset == null || this._storage == null)
            {
                return false;
            }

            if (!this.ValidateDeviceType(deviceType))
            {
                return false;
            }

            string json = this._inputActionAsset.SaveBindingOverridesAsJson();
            if (!this._storage.Save(deviceType, json))
            {
                return false;
            }

            this._profiles[deviceType] = new BindingProfile(deviceType, json);
            return true;
        }

        /// <summary>
        /// 從儲存媒體載入指定裝置類型的綁定設定。
        /// </summary>
        /// <param name="deviceType">裝置類型。</param>
        /// <returns>載入成功回傳 true；失敗時回退至預設值。</returns>
        public bool LoadBindings(InputDeviceType deviceType)
        {
            if (this._inputActionAsset == null || this._storage == null)
            {
                return false;
            }

            if (!this.ValidateDeviceType(deviceType))
            {
                return false;
            }

            string json = this._storage.Load(deviceType);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                this._inputActionAsset.LoadBindingOverridesFromJson(json);
                this._profiles[deviceType] = new BindingProfile(deviceType, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputSystem] 載入 {deviceType} 綁定設定失敗，回退至預設值: {ex.Message}");
                this._inputActionAsset.RemoveAllBindingOverrides();
                return false;
            }
        }

        /// <summary>
        /// 重置指定 Action 的綁定至預設值。
        /// </summary>
        /// <param name="action">動作列舉值（需標註 [InputActionEnum]）。</param>
        /// <param name="deviceType">裝置類型。</param>
        public virtual void ResetBinding(Enum action, InputDeviceType deviceType)
        {
            if (!this.ValidateAction(action))
            {
                return;
            }

            if (!this.ValidateDeviceType(deviceType))
            {
                return;
            }

            InputAction inputAction = this.FindAction(action);
            if (inputAction == null)
            {
                return;
            }

            string groupFilter = GetBindingGroup(deviceType);
            var bindings = inputAction.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                if (this.IsBindingMatchingDevice(bindings[i], deviceType, groupFilter))
                {
                    inputAction.RemoveBindingOverride(i);
                }
            }
        }

        /// <summary>
        /// 重置指定裝置類型所有綁定至預設值。
        /// </summary>
        /// <param name="deviceType">裝置類型。</param>
        public void ResetAllBindings(InputDeviceType deviceType)
        {
            if (!this.ValidateDeviceType(deviceType))
            {
                return;
            }

            if (this._inputActionAsset == null)
            {
                return;
            }

            string groupFilter = GetBindingGroup(deviceType);
            foreach (var actionMap in this._inputActionAsset.actionMaps)
            {
                foreach (var inputAction in actionMap.actions)
                {
                    var bindings = inputAction.bindings;
                    for (int i = 0; i < bindings.Count; i++)
                    {
                        if (this.IsBindingMatchingDevice(bindings[i], deviceType, groupFilter))
                        {
                            inputAction.RemoveBindingOverride(i);
                        }
                    }
                }
            }
        }

        private static string GetBindingGroup(InputDeviceType deviceType)
        {
            switch (deviceType)
            {
                case InputDeviceType.KeyboardMouse:
                    return "Keyboard&Mouse";
                case InputDeviceType.Gamepad:
                    return "Gamepad";
                default:
                    return string.Empty;
            }
        }

        private bool ValidateAction(Enum action)
        {
            if (action == null)
            {
                Debug.LogWarning("[InputSystem] 傳入的 action 為 null");
                return false;
            }

            if (!ActionEnumResolver.Validate(action))
            {
                Debug.LogWarning($"[InputSystem] 列舉型別 {action.GetType().Name} 未標註 [InputActionEnum]");
                return false;
            }

            return true;
        }

        private bool ValidateDeviceType(InputDeviceType deviceType)
        {
            if (!Enum.IsDefined(typeof(InputDeviceType), deviceType))
            {
                Debug.LogWarning($"[InputSystem] 未定義的 InputDeviceType 值: {(int)deviceType}");
                return false;
            }

            return true;
        }

        private InputAction FindAction(Enum action)
        {
            if (this._inputActionAsset == null)
            {
                Debug.LogWarning("[InputSystem] InputActionAsset 未初始化");
                return null;
            }

            string actionName = ActionEnumResolver.Resolve(action);
            InputAction inputAction = this._inputActionAsset.FindAction(actionName);
            if (inputAction == null)
            {
                Debug.LogWarning($"[InputSystem] 找不到 Action: {actionName}");
                return null;
            }

            return inputAction;
        }

        private bool IsBindingMatchingDevice(InputBinding binding, InputDeviceType deviceType, string groupFilter)
        {
            // 若 binding 有 groups 資訊，使用 groups 比對
            if (!string.IsNullOrEmpty(binding.groups))
            {
                return binding.groups.Contains(groupFilter);
            }

            // 若無 groups 資訊，透過 path 或 overridePath 推斷裝置類型
            string effectivePath = binding.overridePath ?? binding.path;
            if (string.IsNullOrEmpty(effectivePath))
            {
                return false;
            }

            switch (deviceType)
            {
                case InputDeviceType.KeyboardMouse:
                    return effectivePath.Contains("<Keyboard>") || effectivePath.Contains("<Mouse>");
                case InputDeviceType.Gamepad:
                    return effectivePath.Contains("<Gamepad>");
                default:
                    return false;
            }
        }
    }
}
