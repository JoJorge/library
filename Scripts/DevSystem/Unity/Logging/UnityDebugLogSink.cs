using System;
using UnityEngine;

namespace DevSystem.Unity
{
    /// <summary>
    /// 預設 <see cref="ILogSink"/> 實作，將已格式化的日誌字串包裹 <see cref="Debug.Log(object)"/> 輸出。
    /// </summary>
    /// <remarks>
    /// 以 try-catch 回報成敗：成功寫入回傳 <see langword="true"/>，底層輸出拋出例外時抑制並回傳
    /// <see langword="false"/>，讓 <see cref="DevLogger"/> 得以在不外拋的情況下處理輸出失敗。
    /// </remarks>
    public class UnityDebugLogSink : ILogSink
    {
        /// <inheritdoc/>
        public bool Write(string formatted)
        {
            try
            {
                Debug.Log(formatted);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
