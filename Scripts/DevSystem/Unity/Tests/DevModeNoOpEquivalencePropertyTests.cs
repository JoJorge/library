namespace DevSystem.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;

    /// <summary>
    /// Property 11: DEV_MODE No-Op Equivalence 的屬性導向測試（Property-Based Test）。
    /// 使用 FsCheck 3.x（<c>FsCheck.Fluent</c>）於標準 NUnit <c>[Test]</c> 方法內直接呼叫 FsCheck API，
    /// 驗證在 **未定義 <c>DEV_MODE</c>** 的組態下，對任意引數呼叫 DevSystem 各具回傳值的公開 API 恆回傳
    /// <c>default(T)</c>，且呼叫前後對系統的查詢無任何可觀察的狀態變更（需求 1.2、1.3、1.5）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>DEV_MODE-guarding 策略：</strong>整個測試 assembly（<c>DevSystem.Unity.Tests</c>）於一般組態下
    /// **不**定義 <c>DEV_MODE</c>，此為驗證空實作等價性的目標組態。為使本測試在兩種組態下皆能編譯，
    /// 每個屬性的斷言主體以 <c>#if !DEV_MODE ... #else Assert.Ignore(...) #endif</c> 包裹：
    /// </para>
    /// <list type="bullet">
    /// <item>未定義 <c>DEV_MODE</c>（目標組態）：執行完整的 no-op 等價性斷言。</item>
    /// <item>已定義 <c>DEV_MODE</c>：API 會執行真實邏輯，no-op 等價性不成立，故以
    /// <see cref="Assert.Ignore(string)"/> 略過，避免誤判失敗。</item>
    /// </list>
    /// <para>
    /// 本測試置於 Unity 測試 assembly，以便同時涵蓋純邏輯（<see cref="DevLogger"/>、
    /// <see cref="DevKeyBinder"/>、<see cref="PanelTreeBuilder"/>）與 Unity 相依
    /// （<see cref="DevSystemManager"/>、<see cref="DevPanel"/>、內建分頁）兩類具回傳值的 API。
    /// 每個屬性最少執行 100 次隨機迭代（<c>QuickCheckThrowOnFailure</c> 預設 MaxTest = 100）。
    /// </para>
    /// <para><strong>Validates: Requirements 1.2, 1.3, 1.5</strong></para>
    /// </remarks>
    [TestFixture]
    public class DevModeNoOpEquivalencePropertyTests
    {
        /// <summary>
        /// Property 11: DEV_MODE No-Op Equivalence — 空實作等價性。
        /// 對於任意引數，未定義 <c>DEV_MODE</c> 時 DevSystem 各具回傳值 API 恆回傳 <c>default(T)</c>，
        /// 且呼叫前後對系統的查詢無可觀察狀態變更。此測試涵蓋純邏輯與 Unity 相依兩類 API。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 11: DEV_MODE No-Op Equivalence.
        /// <para><strong>Validates: Requirements 1.2, 1.3, 1.5</strong></para>
        /// </remarks>
        [Test]
        public void AllReturnValueApis_NoOpAndReturnDefault_WhenDevModeUndefined()
        {
#if !DEV_MODE
            Prop.ForAll(
                GenArgs(),
                args =>
                {
                    // ---- 純邏輯：DevLogger（sink 傳 null 亦不應被觸碰，因主體為空） ----
                    var logger = new DevLogger(null);

                    bool setBefore = logger.SetCategoryEnabled(args.Category, true);
                    bool isEnabled = logger.IsCategoryEnabled(args.Category);
                    IReadOnlyList<LogCategory> categories = logger.GetCategories();

                    // 前後查詢一致：SetCategoryEnabled 為 no-op，故查詢結果不因寫入而改變。
                    bool isEnabledAfterSet = logger.IsCategoryEnabled(args.Category);

                    if (setBefore != default(bool)
                        || isEnabled != default(bool)
                        || categories != null
                        || isEnabledAfterSet != isEnabled)
                    {
                        return false.Label("DevLogger return-value API not no-op");
                    }

                    // ---- 純邏輯：DevKeyBinder（BindingManager/ConflictChecker 傳 null，主體為空不觸碰） ----
                    var keyBinder = new DevKeyBinder(null, null);

                    IReadOnlyList<DevKeyRegistration> regsBefore = keyBinder.GetRegistrations();
                    DevKeyBindResult register =
                        keyBinder.RegisterDevKey(args.Action, args.BindingPath, args.Description);
                    bool unbind = keyBinder.Unbind(args.Action);
                    bool clear = keyBinder.ClearAll();
                    IReadOnlyList<DevKeyRegistration> regsAfter = keyBinder.GetRegistrations();

                    if (register != null
                        || unbind != default(bool)
                        || clear != default(bool)
                        || regsBefore != null
                        || regsAfter != null)
                    {
                        return false.Label("DevKeyBinder return-value API not no-op");
                    }

                    // ---- 純邏輯：PanelTreeBuilder（static） ----
                    var definition = new PanelDefinition(new List<PanelNodeDefinition>
                    {
                        new PanelNodeDefinition(args.Description ?? string.Empty, null, null),
                    });

                    bool tryBuild = PanelTreeBuilder.TryBuild(definition, out IReadOnlyList<PanelNode> roots);
                    if (tryBuild != default(bool) || roots != null)
                    {
                        return false.Label("PanelTreeBuilder.TryBuild not no-op");
                    }

                    // ---- Unity 相依：DevPanel ----
                    var panel = new DevPanel();

                    IReadOnlyList<PanelNode> tabsBefore = panel.GetTabs();
                    bool build = panel.Build(definition);
                    bool release = panel.Release();
                    IReadOnlyList<PanelNode> tabsAfter = panel.GetTabs();

                    if (build != default(bool)
                        || release != default(bool)
                        || tabsBefore != null
                        || tabsAfter != null)
                    {
                        return false.Label("DevPanel return-value API not no-op");
                    }

                    // ---- Unity 相依：內建分頁（具回傳值成員：Title、BuildContent） ----
                    var keyListTab = new DevKeyListTab(keyBinder);
                    var logSettingsTab = new DebugLogSettingsTab(logger);

                    if (keyListTab.Title != null
                        || keyListTab.BuildContent() != null
                        || logSettingsTab.Title != null
                        || logSettingsTab.BuildContent() != null)
                    {
                        return false.Label("Built-in tab return-value API not no-op");
                    }

                    // ---- Unity 相依：DevSystemManager（Singleton 生命週期與屬性 API） ----
                    DevSystemManager manager = DevSystemManager.Instance;

                    DevSystemState stateBefore = manager.State;
                    bool init = manager.Init(definition);
                    bool releaseManager = manager.Release();
                    DevSystemState stateAfter = manager.State;

                    if (init != default(bool)
                        || releaseManager != default(bool)
                        || stateBefore != default(DevSystemState)
                        || stateAfter != default(DevSystemState)
                        || manager.Logger != null
                        || manager.KeyBinder != null
                        || manager.Panel != null)
                    {
                        return false.Label("DevSystemManager return-value API not no-op");
                    }

                    return true.ToProperty();
                }).QuickCheckThrowOnFailure();
#else
            Assert.Ignore(
                "Property 11 (DEV_MODE No-Op Equivalence) 僅於未定義 DEV_MODE 的組態下有意義；"
                + "目前組態已定義 DEV_MODE，API 會執行真實邏輯，故略過。");
#endif
        }

        /// <summary>
        /// 建立涵蓋 DevSystem 各具回傳值 API 所需引數的隨機產生器：
        /// 任意 <see cref="LogCategory"/>（含越界強制轉型）、任意 <see cref="DevAction"/>、
        /// 任意 Binding Path 字串（含 null／空／有效／無效）、任意敘述字串（含 null／空）。
        /// </summary>
        /// <returns>承載一組隨機引數的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<Args> GenArgs()
        {
            Gen<LogCategory> definedCategory =
                Gen.Elements((LogCategory[])Enum.GetValues(typeof(LogCategory)));
            Gen<LogCategory> undefinedCategory = Gen.Choose(-100, 100)
                .Where(value => !Enum.IsDefined(typeof(LogCategory), value))
                .Select(value => (LogCategory)value);
            Gen<LogCategory> categoryGen = Gen.OneOf(definedCategory, undefinedCategory);

            Gen<DevAction> actionGen =
                Gen.Elements((DevAction[])Enum.GetValues(typeof(DevAction)));

            // Binding Path：涵蓋 null、空、有效鍵盤/搖桿路徑與無法解析的任意字串。
            Gen<string> pathGen = Gen.Elements<string>(
                null,
                string.Empty,
                "<Keyboard>/f1",
                "<Gamepad>/buttonSouth",
                "not-a-valid-path");

            Gen<string> descGen = Gen.Elements<string>(null, string.Empty, "desc", "開發者按鍵");

            return (from category in categoryGen
                    from action in actionGen
                    from path in pathGen
                    from desc in descGen
                    select new Args(category, action, path, desc)).ToArbitrary();
        }

        /// <summary>
        /// 承載單次屬性迭代所用的隨機引數集合。
        /// </summary>
        private readonly struct Args
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Args"/> struct.
            /// </summary>
            /// <param name="category">任意日誌分類（含越界值）。</param>
            /// <param name="action">任意開發者動作。</param>
            /// <param name="bindingPath">任意按鍵路徑（含 null／空／有效／無效）。</param>
            /// <param name="description">任意敘述字串（含 null／空）。</param>
            public Args(LogCategory category, DevAction action, string bindingPath, string description)
            {
                this.Category = category;
                this.Action = action;
                this.BindingPath = bindingPath;
                this.Description = description;
            }

            /// <summary>Gets 任意日誌分類。</summary>
            public LogCategory Category { get; }

            /// <summary>Gets 任意開發者動作。</summary>
            public DevAction Action { get; }

            /// <summary>Gets 任意按鍵路徑。</summary>
            public string BindingPath { get; }

            /// <summary>Gets 任意敘述字串。</summary>
            public string Description { get; }
        }
    }
}
