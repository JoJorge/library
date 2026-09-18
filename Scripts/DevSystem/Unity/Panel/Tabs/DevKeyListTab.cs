using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace DevSystem.Unity
{
    /// <summary>
    /// 內建的開發者按鍵列表分頁，列出目前所有已註冊的 <see cref="DevKeyRegistration"/>，
    /// 依綁定按鍵路徑以 ordinal 字典序升冪排序，逐筆顯示綁定按鍵與功能敘述（需求 8.1~8.5）。
    /// </summary>
    /// <remarks>
    /// 每次 <see cref="OnShown"/> 皆重新讀取 <see cref="DevKeyBinder.GetRegistrations"/> 的最新清單並填入內容區，
    /// 以反映當下最新狀態（需求 8.5）。清單為空時顯示提示訊息並顯示零筆項目（需求 8.4）。
    /// </remarks>
    public class DevKeyListTab : IDevPanelTab
    {
        /// <summary>分頁標題。</summary>
        private const string TabTitle = "Dev Keys";

        /// <summary>清單為空時顯示的提示訊息（需求 8.4）。</summary>
        private const string EmptyMessage = "目前沒有任何已註冊的開發者按鍵。";

        /// <summary>單筆項目中綁定按鍵與功能敘述之間的分隔字串。</summary>
        private const string EntrySeparator = " — ";

#if DEV_MODE
        private readonly DevKeyBinder keyBinder;
        private VisualElement contentRoot;
        private VisualElement listContainer;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="DevKeyListTab"/> class.
        /// </summary>
        /// <param name="keyBinder">提供已註冊開發者按鍵清單的按鍵綁定器。</param>
        public DevKeyListTab(DevKeyBinder keyBinder)
        {
#if DEV_MODE
            this.keyBinder = keyBinder;
#endif
        }

        /// <inheritdoc/>
        public string Title
        {
#if DEV_MODE
            get { return TabTitle; }
#else
            get { return default; }
#endif
        }

        /// <inheritdoc/>
        public VisualElement BuildContent()
        {
#if DEV_MODE
            this.contentRoot = new VisualElement();
            this.listContainer = new VisualElement();
            this.contentRoot.Add(this.listContainer);
            return this.contentRoot;
#else
            return default;
#endif
        }

        /// <inheritdoc/>
        public void OnShown()
        {
#if DEV_MODE
            this.Refresh();
#endif
        }

        /// <inheritdoc/>
        public void OnUpdate()
        {
#if DEV_MODE
            // 此分頁以 OnShown 刷新即可，更新週期不需額外處理。
#endif
        }

#if DEV_MODE
        /// <summary>
        /// 重新讀取最新的 <see cref="DevKeyRegistration"/> 清單，依綁定按鍵路徑以 ordinal 升冪排序後填入內容區。
        /// 空清單時顯示提示訊息並保持零筆項目（需求 8.2、8.3、8.4、8.5）。
        /// </summary>
        private void Refresh()
        {
            if (this.listContainer == null)
            {
                return;
            }

            this.listContainer.Clear();

            IReadOnlyList<DevKeyRegistration> registrations = this.keyBinder.GetRegistrations();
            if (registrations == null || registrations.Count == 0)
            {
                this.listContainer.Add(new Label(EmptyMessage));
                return;
            }

            // 以穩定排序（LINQ OrderBy）依 BindingPath 之 ordinal 升冪排序（需求 8.3）；
            // 穩定排序確保相同 BindingPath 的多筆項目維持其原始（註冊）相對順序，顯示具決定性。
            IEnumerable<DevKeyRegistration> sorted =
                registrations.OrderBy(r => r.BindingPath, StringComparer.Ordinal);

            foreach (DevKeyRegistration registration in sorted)
            {
                this.listContainer.Add(
                    new Label(registration.BindingPath + EntrySeparator + registration.Description));
            }
        }
#endif
    }
}
