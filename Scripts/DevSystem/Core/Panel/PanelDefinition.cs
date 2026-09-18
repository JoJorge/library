using System.Collections.Generic;

namespace DevSystem
{
    /// <summary>
    /// 宣告式面板結構定義，於 DevPanel 建構時提供。
    /// Nodes 為頂層 Panel_Tabs（清單順序即顯示順序），各節點以巢狀 Children 表達子分頁。
    /// 深度上限由 DevPanel.MaxDepth 固定常數決定，定義不再攜帶最大深度。
    /// </summary>
    public class PanelDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelDefinition"/> class.
        /// </summary>
        /// <param name="nodes">頂層節點定義清單（Panel_Tabs，清單順序即顯示順序）。</param>
        public PanelDefinition(IReadOnlyList<PanelNodeDefinition> nodes)
        {
            this.Nodes = nodes ?? new List<PanelNodeDefinition>();
        }

        /// <summary>Gets 頂層節點定義清單（Panel_Tabs，清單順序即顯示順序）。</summary>
        public IReadOnlyList<PanelNodeDefinition> Nodes { get; }
    }
}
