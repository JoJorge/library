using UnityEngine.UIElements;

namespace DevSystem.Unity
{
    /// <summary>
    /// 開發者面板分頁的公開契約。葉節點透過 BuildContent 提供內容，
    /// OnShown 於分頁顯示時觸發、OnUpdate 於更新週期觸發。
    /// </summary>
    public interface IDevPanelTab
    {
        /// <summary>Gets 分頁標題。</summary>
        string Title { get; }

        /// <summary>建立分頁內容的 UI Toolkit 元素。</summary>
        /// <returns>內容根元素。</returns>
        VisualElement BuildContent();

        /// <summary>分頁被顯示時呼叫（重新整理資料來源）。</summary>
        void OnShown();

        /// <summary>更新週期呼叫（同步外部變更）。</summary>
        void OnUpdate();
    }
}
