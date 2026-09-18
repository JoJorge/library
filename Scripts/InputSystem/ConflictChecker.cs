using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    /// <summary>
    /// 衝突檢查器，負責檢查按鍵綁定是否存在衝突。
    /// 僅在相同 InputContext 與相同 InputDeviceType 範圍內判定衝突。
    /// </summary>
    public class ConflictChecker
    {
        private InputActionAsset _inputActionAsset;
        private Dictionary<InputContext, AbsContextHandler> _contextHandlers;

        /// <summary>
        /// 衝突被偵測到時觸發的事件。
        /// 參數為觸發綁定的動作列舉值與衝突動作列舉值清單。
        /// </summary>
        public event Action<Enum, IReadOnlyList<Enum>> OnConflictDetected;

        /// <summary>
        /// 初始化衝突檢查器。
        /// </summary>
        /// <param name="asset">Unity InputActionAsset 資產。</param>
        /// <param name="contextHandlers">已註冊的情境處理器字典。</param>
        public void Init(InputActionAsset asset, Dictionary<InputContext, AbsContextHandler> contextHandlers)
        {
            this._inputActionAsset = asset;
            this._contextHandlers = contextHandlers;
        }

        /// <summary>
        /// 釋放衝突檢查器資源。
        /// </summary>
        public void Release()
        {
            this._inputActionAsset = null;
            this._contextHandlers = null;
            this.OnConflictDetected = null;
        }

        /// <summary>
        /// 檢查指定 Action 與 Binding Path 是否存在衝突。
        /// 在同 InputContext 同 DeviceType 下比對 Binding Path 字串完全相等。
        /// </summary>
        /// <param name="action">要檢查的動作列舉值（需標註 [InputActionEnum]）。</param>
        /// <param name="bindingPath">要檢查的按鍵路徑。</param>
        /// <param name="deviceType">裝置類型。</param>
        /// <returns>衝突清單；無衝突時回傳空集合；Action 未註冊時回傳 null。</returns>
        public virtual IReadOnlyList<BindingConflict> CheckConflict(
            Enum action,
            string bindingPath,
            InputDeviceType deviceType)
        {
            if (!this.ValidateAction(action))
            {
                return null;
            }

            if (!this.ValidateDeviceType(deviceType))
            {
                return Array.Empty<BindingConflict>();
            }

            if (this._inputActionAsset == null)
            {
                Debug.LogWarning("[InputSystem] InputActionAsset 未初始化");
                return Array.Empty<BindingConflict>();
            }

            InputContext context = ActionEnumResolver.GetContext(action);

            // Req 3.6: 未註冊的 Context 回傳 null 表示錯誤
            if (this._contextHandlers == null || !this._contextHandlers.ContainsKey(context))
            {
                Debug.LogWarning($"[InputSystem] Action 所屬 Context {context} 未註冊任何 Handler");
                return null;
            }

            if (string.IsNullOrEmpty(bindingPath))
            {
                return Array.Empty<BindingConflict>();
            }

            string actionName = ActionEnumResolver.Resolve(action);
            string groupFilter = GetBindingGroup(deviceType);
            var conflictingActions = new List<Enum>();

            // 掃描同 Context 下所有 Action，找出有相同 bindingPath 的其他 Action
            foreach (var actionMap in this._inputActionAsset.actionMaps)
            {
                foreach (var inputAction in actionMap.actions)
                {
                    // 跳過自身
                    if (inputAction.name == actionName)
                    {
                        continue;
                    }

                    // 檢查此 action 是否屬於同一 context
                    Enum otherActionEnum = this.FindActionEnumInContext(inputAction.name, context);
                    if (otherActionEnum == null)
                    {
                        continue;
                    }

                    // 檢查此 action 是否有匹配的 binding path
                    var bindings = inputAction.bindings;
                    for (int i = 0; i < bindings.Count; i++)
                    {
                        if (!this.IsBindingMatchingDevice(bindings[i], deviceType, groupFilter))
                        {
                            continue;
                        }

                        string effectivePath = bindings[i].overridePath ?? bindings[i].path;
                        if (string.Equals(effectivePath, bindingPath, StringComparison.Ordinal))
                        {
                            conflictingActions.Add(otherActionEnum);
                            break;
                        }
                    }
                }
            }

            if (conflictingActions.Count == 0)
            {
                return Array.Empty<BindingConflict>();
            }

            var result = new List<BindingConflict>
            {
                new BindingConflict(bindingPath, action, conflictingActions, context, deviceType),
            };

            return result;
        }

        /// <summary>
        /// 全域衝突掃描，回傳指定裝置類型下所有存在衝突的 Binding Path 群組。
        /// </summary>
        /// <param name="deviceType">裝置類型。</param>
        /// <returns>所有衝突清單。</returns>
        public IReadOnlyList<BindingConflict> ScanAllConflicts(InputDeviceType deviceType)
        {
            if (!this.ValidateDeviceType(deviceType))
            {
                return Array.Empty<BindingConflict>();
            }

            if (this._inputActionAsset == null || this._contextHandlers == null)
            {
                return Array.Empty<BindingConflict>();
            }

            string groupFilter = GetBindingGroup(deviceType);
            var results = new List<BindingConflict>();

            // 針對每個已註冊的 Context 掃描衝突
            foreach (var kvp in this._contextHandlers)
            {
                InputContext context = kvp.Key;
                var pathToActions = new Dictionary<string, List<Enum>>();

                // 收集此 Context 下所有 Action 的 binding paths
                foreach (var actionMap in this._inputActionAsset.actionMaps)
                {
                    foreach (var inputAction in actionMap.actions)
                    {
                        Enum actionEnum = this.FindActionEnumInContext(inputAction.name, context);
                        if (actionEnum == null)
                        {
                            continue;
                        }

                        var bindings = inputAction.bindings;
                        for (int i = 0; i < bindings.Count; i++)
                        {
                            if (!this.IsBindingMatchingDevice(bindings[i], deviceType, groupFilter))
                            {
                                continue;
                            }

                            string effectivePath = bindings[i].overridePath ?? bindings[i].path;
                            if (string.IsNullOrEmpty(effectivePath))
                            {
                                continue;
                            }

                            if (!pathToActions.ContainsKey(effectivePath))
                            {
                                pathToActions[effectivePath] = new List<Enum>();
                            }

                            pathToActions[effectivePath].Add(actionEnum);
                        }
                    }
                }

                // 找出有多個 Action 綁定到同一 path 的衝突群組
                foreach (var entry in pathToActions)
                {
                    if (entry.Value.Count <= 1)
                    {
                        continue;
                    }

                    // 對每個涉及衝突的 action，建立一個 BindingConflict
                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        var conflicting = new List<Enum>();
                        for (int j = 0; j < entry.Value.Count; j++)
                        {
                            if (i != j)
                            {
                                conflicting.Add(entry.Value[j]);
                            }
                        }

                        results.Add(new BindingConflict(
                            entry.Key,
                            entry.Value[i],
                            conflicting,
                            context,
                            deviceType));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 通知衝突事件。用於動態綁定完成後外部呼叫觸發衝突檢查與事件通知。
        /// </summary>
        /// <param name="action">觸發綁定的動作列舉值。</param>
        /// <param name="bindingPath">綁定的按鍵路徑。</param>
        /// <param name="deviceType">裝置類型。</param>
        public void NotifyConflict(Enum action, string bindingPath, InputDeviceType deviceType)
        {
            var conflicts = this.CheckConflict(action, bindingPath, deviceType);
            if (conflicts == null || conflicts.Count == 0)
            {
                return;
            }

            var conflictingActions = new List<Enum>();
            foreach (var conflict in conflicts)
            {
                foreach (var conflicting in conflict.ConflictingActions)
                {
                    conflictingActions.Add(conflicting);
                }
            }

            this.OnConflictDetected?.Invoke(action, conflictingActions);
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

        private bool IsBindingMatchingDevice(InputBinding binding, InputDeviceType deviceType, string groupFilter)
        {
            if (!string.IsNullOrEmpty(binding.groups))
            {
                return binding.groups.Contains(groupFilter);
            }

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

        /// <summary>
        /// 在指定 Context 下尋找對應 action name 的列舉值。
        /// </summary>
        private Enum FindActionEnumInContext(string actionName, InputContext context)
        {
            // 遍歷所有已知的 action enum 型別，找到屬於指定 context 且名稱匹配的列舉值
            foreach (var enumType in this.GetActionEnumTypes())
            {
                var attr = (InputActionEnumAttribute)Attribute.GetCustomAttribute(
                    enumType, typeof(InputActionEnumAttribute));
                if (attr == null || attr.Context != context)
                {
                    continue;
                }

                foreach (Enum value in Enum.GetValues(enumType))
                {
                    if (value.ToString() == actionName)
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 取得所有標註 [InputActionEnum] 的列舉型別。
        /// </summary>
        private IEnumerable<Type> GetActionEnumTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type.IsEnum && Attribute.IsDefined(type, typeof(InputActionEnumAttribute)))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
