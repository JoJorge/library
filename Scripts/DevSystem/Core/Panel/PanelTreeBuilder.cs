using System.Collections.Generic;

namespace DevSystem
{
    /// <summary>
    /// 純邏輯的面板樹建構器，依巢狀 <see cref="PanelDefinition"/> 建立 <see cref="PanelNode"/> 樹。
    /// 頂層節點 Depth 為 1，子節點 Depth 為父節點 +1；Kind 由 Children 是否為空推導；
    /// 順序即 Children 清單順序。任一節點深度超過 <see cref="MaxDepth"/>、
    /// 頂層數量超過 <see cref="MaxTabCount"/> 或少於 <see cref="MinDepth"/> 時拒絕建構，
    /// 回傳 false 且不產生任何樹（保留呼叫端既有結構）。
    /// <para>
    /// 深度與數量上限於此以純邏輯常數維護，與 Unity assembly 的 DevPanel 對應常數同值，
    /// 避免純邏輯層相依 Unity 型別（見任務 6.2）。
    /// </para>
    /// </summary>
    public static class PanelTreeBuilder
    {
        /// <summary>面板階層深度下限（頂層分頁數量下限）。</summary>
        public const int MinDepth = 1;

        /// <summary>面板階層深度上限。</summary>
        public const int MaxDepth = 10;

        /// <summary>頂層分頁數量上限。</summary>
        public const int MaxTabCount = 100;

        /// <summary>
        /// 依巢狀定義建立 <see cref="PanelNode"/> 樹。
        /// 驗證失敗時回傳 false、<paramref name="roots"/> 為 null，且不改動任何既有結構。
        /// </summary>
        /// <param name="definition">巢狀面板結構定義（Nodes 為頂層 Panel_Tabs，依清單順序）。</param>
        /// <param name="roots">建構成功時輸出的頂層節點清單；失敗時為 null。</param>
        /// <returns>建構成功回傳 true；驗證失敗回傳 false。</returns>
        public static bool TryBuild(PanelDefinition definition, out IReadOnlyList<PanelNode> roots)
        {
            roots = null;

            if (definition == null)
            {
                return false;
            }

            IReadOnlyList<PanelNodeDefinition> topLevel = definition.Nodes;
            int topLevelCount = topLevel?.Count ?? 0;

            // 頂層分頁數量須落在 [MinDepth, MaxTabCount]（需求 7.1、7.2）。
            if (topLevelCount < MinDepth || topLevelCount > MaxTabCount)
            {
                return false;
            }

            // 先整體驗證深度，任一節點超限即整體拒絕、不保留半成品（需求 6.5、7.3）。
            foreach (PanelNodeDefinition node in topLevel)
            {
                if (!IsDepthValid(node, 1))
                {
                    return false;
                }
            }

            var built = new List<PanelNode>(topLevelCount);
            foreach (PanelNodeDefinition node in topLevel)
            {
                built.Add(BuildNode(node, 1));
            }

            roots = built;
            return true;
        }

        /// <summary>
        /// 遞迴驗證節點及其後代深度是否皆不超過 <see cref="MaxDepth"/>。
        /// </summary>
        /// <param name="node">待驗證節點定義。</param>
        /// <param name="depth">該節點所在深度（頂層為 1）。</param>
        /// <returns>皆未超限回傳 true。</returns>
        private static bool IsDepthValid(PanelNodeDefinition node, int depth)
        {
            if (node == null || depth > MaxDepth)
            {
                return false;
            }

            IReadOnlyList<PanelNodeDefinition> children = node.Children;
            if (children == null)
            {
                return true;
            }

            foreach (PanelNodeDefinition child in children)
            {
                if (!IsDepthValid(child, depth + 1))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 遞迴建立節點及其子樹，保留 Children 清單順序，Depth 由巢狀層級推導。
        /// </summary>
        /// <param name="node">節點定義。</param>
        /// <param name="depth">該節點深度（頂層為 1）。</param>
        /// <returns>建立的 <see cref="PanelNode"/>。</returns>
        private static PanelNode BuildNode(PanelNodeDefinition node, int depth)
        {
            IReadOnlyList<PanelNodeDefinition> childDefs = node.Children;
            var childNodes = new List<PanelNode>(childDefs?.Count ?? 0);

            if (childDefs != null)
            {
                foreach (PanelNodeDefinition child in childDefs)
                {
                    childNodes.Add(BuildNode(child, depth + 1));
                }
            }

            return new PanelNode(depth, node.Title, node.Tab, childNodes);
        }
    }
}
