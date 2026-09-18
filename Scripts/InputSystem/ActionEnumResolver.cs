using System;
using System.Reflection;

namespace InputSystem
{
    /// <summary>
    /// 動作列舉解析器，負責驗證列舉合法性並轉換為 Unity action name 字串。
    /// </summary>
    public static class ActionEnumResolver
    {
        /// <summary>
        /// 驗證指定列舉值所屬的列舉型別是否標註 [InputActionEnum]。
        /// </summary>
        /// <param name="action">要驗證的列舉值。</param>
        /// <returns>驗證通過回傳 true；未標註回傳 false。</returns>
        public static bool Validate(Enum action)
        {
            return action.GetType().GetCustomAttribute<InputActionEnumAttribute>() != null;
        }

        /// <summary>
        /// 將列舉值解析為對應的 Unity action name 字串。
        /// 使用 Enum.ToString() 作為 action name。
        /// </summary>
        /// <param name="action">動作列舉值。</param>
        /// <returns>對應的 Unity action name 字串。</returns>
        public static string Resolve(Enum action)
        {
            return action.ToString();
        }

        /// <summary>
        /// 取得指定動作列舉值所屬的 InputContext。
        /// </summary>
        /// <param name="action">動作列舉值。</param>
        /// <returns>關聯的 InputContext。</returns>
        public static InputContext GetContext(Enum action)
        {
            var attr = action.GetType().GetCustomAttribute<InputActionEnumAttribute>();
            return attr.Context;
        }
    }
}
