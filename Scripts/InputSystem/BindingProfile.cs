namespace InputSystem
{
    /// <summary>
    /// 綁定設定檔，儲存特定裝置類型的完整按鍵綁定配置。
    /// </summary>
    public class BindingProfile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingProfile"/> class.
        /// </summary>
        /// <param name="deviceType">裝置類型。</param>
        /// <param name="overridesJson">綁定覆寫的 JSON 字串。</param>
        public BindingProfile(InputDeviceType deviceType, string overridesJson)
        {
            this.DeviceType = deviceType;
            this.OverridesJson = overridesJson;
        }

        /// <summary>
        /// Gets 此設定檔對應的裝置類型。
        /// </summary>
        public InputDeviceType DeviceType { get; }

        /// <summary>
        /// Gets or sets 綁定覆寫的 JSON 字串。
        /// </summary>
        public string OverridesJson { get; set; }
    }
}
