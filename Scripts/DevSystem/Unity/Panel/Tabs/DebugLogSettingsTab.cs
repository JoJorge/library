using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DevSystem.Unity
{
    /// <summary>
    /// 內建分頁：列出各 <see cref="LogCategory"/> 的開關狀態並提供執行期切換。
    /// 切換透過 <see cref="DevLogger"/> API 完成，成功則同影格同步顯示；外部來源變更於下一更新週期同步；
    /// 更新失敗則還原該列顯示狀態並顯示錯誤提示。
    /// </summary>
    public class DebugLogSettingsTab : IDevPanelTab
    {
        /// <summary>分頁標題。</summary>
        private const string TabTitle = "Log Settings";

        /// <summary>分類集合為空時顯示的空狀態訊息（需求 9.3）。</summary>
        private const string EmptyStateMessage = "No log categories available.";

        /// <summary>切換某分類開關更新失敗時顯示的錯誤提示（需求 9.7）。</summary>
        private const string UpdateFailedMessage = "Failed to update log category toggle.";

        private readonly DevLogger _logger;
        private readonly Dictionary<LogCategory, Toggle> _rows = new Dictionary<LogCategory, Toggle>();

#if DEV_MODE
        private Label _errorLabel;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugLogSettingsTab"/> class.
        /// </summary>
        /// <param name="logger">分類日誌元件。</param>
        public DebugLogSettingsTab(DevLogger logger)
        {
            this._logger = logger;
        }

        /// <inheritdoc/>
        public string Title => TabTitle;

        /// <inheritdoc/>
        public VisualElement BuildContent()
        {
#if DEV_MODE
            this._rows.Clear();

            var root = new VisualElement();

            IReadOnlyList<LogCategory> categories = this._logger.GetCategories();

            if (categories == null || categories.Count == 0)
            {
                // 分類集合為空：顯示空狀態訊息且不列出任何項目（需求 9.3）。
                root.Add(new Label(EmptyStateMessage));
                return root;
            }

            foreach (LogCategory category in categories)
            {
                LogCategory captured = category;

                var toggle = new Toggle(captured.ToString())
                {
                    value = this._logger.IsCategoryEnabled(captured),
                };

                toggle.RegisterValueChangedCallback(evt => this.OnToggleChanged(captured, evt.newValue));

                this._rows[captured] = toggle;
                root.Add(toggle);
            }

            // 錯誤提示標籤預設隱藏，僅於更新失敗時顯示（需求 9.7）。
            this._errorLabel = new Label(UpdateFailedMessage)
            {
                style = { display = DisplayStyle.None },
            };
            root.Add(this._errorLabel);

            return root;
#else
            return default;
#endif
        }

        /// <inheritdoc/>
        public void OnShown()
        {
        }

        /// <inheritdoc/>
        public void OnUpdate()
        {
#if DEV_MODE
            // 將各列同步為 DevLogger 最新狀態，以反映外部來源造成的變更（需求 9.6）。
            foreach (KeyValuePair<LogCategory, Toggle> row in this._rows)
            {
                row.Value.SetValueWithoutNotify(this._logger.IsCategoryEnabled(row.Key));
            }
#endif
        }

#if DEV_MODE
        /// <summary>
        /// 使用者切換某列 Toggle 時的處理：透過 <see cref="DevLogger.SetCategoryEnabled"/> 更新。
        /// 成功則同影格將該列同步為 <see cref="DevLogger.IsCategoryEnabled"/> 回報值（需求 9.4、9.5）；
        /// 失敗則還原該列為切換前狀態並顯示錯誤提示（需求 9.7）。
        /// </summary>
        /// <param name="category">被切換的日誌分類。</param>
        /// <param name="target">切換後的目標開關狀態。</param>
        private void OnToggleChanged(LogCategory category, bool target)
        {
            if (!this._rows.TryGetValue(category, out Toggle toggle))
            {
                return;
            }

            bool updated = this._logger.SetCategoryEnabled(category, target);

            if (updated)
            {
                // 更新成功：同影格同步為 DevLogger 回報的最新狀態（需求 9.5）。
                toggle.SetValueWithoutNotify(this._logger.IsCategoryEnabled(category));

                if (this._errorLabel != null)
                {
                    this._errorLabel.style.display = DisplayStyle.None;
                }

                return;
            }

            // 更新失敗：還原該列為切換前狀態並顯示錯誤提示（需求 9.7）。
            toggle.SetValueWithoutNotify(!target);

            if (this._errorLabel != null)
            {
                this._errorLabel.style.display = DisplayStyle.Flex;
            }
        }
#endif
    }
}
