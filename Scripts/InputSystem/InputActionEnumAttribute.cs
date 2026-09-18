using System;

namespace InputSystem
{
    /// <summary>
    /// 標記一個列舉為合法的輸入動作列舉。
    /// 該屬性將列舉與特定的 InputContext 關聯。
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class InputActionEnumAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputActionEnumAttribute"/> class.
        /// </summary>
        /// <param name="context">此動作列舉所屬的情境。</param>
        public InputActionEnumAttribute(InputContext context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Gets 此動作列舉所屬的 InputContext。
        /// </summary>
        public InputContext Context { get; }
    }
}
