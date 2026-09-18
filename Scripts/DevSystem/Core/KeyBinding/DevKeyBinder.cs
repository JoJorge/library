using System;
using System.Collections.Generic;
using InputSystem;

namespace DevSystem
{
    /// <summary>
    /// 開發者按鍵綁定器，負責向輸入系統註冊、解除開發者按鍵綁定，並於註冊前進行衝突閘控。
    /// </summary>
    /// <remarks>
    /// 註冊前先驗證必要欄位與路徑，再透過 <see cref="ConflictChecker"/> 檢查衝突，僅在無衝突時
    /// 才呼叫 <see cref="BindingManager.ApplyBinding"/> 並記錄一筆 <see cref="DevKeyRegistration"/>。
    /// 所有公開方法皆以 <c>DEV_MODE</c> 條件編譯符號閘控，未定義時回傳 <c>default</c>。
    /// </remarks>
    public class DevKeyBinder
    {
        /// <summary>
        /// 鍵盤裝置的 Binding Path 前綴，用於由路徑推斷 <see cref="InputDeviceType.KeyboardMouse"/>。
        /// </summary>
        private const string KeyboardPathPrefix = "<Keyboard>";

        /// <summary>
        /// 滑鼠裝置的 Binding Path 前綴，用於由路徑推斷 <see cref="InputDeviceType.KeyboardMouse"/>。
        /// </summary>
        private const string MousePathPrefix = "<Mouse>";

        /// <summary>
        /// 搖桿裝置的 Binding Path 前綴，用於由路徑推斷 <see cref="InputDeviceType.Gamepad"/>。
        /// </summary>
        private const string GamepadPathPrefix = "<Gamepad>";

        private readonly Dictionary<DevAction, DevKeyRegistration> _registrations;
        private readonly BindingManager _bindingManager;
        private readonly ConflictChecker _conflictChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevKeyBinder"/> class.
        /// </summary>
        /// <param name="bindingManager">套用與重置按鍵綁定的綁定管理器。</param>
        /// <param name="conflictChecker">於註冊前檢查綁定衝突的衝突檢查器。</param>
        public DevKeyBinder(BindingManager bindingManager, ConflictChecker conflictChecker)
        {
            _registrations = new Dictionary<DevAction, DevKeyRegistration>();
            _bindingManager = bindingManager;
            _conflictChecker = conflictChecker;
        }

        /// <summary>
        /// 註冊一個開發者按鍵綁定。註冊流程依序驗證欄位、路徑、重複性與衝突，僅在通過所有檢查後才套用綁定並記錄。
        /// </summary>
        /// <param name="action">要綁定的開發者動作列舉值。</param>
        /// <param name="bindingPath">目標按鍵路徑（Binding Path）。</param>
        /// <param name="description">功能敘述文字。</param>
        /// <returns>綁定結果；成功時 <see cref="DevKeyBindResult.Success"/> 為 true，失敗時攜帶對應原因與衝突清單。</returns>
        public DevKeyBindResult RegisterDevKey(DevAction action, string bindingPath, string description)
        {
#if DEV_MODE
            // Req 5.5：缺少必要欄位（此處為空/null 的 Binding Path）不做衝突檢查，直接回傳 InvalidRequest。
            if (string.IsNullOrEmpty(bindingPath))
            {
                return DevKeyBindResult.Failure(DevKeyBindFailure.InvalidRequest);
            }

            // Req 4.2 / 5.5：路徑非空但裝置類型無法自路徑前綴推斷 → 路徑無法解析。
            // 此處採用與空路徑不同的分類：空/null 路徑歸為 InvalidRequest（缺欄位），
            // 非空但無法解析為有效裝置路徑者歸為 InvalidPath，兩者皆在衝突檢查前拒絕。
            if (!TryResolveDeviceType(bindingPath, out InputDeviceType deviceType))
            {
                return DevKeyBindResult.Failure(DevKeyBindFailure.InvalidPath);
            }

            // Req 4.4：重複的 DevAction 拒絕註冊並保留既有 Registration 不變。
            if (_registrations.ContainsKey(action))
            {
                return DevKeyBindResult.Failure(DevKeyBindFailure.Duplicate);
            }

            // Req 5.1：於寫入 Registration 之前透過 ConflictChecker 進行衝突檢查。
            InputContext context = ActionEnumResolver.GetContext(action);
            IReadOnlyList<BindingConflict> conflicts =
                _conflictChecker.CheckConflict(action, bindingPath, deviceType);

            // Req 5.3：衝突清單非空時拒絕綁定並回傳相同清單，不套用亦不記錄。
            // null 表示 Context 未註冊 / Action 無效，無法驗證此綁定為安全，同樣視為失敗而不寫入；
            // 以空的衝突清單呈現 Conflict 結果（無法確認為無衝突，故不當作成功處理）。
            if (conflicts == null || conflicts.Count > 0)
            {
                return DevKeyBindResult.ConflictResult(conflicts ?? Array.Empty<BindingConflict>());
            }

            // Req 5.4 / 4.1 / 4.3：無衝突才套用綁定並記錄 Registration。
            _bindingManager.ApplyBinding(action, bindingPath, deviceType);
            _registrations[action] =
                new DevKeyRegistration(action, bindingPath, description, context, deviceType);

            return DevKeyBindResult.Ok();
#else
            return default;
#endif
        }

