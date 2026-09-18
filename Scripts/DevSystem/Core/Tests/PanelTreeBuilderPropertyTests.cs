namespace DevSystem.Core.Tests
{
    using System.Collections.Generic;
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;

    /// <summary>
    /// <see cref="PanelTreeBuilder"/> 的屬性導向測試（Property-Based Testing）。
    /// 使用 FsCheck 生成隨機巢狀面板結構定義，驗證深度有效性不變式（Property 6）
    /// 與導覽/順序保留（Property 7）。每個屬性最少執行 100 次隨機迭代
    /// （<c>QuickCheckThrowOnFailure</c> 預設 100 次）。
    /// </summary>
    [TestFixture]
    public class PanelTreeBuilderPropertyTests
    {
        /// <summary>
        /// Property 6: Panel Depth Validity Invariant — 面板階層有效性不變式。
        /// 對於任意面板結構定義，<see cref="PanelTreeBuilder.TryBuild"/> 應僅在每個節點深度
        /// 皆不超過固定常數 <see cref="PanelTreeBuilder.MaxDepth"/> 時才成功建構；成功時所得樹的
        /// 最大深度必 ≤ MaxDepth；任何含越界節點的定義皆被拒絕（回傳 false 且 roots 為 null），
        /// 保留呼叫端既有結構不變（定義物件不被改動）。
        /// </summary>
        /// <remarks><para><strong>Validates: Requirements 6.5, 7.3</strong></para></remarks>
        [Test]
        public void TryBuild_RespectsDepthValidityInvariant()
        {
            Prop.ForAll(
                PanelTreeTestHelpers.MaybeOverDepthTopLevel(),
                topLevel =>
                {
                    var definition = new PanelDefinition(topLevel);

                    // 建構前的定義快照（頂層數量與最大定義深度），用於驗證拒絕時結構不變。
                    int defTopCountBefore = topLevel.Count;
                    int defMaxDepthBefore = PanelTreeTestHelpers.MaxDefinitionDepth(topLevel, 1);

                    bool built = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);

                    // 定義本身不應被 TryBuild 改動（既有結構保持不變）。
                    int defTopCountAfter = topLevel.Count;
                    int defMaxDepthAfter = PanelTreeTestHelpers.MaxDefinitionDepth(topLevel, 1);
                    if (defTopCountAfter != defTopCountBefore || defMaxDepthAfter != defMaxDepthBefore)
                    {
                        return false.Label("definition mutated by TryBuild");
                    }

                    bool depthValid = defMaxDepthBefore <= PanelTreeBuilder.MaxDepth;

                    if (built)
                    {
                        // 成功建構：定義深度必須有效，且所得樹最大深度 ≤ MaxDepth。
                        if (!depthValid)
                        {
                            return false.Label("built succeeded despite over-depth definition");
                        }

                        int treeMaxDepth = PanelTreeTestHelpers.MaxNodeDepth(roots);
                        if (treeMaxDepth > PanelTreeBuilder.MaxDepth)
                        {
                            return false.Label($"built tree max depth {treeMaxDepth} > MaxDepth");
                        }

                        return true.Label("valid definition built within depth limit");
                    }

                    // 被拒絕：roots 必為 null。此測試的頂層數量恆在 [1, MaxTabCount] 內，
                    // 故拒絕的唯一合法原因即深度越界。
                    if (roots != null)
                    {
                        return false.Label("rejected build returned non-null roots");
                    }

                    if (depthValid)
                    {
                        return false.Label("valid-depth definition was rejected");
                    }

                    return true.Label("over-depth definition rejected, structure preserved");
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 7: Panel Navigation and Ordering — 面板導覽與順序保留。
        /// 對於任意有效的巢狀面板結構定義，<see cref="PanelTreeBuilder.TryBuild"/> 後所得樹
        /// 的父子關係與巢狀結構一致、每層同層節點順序等於該層 Children 清單順序（以 Title 比對），
        /// 且每個節點的 <see cref="PanelNode.Kind"/> 為
        /// （Children 非空 ? SubTabContainer : Leaf）。
        /// </summary>
        /// <remarks><para><strong>Validates: Requirements 6.2, 6.3, 6.8, 7.2</strong></para></remarks>
        [Test]
        public void TryBuild_PreservesNavigationStructureAndOrdering()
        {
            Prop.ForAll(
                PanelTreeTestHelpers.ValidTopLevel(),
                topLevel =>
                {
                    var definition = new PanelDefinition(topLevel);

                    bool built = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);
                    if (!built)
                    {
                        return false.Label("valid definition failed to build");
                    }

                    // 頂層節點 Depth 必為 1。
                    string mismatch = CheckLevel(topLevel, roots, expectedDepth: 1);
                    return (mismatch == null).Label(mismatch ?? "structure and ordering preserved");
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// 遞迴比對某一層的定義清單與已建構節點清單，驗證：
        /// 數量一致、順序一致（以 Title 比對）、Depth 正確、Kind 由 Children 是否為空推導。
        /// </summary>
        /// <param name="defs">此層的定義清單。</param>
        /// <param name="nodes">此層對應的已建構節點清單。</param>
        /// <param name="expectedDepth">此層節點應有的 Depth。</param>
        /// <returns>發現不一致時回傳描述訊息；一致時回傳 null。</returns>
        private static string CheckLevel(
            IReadOnlyList<PanelNodeDefinition> defs,
            IReadOnlyList<PanelNode> nodes,
            int expectedDepth)
        {
            if (defs.Count != nodes.Count)
            {
                return $"depth {expectedDepth}: child count mismatch def={defs.Count} node={nodes.Count}";
            }

            for (int i = 0; i < defs.Count; i++)
            {
                PanelNodeDefinition def = defs[i];
                PanelNode node = nodes[i];

                // 順序一致（同層順序等於 Children 清單順序）。
                if (def.Title != node.Title)
                {
                    return $"depth {expectedDepth} index {i}: order/title mismatch def={def.Title} node={node.Title}";
                }

                if (node.Depth != expectedDepth)
                {
                    return $"node {node.Title}: expected depth {expectedDepth} got {node.Depth}";
                }

                int childCount = def.Children?.Count ?? 0;
                PanelNodeKind expectedKind = childCount > 0
                    ? PanelNodeKind.SubTabContainer
                    : PanelNodeKind.Leaf;

                if (node.Kind != expectedKind)
                {
                    return $"node {node.Title}: expected kind {expectedKind} got {node.Kind}";
                }

                // 父子關係與巢狀一致：遞迴比對子層。
                string childMismatch = CheckLevel(def.Children, node.Children, expectedDepth + 1);
                if (childMismatch != null)
                {
                    return childMismatch;
                }
            }

            return null;
        }
    }
}
