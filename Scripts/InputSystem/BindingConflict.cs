using System;
using System.Collections.Generic;

namespace InputSystem
{
    /// <summary>
    /// 表示一個綁定衝突的資料結構。
    /// </summary>
    public class BindingConflict
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingConflict"/> class.
        /// </summary>
        /// <param name="bindingPath">衝突的 Binding Path。</param>
        /// <param name="action">觸發衝突的動作列舉值。</param>
        /// <param name="conflictingActions">衝突的動作列舉值清單。</param>
        /// <param name="context">衝突所在的情境。</param>
        /// <param name="deviceType">衝突所在的裝置類型。</param>
        public BindingConflict(
            string bindingPath,
            Enum action,
            IReadOnlyList<Enum> conflictingActions,
            InputContext context,
            InputDeviceType deviceType)
        {
            this.BindingPath = bindingPath;
            this.Action = action;
            this.ConflictingActions = conflictingActions;
            this.Context = context;
            this.DeviceType = deviceType;
        }

        /// <summary>
        /// Gets 衝突的 Binding Path。
        /// </summary>
        public string BindingPath { get; }

        /// <summary>
        /// Gets 觸發衝突的動作列舉值。
        /// </summary>
        public Enum Action { get; }

        /// <summary>
        /// Gets 衝突的動作列舉值清單。
        /// </summary>
        public IReadOnlyList<Enum> ConflictingActions { get; }

        /// <summary>
        /// Gets 衝突所在的情境。
        /// </summary>
        public InputContext Context { get; }

        /// <summary>
        /// Gets 衝突所在的裝置類型。
        /// </summary>
        public InputDeviceType DeviceType { get; }
    }
}