        /// <summary>
        /// 解除指定開發者動作的按鍵綁定並移除其註冊。
        /// </summary>
        /// <param name="action">要解除綁定的開發者動作列舉值。</param>
        /// <returns>成功解除並移除時回傳 <see langword="true"/>；查無對應註冊時回傳 <see langword="false"/> 且不變更任何綁定。</returns>
        public bool Unbind(DevAction action)
        {
#if DEV_MODE
            // Req 4.8：查無對應 Registration 時不變更任何綁定，回傳 false。
            if (!_registrations.TryGetValue(action, out DevKeyRegistration registration))
            {
                return false;
            }

            // Req 4.7：向 BindingManager 解除綁定（重置回預設）並移除 Registration。
            _bindingManager.ResetBinding(action, registration.DeviceType);
            _registrations.Remove(action);
            return true;
#else
            return default;
#endif
        }

        /// <summary>
        /// 取得目前所有已註冊的開發者按鍵綁定。
        /// </summary>
        /// <returns>所有 <see cref="DevKeyRegistration"/> 的快照清單；呼叫端無法透過此清單變更內部狀態。</returns>
        public IReadOnlyList<DevKeyRegistration> GetRegistrations()
        {
#if DEV_MODE
            // Req 4.9：回傳快照複本，避免呼叫端變更內部字典狀態。
            return new List<DevKeyRegistration>(_registrations.Values);
#else
            return default;
#endif
        }

        /// <summary>
        /// 清除所有開發者按鍵綁定，並解除各自對應的綁定。
        /// </summary>
        /// <returns>全部成功清除時回傳 <see langword="true"/>。</returns>
        public bool ClearAll()
        {
#if DEV_MODE
            // Req 10.3 / 10.4：逐筆解除綁定後清空 Registration。
            bool success = true;
            foreach (DevKeyRegistration registration in _registrations.Values)
            {
                // BindingManager.ResetBinding 無回傳值；此結構保留未來可回報個別失敗的空間。
                _bindingManager.ResetBinding(registration.Action, registration.DeviceType);
            }

            _registrations.Clear();
            return success;
#else
            return default;
#endif
        }

#if DEV_MODE
        /// <summary>
        /// 由 Binding Path 前綴推斷目標裝置類型。
        /// </summary>
        /// <param name="bindingPath">非空的按鍵路徑。</param>
        /// <param name="deviceType">解析出的裝置類型。</param>
        /// <returns>可推斷出已知裝置類型時回傳 <see langword="true"/>；否則回傳 <see langword="false"/>。</returns>
        private static bool TryResolveDeviceType(string bindingPath, out InputDeviceType deviceType)
        {
            if (bindingPath.Contains(KeyboardPathPrefix) || bindingPath.Contains(MousePathPrefix))
            {
                deviceType = InputDeviceType.KeyboardMouse;
                return true;
            }

            if (bindingPath.Contains(GamepadPathPrefix))
            {
                deviceType = InputDeviceType.Gamepad;
                return true;
            }

            deviceType = default;
            return false;
        }
#endif
    }
}
