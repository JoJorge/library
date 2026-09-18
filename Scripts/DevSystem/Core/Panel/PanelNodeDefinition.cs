using System.Collections.Generic;

namespace DevSystem
{
    /// <summary>
    /// 巢狀的面板節點定義。子節點以 Children 清單直接表示，顯示順序即清單順序；
    /// 葉節點提供 Tab 內容，含子節點的容器節點可將 Tab 留為 null。
    /// Tab 以 object 承載，實際型別為 Unity assembly 的 IDevPanelTab（見任務 6.1），
    /// 以維持純邏輯層不相依 UI Toolkit。
    /// </summary>
    public class PanelNodeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelNodeDefinition"/> class.
        /// </summary>
        /// <param name="title">節點標題。</param>
        /// <param name="tab">葉節點內容分頁（實為 IDevPanelTab，以 object 承載）；含子節點的容器節點可為 null。</param>
        /// <param name="children">子節點定義清單；清單順序即顯示順序（無子節點則為空）。</param>
        public PanelNodeDefinition(
            string title,
            object tab,
            IReadOnlyList<PanelNodeDefinition> children)
        {
            this.Title = title;
            this.Tab = tab;
            this.Children = children ?? new List<PanelNodeDefinition>();
        }

        /// <summary>Gets 節點標題。</summary>
        public string Title { get; }

        /// <summary>Gets 葉節點內容分頁（實為 IDevPanelTab，以 object 承載）；含子節點的容器節點可為 null。</summary>
        public object Tab { get; }

        /// <summary>Gets 子節點定義清單；清單順序即顯示順序（無子節點則為空）。</summary>
        public IReadOnlyList<PanelNodeDefinition> Children { get; }
    }
}
