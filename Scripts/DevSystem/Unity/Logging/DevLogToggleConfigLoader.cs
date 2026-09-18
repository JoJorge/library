using System;
using System.Collections.Generic;

namespace DevSystem.Unity
{
    /// <summary>
    /// 預設 <see cref="ILogToggleConfigLoader"/> 實作。包裹一個 <see cref="DevLogToggleConfig"/> 實例，
    /// 於 <see cref="Load"/> 中將其啟用清單映射為涵蓋所有 <see cref="LogCategory"/> 的
    /// <see cref="IReadOnlyDictionary{TKey, TValue}"/>。
    /// </summary>
    /// <remarks>
    /// 啟用清單為權威來源：清單內的分類＝啟用、清單外的分類＝停用，且回傳的對映涵蓋所有已定義的
    /// <see cref="LogCategory"/> 成員。
    /// </remarks>
    public class DevLogToggleConfigLoader : ILogToggleConfigLoader
    {
        private readonly DevLogToggleConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevLogToggleConfigLoader"/> class.
        /// </summary>
        /// <param name="config">開關設定 ScriptableObject。</param>
        public DevLogToggleConfigLoader(DevLogToggleConfig config)
        {
            _config = config;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<LogCategory, bool> Load()
        {
            HashSet<LogCategory> enabled = new HashSet<LogCategory>(_config.EnabledCategories);
            Dictionary<LogCategory, bool> map = new Dictionary<LogCategory, bool>();

            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                map[category] = enabled.Contains(category);
            }

            return map;
        }
    }
}
