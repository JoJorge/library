namespace SingletonPattern.Tests
{
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;

    /// <summary>
    /// 屬性導向測試（Property-Based Tests），使用 FsCheck 驗證 <see cref="Singleton{T}"/> 的正確性屬性。
    /// </summary>
    [TestFixture]
    public class SingletonPropertyTests
    {
        /// <summary>
        /// 每次測試前重置 <see cref="TestSingleton"/> 的靜態實例欄位，確保測試隔離。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            SingletonTestHelper.ResetInstance<TestSingleton>();
        }

        /// <summary>
        /// Property 1: Identity Preservation — 對於任意呼叫次數 n（n ≥ 2），
        /// 每次存取 <c>Singleton&lt;T&gt;.Instance</c> 所回傳的實例參考均應相同（<c>ReferenceEquals</c> 成立）。
        /// </summary>
        /// <param name="accessCount">由 FsCheck 生成的隨機存取次數（範圍 2～1000）。</param>
        /// <remarks>
        /// <para><strong>Validates: Requirements 1.2, 2.3</strong></para>
        /// </remarks>
        [Test]
        public void Instance_MultipleAccesses_AlwaysReturnsSameReference()
        {
            Prop.ForAll(
                Gen.Choose(2, 1000).ToArbitrary(),
                accessCount =>
                {
                    SingletonTestHelper.ResetInstance<TestSingleton>();

                    TestSingleton first = TestSingleton.Instance;

                    for (int i = 1; i < accessCount; i++)
                    {
                        TestSingleton current = TestSingleton.Instance;

                        if (!ReferenceEquals(first, current))
                        {
                            return false;
                        }
                    }

                    return true;
                }).QuickCheckThrowOnFailure();
        }
    }
}
