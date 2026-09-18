using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevSystem.Unity
{
    /// <summary>
    /// 日誌開關設定（ScriptableObject）。以「啟用的 <see cref="LogCategory"/> 清單」儲存開關，
    /// 新增 <see cref="LogCategory"/> 列舉成員時無需修改本類別（利於擴充）。
    /// </summary>
    /// <remarks>
    /// 清單以 Odin Inspector 的 <c>[ShowInInspector]</c> 呈現，且為純 C# property（非 <c>[SerializeField]</c>），
    /// 因此值不被 Unity 序列化、不落地、不進版本控制（需求 3.7）。開發者於執行前在 Editor Inspector 中手動設定啟用清單。
    /// </remarks>
    [CreateAssetMenu(menuName = "DevSystem/Log Toggle Config")]
    public class DevLogToggleConfig : ScriptableObject
    {
        /// <summary>
        /// Gets or sets 啟用的日誌分類清單；清單內為開啟、清單外為關閉。
        /// </summary>
        [ShowInInspector]
        public List<LogCategory> EnabledCategories { get; set; } = new List<LogCategory>();
    }
}
