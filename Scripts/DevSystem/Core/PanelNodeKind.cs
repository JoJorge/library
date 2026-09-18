namespace DevSystem
{
    /// <summary>
    /// 面板節點種類，由節點的 Children 是否為空推導而得。
    /// </summary>
    public enum PanelNodeKind
    {
        /// <summary>
        /// 葉節點，無子分頁，顯示自身的分頁內容。
        /// </summary>
        Leaf,

        /// <summary>
        /// 容器節點，含子分頁，顯示下一層子分頁選擇器。
        /// </summary>
        SubTabContainer,
    }
}
