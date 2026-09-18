using System;
using System.Collections.Generic;

namespace DevSystem
{
    /// <summary>
    /// 開發者日誌元件，提供帶分類前綴的除錯日誌輸出，並維護各分類的暫存開關狀態。
    /// </summary>
    /// <remarks>
    /// 所有輸出皆依 Log_Message_Format 格式化為 <c>[Dev][分類] MSG</c>。開關狀態僅維護於記憶體，
    /// 永不寫回設定檔或任何持久化儲存。<see cref="Log"/> 以 <c>DEV_MODE</c> 條件編譯符號閘控，
    /// 未定義時呼叫端的呼叫點會被完全移除。
    /// </remarks>
    public class DevLogger
    {
        /// <summary>
        /// 單筆日誌訊息允許的最大字元數，超過此上限的訊息會被截斷。
        /// </summary>
        public const int MaxMessageLength = 4096;

        /// <summary>
        /// 當傳入的分類值未對應任何已定義列舉成員時，用於格式化前綴的固定預設分類名稱。
        /// </summary>
        private const string DefaultCategoryName = "General";

        private readonly Dictionary<LogCategory, bool> _toggles;
        private readonly ILogSink _sink;
        private bool _configLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevLogger"/> class.
        /// </summary>
        /// <param name="sink">承接已格式化日誌字串的底層輸出目的地。</param>
        public DevLogger(ILogSink sink)
        {
            _toggles = new Dictionary<LogCategory, bool>();
            _sink = sink;
            _configLoaded = false;
        }

        /// <summary>
        /// 依指定分類與訊息輸出一筆除錯日誌。
        /// </summary>
        /// <param name="category">日誌分類，用於閘控與格式化前綴。</param>
        /// <param name="message">日誌訊息內容，允許為 <see langword="null"/> 或空字串。</param>
        [System.Diagnostics.Conditional("DEV_MODE")]
        public void Log(LogCategory category, string message)
        {
#if DEV_MODE
            if (!IsCategoryEnabled(category))
            {
                return;
            }

            string name = Enum.IsDefined(typeof(LogCategory), category)
                ? category.ToString()
                : DefaultCategoryName;

            string msg = message ?? string.Empty;

            if (msg.Length > MaxMessageLength)
            {
                msg = msg.Substring(0, MaxMessageLength);
            }

            string formatted = $"[Dev][{name}] {msg}";

            try
            {
                _sink.Write(formatted);
            }
            catch (Exception)
            {
                // 底層輸出失敗時抑制該筆日誌，不影響呼叫端（Req 2.8）。
            }
#endif
        }

        /// <summary>
        /// 設定指定分類的暫存開關狀態。
        /// </summary>
        /// <param name="category">要調整的日誌分類。</param>
        /// <param name="enabled"><see langword="true"/> 表示啟用、<see langword="false"/> 表示停用。</param>
        /// <returns>設定成功時回傳 <see langword="true"/>。</returns>
        public virtual bool SetCategoryEnabled(LogCategory category, bool enabled)
        {
#if DEV_MODE
            _toggles[category] = enabled;
            return true;
#else
            return default;
#endif
        }

        /// <summary>
        /// 查詢指定分類目前的暫存開關狀態。
        /// </summary>
        /// <param name="category">要查詢的日誌分類。</param>
        /// <returns>啟用時回傳 <see langword="true"/>；設定尚未載入或無對應項目時視為啟用並回傳 <see langword="true"/>。</returns>
        public virtual bool IsCategoryEnabled(LogCategory category)
        {
#if DEV_MODE
            if (!_configLoaded)
            {
                return true;
            }

            if (_toggles.TryGetValue(category, out bool enabled))
            {
                return enabled;
            }

            return true;
#else
            return default;
#endif
        }

        /// <summary>
        /// 取得所有已定義的日誌分類清單。
        /// </summary>
        /// <returns>涵蓋所有已定義 <see cref="LogCategory"/> 成員的唯讀清單。</returns>
        public virtual IReadOnlyList<LogCategory> GetCategories()
        {
#if DEV_MODE
            LogCategory[] values = (LogCategory[])Enum.GetValues(typeof(LogCategory));
            return new List<LogCategory>(values);
#else
            return default;
#endif
        }

        /// <summary>
        /// 透過指定載入器讀取 Log_Toggle_Config，並以其內容重建各分類的暫存開關狀態。
        /// </summary>
        /// <param name="loader">提供各分類初始開關狀態的設定載入器。</param>
        public void LoadToggleConfig(ILogToggleConfigLoader loader)
        {
#if DEV_MODE
            IReadOnlyDictionary<LogCategory, bool> config = loader.Load();

            _toggles.Clear();

            if (config != null)
            {
                foreach (KeyValuePair<LogCategory, bool> entry in config)
                {
                    _toggles[entry.Key] = entry.Value;
                }
            }

            _configLoaded = true;
#endif
        }
    }
}
