using System.Collections.Generic;

namespace DevSystem
{
    /// <summary>
    /// 建構後的面板節點樹。Depth 由巢狀層級推導（頂層為 1），Children 順序即定義的清單順序；
    /// Kind 由是否有子節點推導：有子節點為 SubTabContainer，否則為 Leaf。
    /// Tab 以 object 承載，實際型別為 Unity assembly 的 IDevPanelTab（見任務 6.1）。
    /// </summary>
    public class PanelNode
    {
        private readonly List<PanelNode> children = new List<PanelNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PanelNode"/> class。
        /// Kind 由傳入的子節點是否為空推導。
        /// </summary>
        /// <param name="depth">節點在樹中的深度（頂層為 1）。</param>
        /// <param name="title">節點標題。</param>
        /// <param name="tab">葉節點內容分頁（實為 IDevPanelTab，以 object 承載）；容器節點為 null。</param>
        /// <param name="children">子節點清單（保留定義的 Children 清單順序）。</param>
        public PanelNode(
            int depth,
            string title,
            object tab,
            IReadOnlyList<PanelNode> children)
        {
            this.Depth = depth;
            this.Title = title;
            this.Tab = tab;

            if (children != null)
            {
                this.children.AddRange(children);
            }

            this.Kind = this.children.Count > 0 ? PanelNodeKind.SubTabContainer : PanelNodeKind.Leaf;
        }

        /// <summary>Gets 節點在樹中的深度（頂層 Panel_Tab 為 1，由巢狀層級推導）。</summary>
        public int Depth { get; }

        /// <summary>Gets 節點種類（容器或葉），由是否有子節點推導。</summary>
        public PanelNodeKind Kind { get; }

        /// <summary>Gets 節點標題。</summary>
        public string Title { get; }

        /// <summary>Gets 葉節點內容分頁（實為 IDevPanelTab，以 object 承載）；容器節點為 null。</summary>
        public object Tab { get; }

        /// <summary>Gets 子節點清單（保留定義的 Children 清單順序）。</summary>
        public IReadOnlyList<PanelNode> Children => this.children;
    }
}
