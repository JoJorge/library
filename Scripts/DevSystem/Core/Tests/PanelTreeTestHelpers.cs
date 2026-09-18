namespace DevSystem.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using FsCheck;
    using FsCheck.Fluent;

    /// <summary>
    /// Panel 樹測試的共用工具與 FsCheck 產生器。
    /// 提供有界深度的巢狀 <see cref="PanelNodeDefinition"/> 樹產生器，
    /// 供 Property 6（深度有效性）與 Property 7（導覽與順序）測試使用。
    /// </summary>
    internal static class PanelTreeTestHelpers
    {
        /// <summary>
        /// 計算節點樹的最大深度（頂層清單的 <paramref name="rootDepth"/> 為起始深度）。
        /// 空清單回傳 0。
        /// </summary>
        /// <param name="nodes">節點定義清單。</param>
        /// <param name="rootDepth">此層節點所在的深度（頂層為 1）。</param>
        /// <returns>此清單所有節點與後代中的最大深度。</returns>
        public static int MaxDefinitionDepth(IReadOnlyList<PanelNodeDefinition> nodes, int rootDepth)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return 0;
            }

            int max = rootDepth;
            foreach (PanelNodeDefinition node in nodes)
            {
                int childMax = MaxDefinitionDepth(node.Children, rootDepth + 1);
                if (childMax > max)
                {
                    max = childMax;
                }
            }

            return max;
        }

        /// <summary>
        /// 計算已建構節點樹的最大深度（讀取 <see cref="PanelNode.Depth"/>）。
        /// 空清單回傳 0。
        /// </summary>
        /// <param name="nodes">已建構節點清單。</param>
        /// <returns>樹中所有節點的最大 Depth。</returns>
        public static int MaxNodeDepth(IReadOnlyList<PanelNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return 0;
            }

            int max = 0;
            foreach (PanelNode node in nodes)
            {
                if (node.Depth > max)
                {
                    max = node.Depth;
                }

                int childMax = MaxNodeDepth(node.Children);
                if (childMax > max)
                {
                    max = childMax;
                }
            }

            return max;
        }

        /// <summary>
        /// 產生「有效」的頂層節點清單：頂層數量落在 [MinDepth, 上限]，
        /// 且任一節點深度不超過 <see cref="PanelTreeBuilder.MaxDepth"/>。
        /// 為控制測試規模，頂層數量與子節點數量皆採較小上限。
        /// </summary>
        /// <returns>有效頂層節點清單的 Arbitrary。</returns>
        public static Arbitrary<IReadOnlyList<PanelNodeDefinition>> ValidTopLevel()
        {
            // 有效樹：深度 1..MaxDepth，頂層數量 1..8（遠小於 MaxTabCount，聚焦深度/結構）。
            Gen<IReadOnlyList<PanelNodeDefinition>> gen =
                Gen.Choose(1, 8).SelectMany(count =>
                    SequenceList(Repeat(() => NodeGen(1, PanelTreeBuilder.MaxDepth), count))
                        .Select(list => (IReadOnlyList<PanelNodeDefinition>)list));

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 產生「可能越界」的頂層節點清單：允許節點深度超過 <see cref="PanelTreeBuilder.MaxDepth"/>，
        /// 用於驗證越界定義被拒絕。頂層數量 1..6，深度上限延伸至 MaxDepth + 4。
        /// </summary>
        /// <returns>可能越界的頂層節點清單的 Arbitrary。</returns>
        public static Arbitrary<IReadOnlyList<PanelNodeDefinition>> MaybeOverDepthTopLevel()
        {
            Gen<IReadOnlyList<PanelNodeDefinition>> gen =
                Gen.Choose(1, 6).SelectMany(count =>
                    SequenceList(Repeat(() => NodeGen(1, PanelTreeBuilder.MaxDepth + 4), count))
                        .Select(list => (IReadOnlyList<PanelNodeDefinition>)list));

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 遞迴產生單一節點定義。節點所在深度為 <paramref name="depth"/>，
        /// 允許的子孫最深深度為 <paramref name="maxDepthAllowed"/>。
        /// 當深度已達上限時不再產生子節點；否則以隨機數量（0..3）產生子節點。
        /// </summary>
        /// <param name="depth">此節點所在深度（頂層為 1）。</param>
        /// <param name="maxDepthAllowed">此產生器允許出現的最大深度。</param>
        /// <returns>節點定義的產生器。</returns>
        private static Gen<PanelNodeDefinition> NodeGen(int depth, int maxDepthAllowed)
        {
            string title = "N" + depth;

            if (depth >= maxDepthAllowed)
            {
                // 已達此產生器允許的最深深度，強制為葉節點。
                return Gen.Constant(new PanelNodeDefinition(title, new object(), null));
            }

            return Gen.Choose(0, 3).SelectMany(childCount =>
            {
                if (childCount == 0)
                {
                    // 葉節點：帶一個 tab（以 object 承載，模擬 IDevPanelTab）。
                    return Gen.Constant(new PanelNodeDefinition(title, new object(), null));
                }

                IReadOnlyList<Gen<PanelNodeDefinition>> childGens =
                    Repeat(() => NodeGen(depth + 1, maxDepthAllowed), childCount);

                return SequenceList(childGens).Select(children =>
                    new PanelNodeDefinition(
                        title,
                        null,
                        (IReadOnlyList<PanelNodeDefinition>)children));
            });
        }

        /// <summary>
        /// 將一組產生器序列化為「產生 List」的產生器（僅使用 <c>FsCheck.Fluent.Gen</c> 的
        /// Select/SelectMany，避免相依 <c>FsCheck.FSharp.Gen</c> 的 Sequence API）。
        /// </summary>
        /// <typeparam name="T">元素型別。</typeparam>
        /// <param name="gens">產生器清單。</param>
        /// <returns>產生 <see cref="List{T}"/> 的產生器。</returns>
        private static Gen<List<T>> SequenceList<T>(IReadOnlyList<Gen<T>> gens)
        {
            // 以 Select 產生「每次抽樣皆為全新空 List」的起始產生器，避免共用同一 List 實例
            // 造成跨抽樣累積。之後逐一 SelectMany 折疊各元素產生器。
            Gen<List<T>> acc = Gen.Choose(0, 0).Select(_ => new List<T>());
            foreach (Gen<T> g in gens)
            {
                Gen<T> current = g;
                acc = acc.SelectMany(list => current.Select(value =>
                {
                    list.Add(value);
                    return list;
                }));
            }

            return acc;
        }

        /// <summary>
        /// 重複呼叫工廠 <paramref name="factory"/> 產生 <paramref name="count"/> 個項目的序列。
        /// </summary>
        /// <typeparam name="T">項目型別。</typeparam>
        /// <param name="factory">項目工廠。</param>
        /// <param name="count">產生數量。</param>
        /// <returns>項目序列。</returns>
        private static IReadOnlyList<T> Repeat<T>(Func<T> factory, int count)
        {
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(factory());
            }

            return list;
        }
    }
}
