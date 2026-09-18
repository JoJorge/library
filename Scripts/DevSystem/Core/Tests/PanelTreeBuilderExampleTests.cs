namespace DevSystem.Core.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;

    /// <summary>
    /// <see cref="PanelTreeBuilder"/> 的範例（example-based）測試，聚焦巢狀順序保留
    /// 與深度上限邊界的具體案例。補充 Property 6/7 的隨機測試，驗證固定的代表性情境。
    /// </summary>
    [TestFixture]
    public class PanelTreeBuilderExampleTests
    {
        /// <summary>
        /// 建立葉節點定義（帶一個以 object 承載的 tab）。
        /// </summary>
        /// <param name="title">節點標題。</param>
        /// <returns>葉節點定義。</returns>
        private static PanelNodeDefinition Leaf(string title)
            => new PanelNodeDefinition(title, new object(), null);

        /// <summary>
        /// 建立容器節點定義（帶子節點，tab 為 null）。
        /// </summary>
        /// <param name="title">節點標題。</param>
        /// <param name="children">子節點。</param>
        /// <returns>容器節點定義。</returns>
        private static PanelNodeDefinition Container(string title, params PanelNodeDefinition[] children)
            => new PanelNodeDefinition(title, null, new List<PanelNodeDefinition>(children));

        /// <summary>
        /// 範例（7.2）：頂層分頁的顯示順序等於 Nodes 清單順序，建構後 Title 序列不變。
        /// </summary>
        [Test]
        public void TryBuild_TopLevelOrderMatchesDefinitionListOrder()
        {
            var definition = new PanelDefinition(new List<PanelNodeDefinition>
            {
                Leaf("Alpha"),
                Leaf("Beta"),
                Leaf("Gamma"),
            });

            bool built = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);

            Assert.That(built, Is.True);
            Assert.That(roots, Is.Not.Null);
            Assert.That(new[] { roots[0].Title, roots[1].Title, roots[2].Title },
                Is.EqualTo(new[] { "Alpha", "Beta", "Gamma" }));
        }

        /// <summary>
        /// 範例（7.2, 7.4）：巢狀子分頁順序保留，且父子巢狀關係與 Depth 依定義推導。
        /// 驗證每層同層順序等於該層 Children 清單順序、Kind 由 Children 是否為空推導。
        /// </summary>
        [Test]
        public void TryBuild_NestedOrderAndParentChildStructurePreserved()
        {
            // 頂層 Root（容器）→ 子 C1(容器)/C2(葉) → C1 下 L1a/L1b（葉）。
            var definition = new PanelDefinition(new List<PanelNodeDefinition>
            {
                Container(
                    "Root",
                    Container("C1", Leaf("L1a"), Leaf("L1b")),
                    Leaf("C2")),
            });

            bool built = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);

            Assert.That(built, Is.True);
            Assert.That(roots.Count, Is.EqualTo(1));

            PanelNode root = roots[0];
            Assert.That(root.Title, Is.EqualTo("Root"));
            Assert.That(root.Depth, Is.EqualTo(1));
            Assert.That(root.Kind, Is.EqualTo(PanelNodeKind.SubTabContainer));

            // 第二層順序：C1 在 C2 之前（等於 Children 清單順序）。
            Assert.That(new[] { root.Children[0].Title, root.Children[1].Title },
                Is.EqualTo(new[] { "C1", "C2" }));

            PanelNode c1 = root.Children[0];
            PanelNode c2 = root.Children[1];
            Assert.That(c1.Depth, Is.EqualTo(2));
            Assert.That(c1.Kind, Is.EqualTo(PanelNodeKind.SubTabContainer));
            Assert.That(c2.Depth, Is.EqualTo(2));
            Assert.That(c2.Kind, Is.EqualTo(PanelNodeKind.Leaf));

            // 第三層順序：L1a 在 L1b 之前，皆為葉節點且 Depth 為 3。
            Assert.That(new[] { c1.Children[0].Title, c1.Children[1].Title },
                Is.EqualTo(new[] { "L1a", "L1b" }));
            Assert.That(c1.Children[0].Kind, Is.EqualTo(PanelNodeKind.Leaf));
            Assert.That(c1.Children[1].Kind, Is.EqualTo(PanelNodeKind.Leaf));
            Assert.That(c1.Children[0].Depth, Is.EqualTo(3));
        }

        /// <summary>
        /// 範例（7.3, 6.5 邊界）：深度恰為 MaxDepth 的線性鏈可成功建構。
        /// </summary>
        [Test]
        public void TryBuild_ChainAtMaxDepthSucceeds()
        {
            PanelNodeDefinition chain = BuildLinearChain(PanelTreeBuilder.MaxDepth);
            var definition = new PanelDefinition(new List<PanelNodeDefinition> { chain });

            bool built = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);

            Assert.That(built, Is.True);
            Assert.That(PanelTreeTestHelpers.MaxNodeDepth(roots), Is.EqualTo(PanelTreeBuilder.MaxDepth));
        }

        /// <summary>
        /// 範例（7.3, 6.5 邊界）：深度超過 MaxDepth 一層的線性鏈被拒絕，roots 為 null。
        /// </summary>
        [Test]
        public void TryBuild_ChainExceedingMaxDepthRejected()
        {
            PanelNodeDefinition chain = BuildLinearChain(PanelTreeBuilder.MaxDepth + 1);
            var definition = new PanelDefinition(new List<PanelNodeDefinition> { chain });

            bool built = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);

            Assert.That(built, Is.False);
            Assert.That(roots, Is.Null);
        }

        /// <summary>
        /// 範例（7.1 邊界）：空的頂層清單（少於 MinDepth）被拒絕。
        /// </summary>
        [Test]
        public void TryBuild_EmptyTopLevelRejected()
        {
            var definition = new PanelDefinition(new List<PanelNodeDefinition>());

            bool built = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);

            Assert.That(built, Is.False);
            Assert.That(roots, Is.Null);
        }

        /// <summary>
        /// 建立一條深度為 <paramref name="depth"/> 的線性鏈（每層恰一個子節點，末端為葉）。
        /// </summary>
        /// <param name="depth">鏈的總深度（≥ 1）。</param>
        /// <returns>鏈的頂層節點定義。</returns>
        private static PanelNodeDefinition BuildLinearChain(int depth)
        {
            PanelNodeDefinition node = Leaf("D" + depth);
            for (int d = depth - 1; d >= 1; d--)
            {
                node = Container("D" + d, node);
            }

            return node;
        }
    }
}
