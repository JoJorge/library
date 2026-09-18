namespace DevSystem.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using FsCheck;
    using FsCheck.Fluent;
    using NSubstitute;
    using NUnit.Framework;

    /// <summary>
    /// <see cref="DevLogger"/> 的屬性導向測試（Property-Based Tests），使用 FsCheck 3.x（FsCheck.Fluent）
    /// 於標準 NUnit <c>[Test]</c> 方法內直接呼叫 FsCheck API 驗證日誌格式、開關閘控與設定套用的正確性屬性。
    /// </summary>
    /// <remarks>
    /// 這些屬性驗證的是 <c>DEV_MODE</c> 已定義時的行為（<see cref="DevLogger.Log"/> 以
    /// <c>[Conditional("DEV_MODE")]</c> 閘控，其實際輸出僅於 <c>DEV_MODE</c> 生效時可觀察）。
    /// <c>DEV_MODE</c> 未定義時的空實作等價性由另一項任務（Property 11）獨立驗證。
    /// 每個屬性最少執行 100 次隨機迭代（FsCheck 3.x 之 <c>QuickCheckThrowOnFailure</c> 預設 MaxTest = 100）。
    /// </remarks>
    [TestFixture]
    public class DevLoggerPropertyTests
    {
        /// <summary>
        /// 當分類值未對應任何已定義列舉成員時，格式化前綴所使用的固定預設分類名稱（對應 <see cref="LogCategory.General"/>）。
        /// </summary>
        private const string DefaultCategoryName = "General";

        /// <summary>
        /// Property 1: Log Format Structure and Robustness — 日誌格式結構與健壯性。
        /// 對於任意 <see cref="LogCategory"/>（含越界強制轉型）與任意訊息（含 null／空／超過 4096 字元），
        /// 當分類開關為開啟時，寫入底層 <see cref="ILogSink"/> 的字串恆為 <c>"[Dev][" + name + "] " + msg</c> 結構：
        /// name 於分類已定義時為列舉名稱、未定義時為固定預設名；msg 於 null／空時為空字串、
        /// 於超過 4096 時為原字串前 4096 字元前綴、否則等於原字串。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 1: Log Format Structure and Robustness.
        /// <para><strong>Validates: Requirements 2.2, 2.5, 2.6, 2.7</strong></para>
        /// </remarks>
        [Test]
        public void Log_FormatStructure_AlwaysMatchesExpected()
        {
            Prop.ForAll(
                GenAnyCategory(),
                GenMessage(),
                (category, message) =>
                {
                    string captured = null;
                    ILogSink sink = Substitute.For<ILogSink>();
                    sink.Write(Arg.Do<string>(s => captured = s)).Returns(true);

                    // 載入前所有分類視為開啟，故不需額外設定即可通過閘控（Req 3.6）。
                    DevLogger logger = new DevLogger(sink);

                    logger.Log(category, message);

                    string expectedName = Enum.IsDefined(typeof(LogCategory), category)
                        ? category.ToString()
                        : DefaultCategoryName;

                    string msg = message ?? string.Empty;
                    if (msg.Length > DevLogger.MaxMessageLength)
                    {
                        msg = msg.Substring(0, DevLogger.MaxMessageLength);
                    }

                    string expected = "[Dev][" + expectedName + "] " + msg;

                    return captured == expected;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 1（延伸）：截斷後的 MSG 長度恆為 <c>min(原長度, 4096)</c> 且為原字串前綴。
        /// 針對超過上限的長字串特別驗證截斷行為。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 1: Log Format Structure and Robustness.
        /// <para><strong>Validates: Requirements 2.7</strong></para>
        /// </remarks>
        [Test]
        public void Log_LongMessage_TruncatedToPrefix()
        {
            // 生成長度介於 0 與 MaxMessageLength 兩倍之間的字串，涵蓋未超過與超過上限兩種情況。
            Arbitrary<string> messageArb = Gen.Choose(0, DevLogger.MaxMessageLength * 2)
                .Select(len => new string('x', len))
                .ToArbitrary();

            Prop.ForAll(
                messageArb,
                message =>
                {
                    string captured = null;
                    ILogSink sink = Substitute.For<ILogSink>();
                    sink.Write(Arg.Do<string>(s => captured = s)).Returns(true);

                    DevLogger logger = new DevLogger(sink);

                    logger.Log(LogCategory.General, message);

                    const string prefix = "[Dev][General] ";
                    if (captured == null || !captured.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        return false;
                    }

                    string emittedMsg = captured.Substring(prefix.Length);
                    int expectedLen = Math.Min(message.Length, DevLogger.MaxMessageLength);

                    return emittedMsg.Length == expectedLen
                        && message.StartsWith(emittedMsg, StringComparison.Ordinal);
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 1（健壯性）：對於任意分類與訊息，即使底層 <see cref="ILogSink"/> 拋出例外或回傳 false，
        /// <see cref="DevLogger.Log"/> 均不得對呼叫端拋出例外（Req 2.8）。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 1: Log Format Structure and Robustness.
        /// <para><strong>Validates: Requirements 2.8</strong></para>
        /// </remarks>
        [Test]
        public void Log_SinkFailsOrThrows_NeverPropagatesException()
        {
            Prop.ForAll(
                GenAnyCategory(),
                GenMessage(),
                Gen.Elements(false, true).ToArbitrary(),
                (category, message, sinkThrows) =>
                {
                    ILogSink sink = Substitute.For<ILogSink>();
                    if (sinkThrows)
                    {
                        sink.Write(Arg.Any<string>()).Returns(_ => throw new InvalidOperationException("sink failure"));
                    }
                    else
                    {
                        sink.Write(Arg.Any<string>()).Returns(false);
                    }

                    DevLogger logger = new DevLogger(sink);

                    try
                    {
                        logger.Log(category, message);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 2: Category Toggle Gate Correctness — 分類開關閘控正確性。
        /// 對於任意分類與任意開關狀態序列，<see cref="DevLogger.Log"/> 寫入底層 <see cref="ILogSink"/> 的次數
        /// 恰等於「呼叫當下該分類開關為開啟」時為 1、否則為 0；且 <see cref="DevLogger.SetCategoryEnabled"/>
        /// 切換後的狀態於後續第一筆該分類日誌請求即生效。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 2: Category Toggle Gate Correctness.
        /// <para><strong>Validates: Requirements 3.1, 3.2, 3.10</strong></para>
        /// </remarks>
        [Test]
        public void Log_ToggleGate_WriteCountMatchesEnabledState()
        {
            // 生成隨機的（分類、目標開關）序列，逐步套用開關並於每步後發出一筆日誌。
            Gen<(LogCategory, bool)> stepGen =
                from category in GenDefinedCategory()
                from enabled in Gen.Elements(false, true)
                select (category, enabled);

            Arbitrary<(LogCategory, bool)[]> sequenceArb = stepGen
                .ArrayOf()
                .Where(arr => arr.Length >= 1 && arr.Length <= 50)
                .ToArbitrary();

            Prop.ForAll(
                sequenceArb,
                sequence =>
                {
                    int writeCount = 0;
                    ILogSink sink = Substitute.For<ILogSink>();
                    sink.Write(Arg.Do<string>(_ => writeCount++)).Returns(true);

                    DevLogger logger = new DevLogger(sink);

                    // 明確載入一份全停用的設定，使開關狀態完全由後續 SetCategoryEnabled 決定，
                    // 避免「載入前全開」的預設狀態干擾閘控計數。
                    logger.LoadToggleConfig(new EmptyConfigLoader());

                    foreach ((LogCategory category, bool enabled) in sequence)
                    {
                        logger.SetCategoryEnabled(category, enabled);
                        writeCount = 0;

                        logger.Log(category, "msg");

                        int expectedThisStep = enabled ? 1 : 0;
                        if (writeCount != expectedThisStep)
                        {
                            return false;
                        }
                    }

                    return true;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 3: Toggle Config Application and Non-Persistence — 開關設定套用與不持久化。
        /// 對於任意啟用清單設定，<see cref="DevLogger.LoadToggleConfig"/> 後任一分類的
        /// <see cref="DevLogger.IsCategoryEnabled"/> 等於「清單是否包含該分類」；重複載入相同設定結果一致（決定性）；
        /// 且任意 <see cref="DevLogger.SetCategoryEnabled"/> 呼叫序列均不對設定來源產生任何寫入。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 3: Toggle Config Application and Non-Persistence.
        /// <para><strong>Validates: Requirements 3.3, 3.4, 3.7, 3.8</strong></para>
        /// </remarks>
        [Test]
        public void LoadToggleConfig_AppliesListAsAuthoritativeAndNeverPersists()
        {
            LogCategory[] allCategories = (LogCategory[])Enum.GetValues(typeof(LogCategory));

            // 生成「啟用分類」子集合（以 bool 遮罩表示每個已定義分類是否啟用）。
            Arbitrary<bool[]> maskArb = Gen.ArrayOf(Gen.Elements(false, true), allCategories.Length)
                .ToArbitrary();

            Prop.ForAll(
                maskArb,
                mask =>
                {
                    Dictionary<LogCategory, bool> config = new Dictionary<LogCategory, bool>();
                    for (int i = 0; i < allCategories.Length; i++)
                    {
                        config[allCategories[i]] = mask[i];
                    }

                    ILogSink sink = Substitute.For<ILogSink>();
                    sink.Write(Arg.Any<string>()).Returns(true);

                    ILogToggleConfigLoader loader = Substitute.For<ILogToggleConfigLoader>();
                    loader.Load().Returns(config);

                    DevLogger logger = new DevLogger(sink);

                    // 第一次載入：套用清單為權威來源（Req 3.3、3.4）。
                    logger.LoadToggleConfig(loader);
                    for (int i = 0; i < allCategories.Length; i++)
                    {
                        if (logger.IsCategoryEnabled(allCategories[i]) != mask[i])
                        {
                            return false;
                        }
                    }

                    // 執行期任意切換皆為暫存，不得寫回設定來源（Req 3.7）。
                    foreach (LogCategory category in allCategories)
                    {
                        logger.SetCategoryEnabled(category, true);
                        logger.SetCategoryEnabled(category, false);
                    }

                    // 重載相同設定：決定性覆蓋任何執行期調整（Req 3.8）。
                    logger.LoadToggleConfig(loader);
                    for (int i = 0; i < allCategories.Length; i++)
                    {
                        if (logger.IsCategoryEnabled(allCategories[i]) != mask[i])
                        {
                            return false;
                        }
                    }

                    // 斷言設定來源僅被 Load 讀取。設定來源介面本身不提供任何寫入 API，
                    // 故 DevLogger 對其僅可能發生讀取（Load）呼叫，結構上即保證零寫入（Req 3.7）。
                    loader.ReceivedWithAnyArgs().Load();
                    return true;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// 建立「任意 <see cref="LogCategory"/>」產生器，混合已定義成員與越界強制轉型的未定義整數值，
        /// 以覆蓋 Req 2.6 之未定義分類預設名路徑。
        /// </summary>
        /// <returns>涵蓋已定義與未定義分類值的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<LogCategory> GenAnyCategory()
        {
            Gen<LogCategory> definedGen = GenDefinedCategory();

            Gen<LogCategory> undefinedGen = Gen.Choose(-100, 100)
                .Where(value => !Enum.IsDefined(typeof(LogCategory), value))
                .Select(value => (LogCategory)value);

            return Gen.OneOf(definedGen, undefinedGen).ToArbitrary();
        }

        /// <summary>
        /// 建立「任意訊息字串」產生器，刻意涵蓋 null、空字串、一般字串、上限邊界長度與超過上限的長字串，
        /// 以完整覆蓋 Req 2.5（null／空）與 Req 2.7（截斷）之輸入空間。
        /// </summary>
        /// <returns>涵蓋各邊界情況的訊息字串 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<string> GenMessage()
        {
            Gen<string> specialGen = Gen.Elements<string>(
                null,
                string.Empty,
                new string('a', DevLogger.MaxMessageLength),
                new string('b', DevLogger.MaxMessageLength + 1));

            Gen<string> arbitraryGen = Gen.Choose(0, DevLogger.MaxMessageLength * 2)
                .Select(len => new string('c', len));

            return Gen.OneOf(specialGen, arbitraryGen).ToArbitrary();
        }

        /// <summary>
        /// 建立「已定義 <see cref="LogCategory"/>」產生器，僅取列舉已定義成員。
        /// </summary>
        /// <returns>已定義分類值的 <see cref="Gen{T}"/>。</returns>
        private static Gen<LogCategory> GenDefinedCategory()
        {
            LogCategory[] values = (LogCategory[])Enum.GetValues(typeof(LogCategory));
            return Gen.Elements(values);
        }

        /// <summary>
        /// 回傳空對映的設定載入器，使 <see cref="DevLogger.LoadToggleConfig"/> 後所有分類皆為停用，
        /// 供閘控計數測試建立確定的初始狀態。
        /// </summary>
        private sealed class EmptyConfigLoader : ILogToggleConfigLoader
        {
            /// <inheritdoc />
            public IReadOnlyDictionary<LogCategory, bool> Load()
            {
                return new Dictionary<LogCategory, bool>();
            }
        }
    }
}
