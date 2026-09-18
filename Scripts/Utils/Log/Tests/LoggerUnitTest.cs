namespace Utils.Tests
{
    using NUnit.Framework;
    /// <summary>
    /// Logger 的單元測試類別。
    /// </summary>
    [TestFixture]
    public class LoggerUnitTest
    {
        /// <summary>
        /// 在每個測試方法執行前，重置 Logger 的設定。
        /// </summary>
        [SetUp]
        public void Setup()
        {
            Logger.SetLogAction(null);
            Logger.SetLogWarningAction(null);
            Logger.SetLogErrorAction(null);
        }

        /// <summary>
        /// 測試 Logger 的 Log 方法是否正確呼叫設定的 LogAction。
        /// </summary>
        [Test]
        public void TestLogAction()
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Log, "Test message");
#endif
            bool logCalled = false;
            Logger.SetLogAction(msg => logCalled = true);
            Logger.Log("Test message");
            Assert.IsTrue(logCalled, "Log action should be called.");
        }

        /// <summary>
        /// 測試 Logger 的 LogWarning 方法是否正確呼叫設定的 LogWarningAction。
        /// </summary>
        [Test]
        public void TestLogWarningAction()
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, "Test warning message");
#endif
            bool logWarningCalled = false;
            Logger.SetLogWarningAction(msg => logWarningCalled = true);
            Logger.LogWarning("Test warning message");
            Assert.IsTrue(logWarningCalled, "Log warning action should be called.");
        }

        /// <summary>
        /// 測試 Logger 的 LogError 方法是否正確呼叫設定的 LogErrorAction。
        /// </summary>
        [Test]
        public void TestLogErrorAction()
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, "Test error message");
#endif
            bool logErrorCalled = false;
            Logger.SetLogErrorAction(msg => logErrorCalled = true);
            Logger.LogError("Test error message");
            Assert.IsTrue(logErrorCalled, "Log error action should be called.");
        }
    }
}
