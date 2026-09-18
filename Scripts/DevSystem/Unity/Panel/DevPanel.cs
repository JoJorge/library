using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DevSystem.Unity
{
    /// <summary>
    /// 開發者面板，以 UI Toolkit 呈現分頁與子分頁樹。
    /// 於建構時依 <see cref="PanelDefinition"/> 宣告式建立結構，不提供執行期公開變更 API（需求 7.5）。
    /// 純邏輯的建樹與驗證委派 <see cref="PanelTreeBuilder"/>，本類別僅負責 UI Toolkit 呈現與資源管理。
    /// </summary>
    public class DevPanel
    {
        /// <summary>面板階層深度下限（頂層分頁數量下限）。</summary>
        public const int MinDepth = 1;

        /// <summary>面板階層深度上限。</summary>
        public const int MaxDepth = 10;

        /// <summary>頂層分頁數量上限。</summary>
        public const int MaxTabCount = 100;

#if DEV_MODE
        private readonly List<PanelNode> roots = new List<PanelNode>();
        private VisualElement rootElement;
        private VisualElement contentArea;
#endif

        /// <summary>
        /// 依巢狀 <see cref="PanelDefinition"/> 建構面板結構。
        /// 建樹與驗證委派 <see cref="PanelTreeBuilder.TryBuild"/>；成功才建立 UI Toolkit 呈現並保存頂層節點，
        /// 失敗回傳 false 且保留既有結構（不清空既有樹，需求 6.5、7.1、7.2、7.3）。
        /// </summary>
        /// <param name="definition">巢狀面板結構定義（Nodes 為頂層 Panel_Tabs，依清單順序）。</param>
        /// <returns>建構成功回傳 true；否則回傳 false 並保留既有結構。</returns>
        public bool Build(PanelDefinition definition)
        {
#if DEV_MODE
            if (!PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> built))
            {
                // 驗證失敗：保留既有結構與 UI，不做任何變更（需求 7.3）。
                return false;
            }

            // 驗證成功才重建：先釋放既有 UI 資源，再保存新的頂層節點並建立呈現。
            this.ReleaseInternal();

            this.roots.AddRange(built);

            this.rootElement = new VisualElement();
            this.contentArea = new VisualElement();

            VisualElement tabBar = this.BuildSelector(this.roots);
            this.rootElement.Add(tabBar);
            this.rootElement.Add(this.contentArea);

            return true;
#else
            return default;
#endif
        }

        /// <summary>
        /// 釋放面板 UI Toolkit 資源並清除內部頂層節點參考（需求 10.3、10.4）。
        /// </summary>
        /// <returns>完整釋放回傳 true。</returns>
        public bool Release()
        {
#if DEV_MODE
            this.ReleaseInternal();
            return true;
#else
            return default;
#endif
        }

        /// <summary>
        /// 取得目前所有頂層 Panel_Tab 節點（依顯示順序）。
        /// </summary>
        /// <returns>頂層節點清單；尚未建構時為空清單。</returns>
        public IReadOnlyList<PanelNode> GetTabs()
        {
#if DEV_MODE
            return new List<PanelNode>(this.roots);
#else
            return default;
#endif
        }

#if DEV_MODE
        /// <summary>
        /// 釋放已建立的 UI Toolkit 元素並清空內部頂層節點參考。
        /// </summary>
        private void ReleaseInternal()
        {
            this.rootElement?.RemoveFromHierarchy();
            this.rootElement = null;
            this.contentArea = null;
            this.roots.Clear();
        }

        /// <summary>
        /// 為指定同層節點建立分頁選擇器；選取節點時依導覽顯示規則呈現下一層或葉內容
        /// （需求 6.2、6.3、6.8）。
        /// </summary>
        /// <param name="nodes">要呈現為選擇器的同層節點清單（依顯示順序）。</param>
        /// <returns>承載各節點選擇按鈕的容器元素。</returns>
        private VisualElement BuildSelector(IReadOnlyList<PanelNode> nodes)
        {
            var selector = new VisualElement();

            foreach (PanelNode node in nodes)
            {
                PanelNode captured = node;
                var button = new Button(() => this.SelectNode(captured))
                {
                    text = captured.Title,
                };
                selector.Add(button);
            }

            return selector;
        }

        /// <summary>
        /// 選取節點時的導覽顯示：容器節點（Children 非空）顯示子分頁選擇器，
        /// 葉節點呼叫其 Tab 的 <see cref="IDevPanelTab.BuildContent"/> 掛入內容區（需求 6.2、6.3、6.8）。
        /// </summary>
        /// <param name="node">被選取的節點。</param>
        private void SelectNode(PanelNode node)
        {
            this.contentArea.Clear();

            if (node.Children.Count > 0)
            {
                // 容器節點：顯示下一層子分頁選擇器。
                this.contentArea.Add(this.BuildSelector(node.Children));
                return;
            }

            // 葉節點：Tab 以 object 承載，Unity 層轉型為 IDevPanelTab 使用。
            if (node.Tab is IDevPanelTab tab)
            {
                this.contentArea.Add(tab.BuildContent());
                tab.OnShown();
            }
        }
#endif
    }
}
