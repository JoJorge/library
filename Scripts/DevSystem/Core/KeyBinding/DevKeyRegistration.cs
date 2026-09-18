using InputSystem;

namespace DevSystem
{
    /// <summary>
    /// 開發者按鍵註冊資訊，記錄某個開發者功能、其綁定按鍵路徑與功能敘述的對應關係。
    /// </summary>
    public class DevKeyRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevKeyRegistration"/> class.
        /// </summary>
        /// <param name="action">開發者動作列舉值。</param>
        /// <param name="bindingPath">綁定按鍵路徑。</param>
        /// <param name="description">功能敘述文字。</param>
        /// <param name="context">所屬輸入情境（衝突範圍）。</param>
        /// <param name="deviceType">所屬裝置類型（衝突範圍）。</param>
        public DevKeyRegistration(
            DevAction action,
            string bindingPath,
            string description,
            InputContext context,
            InputDeviceType deviceType)
        {
            this.Action = action;
            this.BindingPath = bindingPath;
            this.Description = description;
            this.Context = context;
            this.DeviceType = deviceType;
        }

        /// <summary>Gets 開發者動作列舉值。</summary>
        public DevAction Action { get; }

        /// <summary>Gets 綁定按鍵路徑。</summary>
        public string BindingPath { get; }

        /// <summary>Gets 功能敘述文字。</summary>
        public string Description { get; }

        /// <summary>Gets 所屬輸入情境。</summary>
        public InputContext Context { get; }

        /// <summary>Gets 所屬裝置類型。</summary>
        public InputDeviceType DeviceType { get; }
    }
}
