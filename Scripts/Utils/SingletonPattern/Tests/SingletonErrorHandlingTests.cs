namespace SingletonPattern.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    /// <summary>
    /// 反射防護與錯誤處理的單元測試。
    /// 驗證 <see cref="Singleton{T}"/> 在反射攻擊與子類別未提供無參建構子時的行為。
    /// </summary>
    [TestFixture]
    public class SingletonErrorHandlingTests
    {
        /// <summary>
        /// 每個測試前重置 <see cref="TestSingleton"/> 的實例，確保測試隔離性。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            SingletonTestHelper.ResetInstance<TestSingleton>();
        }

        /// <summary>
        /// 驗證在實例已存在的情況下，透過反射呼叫建構子會拋出 <see cref="InvalidOperationException"/>。
        /// </summary>
        [Test]
        public void Constructor_WhenInstanceAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange: 確保實例已存在
            _ = TestSingleton.Instance;

            ConstructorInfo ctor = typeof(TestSingleton).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);

            Assert.That(ctor, Is.Not.Null, "應存在 protected 無參建構子");

            // Act & Assert: 反射呼叫建構子應拋出 TargetInvocationException，其內部例外為 InvalidOperationException
            TargetInvocationException ex = Assert.Throws<TargetInvocationException>(
                () => ctor.Invoke(null));

            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(
                ex.InnerException.Message,
                Does.Contain(nameof(TestSingleton)));
        }

        /// <summary>
        /// 驗證反射防護的例外訊息包含正確的型別名稱與使用提示。
        /// </summary>
        [Test]
        public void Constructor_WhenInstanceAlreadyExists_ExceptionMessageContainsUsageHint()
        {
            // Arrange: 確保實例已存在
            _ = TestSingleton.Instance;

            ConstructorInfo ctor = typeof(TestSingleton).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);

            // Act
            TargetInvocationException ex = Assert.Throws<TargetInvocationException>(
                () => ctor.Invoke(null));

            // Assert: 訊息應包含 Instance 存取提示
            string message = ex.InnerException.Message;
            Assert.That(message, Does.Contain($"{nameof(TestSingleton)}.Instance"));
        }

        /// <summary>
        /// 驗證子類別未提供無參建構子時，存取 <c>Instance</c> 屬性拋出 <see cref="MissingMethodException"/>。
        /// </summary>
        [Test]
        public void Instance_WhenSubclassLacksParameterlessConstructor_ThrowsMissingMethodException()
        {
            // Arrange & Act & Assert
            Assert.Throws<MissingMethodException>(
                () => _ = NoParameterlessCtorSingleton.Instance);
        }
    }
}
