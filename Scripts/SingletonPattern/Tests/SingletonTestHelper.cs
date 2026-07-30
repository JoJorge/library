namespace SingletonPattern.Tests
{
    using System.Reflection;

    /// <summary>
    /// 提供測試輔助方法，用於在測試之間重置 <see cref="Singleton{T}"/> 的靜態實例欄位。
    /// </summary>
    /// <remarks>
    /// 此類別透過反射存取 <c>Singleton&lt;T&gt;</c> 的 <c>private static volatile T _instance</c> 欄位，
    /// 將其設為 <c>null</c>，以確保各測試之間的狀態隔離。
    /// 建議在 <c>[SetUp]</c> 中呼叫 <see cref="ResetInstance{T}"/>。
    /// </remarks>
    public static class SingletonTestHelper
    {
        /// <summary>
        /// 透過反射將 <see cref="Singleton{T}._instance"/> 靜態欄位重置為 <c>null</c>。
        /// </summary>
        /// <typeparam name="T">繼承自 <see cref="Singleton{T}"/> 的子類別型別。</typeparam>
        public static void ResetInstance<T>()
            where T : Singleton<T>
        {
            FieldInfo instanceField = typeof(Singleton<T>).GetField(
                "_instance",
                BindingFlags.NonPublic | BindingFlags.Static);

            instanceField?.SetValue(null, null);
        }
    }
}
