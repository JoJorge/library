namespace DevSystem.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NSubstitute;
    using NUnit.Framework;

    /// <summary>
    /// <see cref="DevLogger"/> 的範例導向單元測試（Example-Based Tests），涵蓋 API 存在性、
    /// 列舉可擴充性與載入前預設全開等邊界行為。
    /// </summary>
    /// <remarks>
    /// 涉及實際日誌輸出的測項驗證的是 <c>DEV_MODE</c> 已定義時的行為
    /// （<see cref="DevLogger.Log"/> 以 <c>[Conditional("DEV_MODE")]</c> 閘控）。
    /// </remarks>
    [TestFixture]
    public class DevLoggerUnitTests
    {
        /// <summary>
        /// 驗證 <see cref="DevLogger.Log"/> 公開簽章存在：接受一個 <see cref="LogCategory"/> 與一段字串、回傳 void（Req 2.1）。
        /// </summary>
        [Test]
        public void Log_PublicSignature_AcceptsCategoryAndString()
        {
            MethodInfo method = typeof(DevLogger).GetMethod(
                nameof(DevLogger.Log),
                new[] { typeof(LogCategory), typeof(string) });

            Assert.That(method, Is.Not.Null, "DevLogger 應提供 Log(LogCategory, string) 公開方法");
            Assert.That(method.IsPublic, Is.True, "Log 應為公開方法");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(void)), "Log 應回傳 void");
        }

        /// <summary>
        /// 驗證 <see cref="LogCategory"/> 為 enum 型別，使呼叫端僅能指定已定義分類（Req 2.3）。
        /// </summary>
        [Test]
        public void LogCategory_IsEnumType()
        {
            Assert.That(typeof(LogCategory).IsEnum, Is.True, "LogCategory 應為 enum 型別");
        }

        /// <summary>
        /// 驗證 <see cref="DevLogger"/> 的分類清單與格式化邏輯以列舉為單一來源，
        /// 新增列舉成員即自動納入而不需修改日誌輸出邏輯（Req 2.4）。
        /// <see cref="DevLogger.GetCategories"/> 回傳的集合應與 <see cref="Enum.GetValues"/> 完全一致。
        /// </summary>
        [Test]
        public void GetCategories_ReflectsAllDefinedEnumMembers()
        {
            ILogSink sink = Substitute.For<ILogSink>();
            DevLogger logger = new DevLogger(sink);

            LogCategory[] expected = (LogCategory[])Enum.GetValues(typeof(LogCategory));
            IReadOnlyList<LogCategory> actual = logger.GetCategories();

            Assert.That(actual, Is.EquivalentTo(expected), "GetCategories 應涵蓋所有已定義的 LogCategory 成員");
        }

        /// <summary>
        /// 驗證於 <see cref="DevLogger.LoadToggleConfig"/> 尚未呼叫前，所有分類皆視為開啟並輸出日誌（Req 3.6）。
        /// </summary>
        [Test]
        public void Log_BeforeConfigLoaded_AllCategoriesEnabled()
        {
            int writeCount = 0;
            ILogSink sink = Substitute.For<ILogSink>();
            sink.Write(Arg.Do<string>(_ => writeCount++)).Returns(true);

            DevLogger logger = new DevLogger(sink);

            foreach (LogCategory category in (LogCategory[])Enum.GetValues(typeof(LogCategory)))
            {
                Assert.That(
                    logger.IsCategoryEnabled(category),
                    Is.True,
                    $"載入設定前分類 {category} 應視為開啟");
            }

            logger.Log(LogCategory.General, "before config");

            Assert.That(writeCount, Is.EqualTo(1), "載入設定前應輸出日誌");
        }

        /// <summary>
        /// 驗證 <see cref="DevLogger.SetCategoryEnabled"/> 公開簽章存在並回傳 <see cref="bool"/>（Req 3.9）。
        /// </summary>
        [Test]
        public void SetCategoryEnabled_PublicSignature_ReturnsBool()
        {
            MethodInfo method = typeof(DevLogger).GetMethod(
                nameof(DevLogger.SetCategoryEnabled),
                new[] { typeof(LogCategory), typeof(bool) });

            Assert.That(method, Is.Not.Null, "DevLogger 應提供 SetCategoryEnabled(LogCategory, bool) 公開方法");
            Assert.That(method.IsPublic, Is.True, "SetCategoryEnabled 應為公開方法");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(bool)), "SetCategoryEnabled 應回傳 bool");
        }

        /// <summary>
        /// 驗證 <see cref="DevLogger.SetCategoryEnabled"/> 於執行期呼叫時回傳 <see langword="true"/> 表示設定成功（Req 3.9）。
        /// </summary>
        [Test]
        public void SetCategoryEnabled_WhenInvoked_ReturnsTrue()
        {
            ILogSink sink = Substitute.For<ILogSink>();
            DevLogger logger = new DevLogger(sink);

            bool result = logger.SetCategoryEnabled(LogCategory.Network, false);

            Assert.That(result, Is.True, "SetCategoryEnabled 於 DEV_MODE 下應回傳 true");
        }
    }
}
