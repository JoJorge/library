namespace SingletonPattern.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using NUnit.Framework;

    /// <summary>
    /// 基本單例行為的範例導向單元測試。
    /// 驗證延遲初始化、存取控制、抽象類別特性、泛型約束、實例唯一性，以及子類別繼承可用性。
    /// </summary>
    [TestFixture]
    public class SingletonUnitTests
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
        /// 驗證首次存取 Instance 前，私有靜態欄位 _instance 為 null（延遲初始化行為）。
        /// </summary>
        [Test]
        public void Instance_BeforeFirstAccess_FieldIsNull()
        {
            FieldInfo instanceField = typeof(Singleton<TestSingleton>).GetField(
                "_instance",
                BindingFlags.NonPublic | BindingFlags.Static);

            object value = instanceField.GetValue(null);

            Assert.That(value, Is.Null);
        }

        /// <summary>
        /// 驗證 Singleton&lt;T&gt; 的建構子為 protected（反射修飾詞為 Family）。
        /// </summary>
        [Test]
        public void Constructor_AccessModifier_IsProtected()
        {
            ConstructorInfo[] constructors = typeof(Singleton<TestSingleton>).GetConstructors(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.That(constructors.Length, Is.GreaterThan(0), "應至少存在一個建構子");

            ConstructorInfo protectedCtor = constructors.FirstOrDefault(c => c.IsFamily);

            Assert.That(protectedCtor, Is.Not.Null, "Singleton<T> 應具備 protected (Family) 建構子");
        }

        /// <summary>
        /// 驗證 Singleton&lt;&gt; 開放泛型型別為 abstract 類別。
        /// </summary>
        [Test]
        public void SingletonOpenGenericType_IsAbstract()
        {
            Type openGenericType = typeof(Singleton<>);

            Assert.That(openGenericType.IsAbstract, Is.True);
        }

        /// <summary>
        /// 驗證泛型參數 T 受限為 Singleton&lt;T&gt; 的子類別（GenericParameterConstraints 包含基底類別約束）。
        /// </summary>
        [Test]
        public void GenericConstraint_T_IsConstrainedToSingletonSubclass()
        {
            Type openGenericType = typeof(Singleton<>);
            Type[] genericParams = openGenericType.GetGenericArguments();

            Assert.That(genericParams.Length, Is.EqualTo(1), "應有一個泛型型別參數");

            Type typeParam = genericParams[0];
            Type[] constraints = typeParam.GetGenericParameterConstraints();

            Assert.That(constraints.Length, Is.GreaterThan(0), "泛型參數 T 應有約束");
            Assert.That(
                constraints.Any(c => c.IsGenericType && c.GetGenericTypeDefinition() == typeof(Singleton<>)),
                Is.True,
                "泛型參數 T 應受限為 Singleton<T> 的子類別");
        }

        /// <summary>
        /// 驗證多次存取 Instance 屬性回傳相同的實例參考。
        /// </summary>
        [Test]
        public void Instance_MultipleAccesses_ReturnsSameReference()
        {
            TestSingleton first = TestSingleton.Instance;
            TestSingleton second = TestSingleton.Instance;
            TestSingleton third = TestSingleton.Instance;

            Assert.That(ReferenceEquals(first, second), Is.True, "第一次與第二次存取應為同一實例");
            Assert.That(ReferenceEquals(second, third), Is.True, "第二次與第三次存取應為同一實例");
        }

        /// <summary>
        /// 驗證子類別繼承 Singleton&lt;T&gt; 後即可直接使用 Instance 屬性，無需額外單例邏輯。
        /// </summary>
        [Test]
        public void Subclass_InheritsInstanceProperty_WithoutAdditionalLogic()
        {
            SingletonTestHelper.ResetInstance<AnotherTestSingleton>();

            AnotherTestSingleton instance = AnotherTestSingleton.Instance;

            Assert.That(instance, Is.Not.Null, "子類別應可透過 Instance 取得實例");
            Assert.That(instance, Is.TypeOf<AnotherTestSingleton>(), "回傳的實例應為正確的子類別型別");
            Assert.That(ReferenceEquals(instance, AnotherTestSingleton.Instance), Is.True, "多次存取應回傳同一實例");
        }
    }
}
