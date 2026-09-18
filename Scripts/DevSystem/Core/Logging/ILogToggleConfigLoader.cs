using System.Collections.Generic;

namespace DevSystem
{
    /// <summary>
    /// 日誌開關設定（Log_Toggle_Config）的載入抽象，提供各 <see cref="LogCategory"/> 的初始開關狀態。
    /// </summary>
    /// <remarks>
    /// 此介面僅負責讀取設定並回傳暫存用的開關對映，<see cref="DevLogger"/> 不會透過此介面寫回任何狀態，
    /// 以避免將個人設定持久化至版本控制。
    /// </remarks>
    public interface ILogToggleConfigLoader
    {
        /// <summary>
        /// 載入並回傳涵蓋所有已定義 <see cref="LogCategory"/> 的開關對映。
        /// </summary>
        /// <returns>
        /// 各 <see cref="LogCategory"/> 對應開關狀態的唯讀對映，<see langword="true"/> 表示啟用、
        /// <see langword="false"/> 表示停用。
        /// </returns>
        IReadOnlyDictionary<LogCategory, bool> Load();
    }
}
