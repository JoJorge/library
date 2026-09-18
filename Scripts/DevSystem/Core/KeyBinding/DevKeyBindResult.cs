using System;
using System.Collections.Generic;
using InputSystem;

namespace DevSystem
{
    /// <summary>
    /// 開發者按鍵綁定結果。成功時 Success 為 true；失敗時 FailureReason 指出原因，
    /// Conflicts 於衝突失敗時攜帶所有衝突對象（需求 5.3）。
    /// </summary>
    public class DevKeyBindResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevKeyBindResult"/> class.
        /// </summary>
        /// <param name="success">綁定是否成功。</param>
        /// <param name="failureReason">失敗原因；成功時應為 <see cref="DevKeyBindFailure.None"/>。</param>
        /// <param name="conflicts">衝突對象清單；null 時以空集合取代。</param>
        public DevKeyBindResult(
            bool success,
            DevKeyBindFailure failureReason,
            IReadOnlyList<BindingConflict> conflicts)
        {
            this.Success = success;
            this.FailureReason = failureReason;
            this.Conflicts = conflicts ?? Array.Empty<BindingConflict>();
        }

        /// <summary>Gets a value indicating whether 綁定是否成功。</summary>
        public bool Success { get; }

        /// <summary>Gets 失敗原因（成功時為 None）。</summary>
        public DevKeyBindFailure FailureReason { get; }

        /// <summary>Gets 衝突對象清單（僅衝突失敗時非空，重用 input-system 的 BindingConflict）。</summary>
        public IReadOnlyList<BindingConflict> Conflicts { get; }

        /// <summary>
        /// 建立一個成功的綁定結果。
        /// </summary>
        /// <returns>Success 為 true、FailureReason 為 None、Conflicts 為空的結果。</returns>
        public static DevKeyBindResult Ok()
        {
            return new DevKeyBindResult(true, DevKeyBindFailure.None, Array.Empty<BindingConflict>());
        }

        /// <summary>
        /// 建立一個非衝突類別的失敗結果。
        /// </summary>
        /// <param name="reason">失敗原因。</param>
        /// <returns>Success 為 false、帶指定原因、Conflicts 為空的結果。</returns>
        public static DevKeyBindResult Failure(DevKeyBindFailure reason)
        {
            return new DevKeyBindResult(false, reason, Array.Empty<BindingConflict>());
        }

        /// <summary>
        /// 建立一個衝突失敗結果，攜帶所有衝突對象供呼叫端處理（需求 5.3）。
        /// </summary>
        /// <param name="conflicts">與既有綁定衝突的對象清單。</param>
        /// <returns>Success 為 false、FailureReason 為 Conflict、Conflicts 帶入指定清單的結果。</returns>
        public static DevKeyBindResult ConflictResult(IReadOnlyList<BindingConflict> conflicts)
        {
            return new DevKeyBindResult(false, DevKeyBindFailure.Conflict, conflicts);
        }
    }
}
