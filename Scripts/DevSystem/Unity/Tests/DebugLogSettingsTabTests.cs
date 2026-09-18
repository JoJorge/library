namespace DevSystem.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DevSystem.Unity;
    using FsCheck;
    using FsCheck.Fluent;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// <see cref="DebugLogSettingsTab"/> 的屬性導向測試（PBT）與範例導向測試。
    /// 這些屬性驗證的是 <c>DEV_MODE</c> 已定義時的行為（分頁的切換與同步邏輯僅於 <c>DEV_MODE</c> 生效時存在）。
    /// 以真實 <see cref="DevLogger"/>（搭配 <see cref="ILogSink"/> 替身）作為模型來源，透過設定使用者於
    /// UI Toolkit <see cref="Toggle"/> 的 <see cref="Toggle.value"/> 觸發切換回呼，驗證 UI 與模型狀態的同步。
    ///
    /// Feature: dev-system, Property 9: Log Settings UI-Model Sync.
    /// </summary>
    [TestFixture]
    public class DebugLogSettingsTabTests
    {
        /// <summary>
        /// 切換某分類開關更新失敗時 <see cref="DebugLogSettingsTab"/> 顯示的錯誤提示（對應其內部常數，需求 9.7）。
        /// </summary>
        private const string UpdateFailedMessage = "Failed to update log category toggle.";

        /// <summary>
        /// 全部已定義的 <see cref="LogCategory"/> 列舉值。
        /// </summary>
        private static readonly LogCategory[] AllCategories =
            (LogCategory[])Enum.GetValues(typeof(LogCategory));

        /// <summary>
        /// 承載受測分頁內容的宿主視窗，使其 <see cref="Toggle"/> 附著於真實 UI Toolkit panel。
        /// </summary>
        /// <remarks>
        /// UI Toolkit 的 <see cref="Toggle.value"/> setter 僅在元素已附著於 panel 時才會派發
        /// <c>ChangeEvent</c> 並觸發 <c>RegisterValueChangedCallback</c>（未附著的元素僅更新內部值而不派發事件）。
        /// 為忠實模擬使用者於執行期於面板中的切換操作（需求 9.4、9.5、9.7），需將分頁內容附著於一個
        /// 具備 panel 的 <see cref="EditorWindow"/> 後再設定 <c>value</c>。
        /// </remarks>
        private HostWindow _host;

        /// <summary>
        /// 於每個測試後關閉宿主視窗，避免視窗與其 panel 殘留。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (this._host != null)
            {
                this._host.Close();
                this._host = null;
            }
        }

        // =========================================================
        // Property 9: Log Settings UI-Model Sync
        // =========================================================

        /// <summary>
        /// Property 9: Log Settings UI-Model Sync — 日誌設定 UI 與模型同步。
        /// 對於任意 <see cref="LogCategory"/> 與任意切換目標狀態序列，當使用者於 <see cref="DebugLogSettingsTab"/>
        /// 切換某列時，元件以 <see cref="DevLogger.SetCategoryEnabled"/> 更新，且更新成功後該列於同影格顯示的狀態
        /// 等於 <see cref="DevLogger.IsCategoryEnabled"/> 回報值；於外部直接變更 <see cref="DevLogger"/> 後執行
        /// <see cref="DebugLogSettingsTab.OnUpdate"/>，各列顯示狀態應同步為 <see cref="DevLogger"/> 目前狀態。
        /// **Validates: Requirements 9.2, 9.4, 9.5, 9.6**
        /// </summary>
        [Test]
        public void Property9_UiSyncsWithModelOnToggleAndUpdate()
        {
            // 生成一序列 (分類索引, 目標狀態) 的使用者切換步驟，以及一份用於「外部變更」的目標遮罩。
            Gen<(int, bool)> stepGen =
                from categoryIdx in Gen.Choose(0, AllCategories.Length - 1)
                from target in Gen.Elements(false, true)
                select (categoryIdx, target);

            var arb = Arb.From(
                from steps in stepGen.ArrayOf().Where(a => a.Length >= 1 && a.Length <= 30)
                from externalMask in Gen.ArrayOf(Gen.Elements(false, true), AllCategories.Length)
                select (steps, externalMask));

            Prop.ForAll(arb, data =>
            {
                DevLogger logger = CreateLogger();

                // 明確載入一份全停用設定，使開關狀態完全由後續操作決定（避免「載入前全開」預設干擾）。
                logger.LoadToggleConfig(new MaskConfigLoader(AllZeros()));

                var tab = new DebugLogSettingsTab(logger);
                VisualElement root = tab.BuildContent();

                // 附著於 panel，使 Toggle.value 變更得以派發 ChangeEvent 並觸發切換回呼。
                this.AttachToPanel(root);

                Dictionary<LogCategory, Toggle> toggles = MapToggles(root);

                // 使用者切換：逐步設定該列 Toggle.value 觸發回呼，驗證同影格顯示 == IsCategoryEnabled（需求 9.4、9.5）。
                foreach ((int categoryIdx, bool target) in data.steps)
                {
                    LogCategory category = AllCategories[categoryIdx];
                    Toggle toggle = toggles[category];

                    // 設定 value 會觸發 RegisterValueChangedCallback，模擬使用者操作。
                    toggle.value = target;

                    if (toggle.value != logger.IsCategoryEnabled(category))
                    {
                        return false.Label($"row {category} must match IsCategoryEnabled after toggle");
                    }
                }

                // 外部來源變更：直接改 DevLogger 狀態後執行 OnUpdate，驗證各列同步（需求 9.6）。
                for (int i = 0; i < AllCategories.Length; i++)
                {
                    logger.SetCategoryEnabled(AllCategories[i], data.externalMask[i]);
                }

                tab.OnUpdate();

                for (int i = 0; i < AllCategories.Length; i++)
                {
                    LogCategory category = AllCategories[i];
                    if (toggles[category].value != logger.IsCategoryEnabled(category))
                    {
                        return false.Label($"row {category} must sync to model on OnUpdate");
                    }
                }

                return true.Label("UI syncs with model on toggle and update");
            }).QuickCheckThrowOnFailure();
        }

        // =========================================================
        // Example tests
        // =========================================================

        /// <summary>
        /// 空狀態：可用 <see cref="LogCategory"/> 集合為空時，顯示空狀態訊息且不列出任何 <see cref="Toggle"/>。
        /// 以回傳空分類清單的 <see cref="DevLogger"/> 替身驅動此邊界。
        /// **Validates: Requirements 9.3**
        /// </summary>
        [Test]
        public void EmptyCategories_ShowsEmptyStateAndNoToggles()
        {
            // GetCategories 由列舉推導，恆非空；以替身覆寫回傳空集合以驅動空狀態分支。
            DevLogger logger = Substitute.For<DevLogger>(CreateSink());
            logger.GetCategories().Returns(Array.Empty<LogCategory>());

            var tab = new DebugLogSettingsTab(logger);
            VisualElement root = tab.BuildContent();

            List<Toggle> toggles = root.Query<Toggle>().ToList();
            Assert.That(toggles, Is.Empty, "空分類集合不應列出任何 Toggle");

            List<string> labels = root.Query<Label>().ToList().Select(l => l.text).ToList();
            Assert.That(labels, Does.Contain("No log categories available."), "應顯示空狀態訊息");
        }

        /// <summary>
        /// 切換更新失敗：<see cref="DevLogger.SetCategoryEnabled"/> 回傳 false 時，該列還原為切換前狀態並顯示錯誤提示。
        /// 以替身將 <see cref="DevLogger.SetCategoryEnabled"/> 設為回傳 false 模擬更新失敗（需求 9.7）。
        /// **Validates: Requirements 9.7**
        /// </summary>
        [Test]
        public void ToggleUpdateFails_RevertsRowAndShowsError()
        {
            DevLogger logger = Substitute.For<DevLogger>(CreateSink());

            // 使 GetCategories/IsCategoryEnabled 有確定行為，並使 SetCategoryEnabled 一律失敗。
            logger.GetCategories().Returns(AllCategories);
            logger.IsCategoryEnabled(Arg.Any<LogCategory>()).Returns(false);
            logger.SetCategoryEnabled(Arg.Any<LogCategory>(), Arg.Any<bool>()).Returns(false);

            var tab = new DebugLogSettingsTab(logger);
            VisualElement root = tab.BuildContent();

            // 附著於 panel，使 Toggle.value 變更得以派發 ChangeEvent 並觸發切換回呼。
            this.AttachToPanel(root);

            Dictionary<LogCategory, Toggle> toggles = MapToggles(root);

            LogCategory target = AllCategories[0];
            Toggle toggle = toggles[target];

            // 初始為 false（IsCategoryEnabled 回傳 false）；使用者嘗試切為 true → 更新失敗 → 應還原為 false。
            Assert.That(toggle.value, Is.False, "初始列狀態應為停用");

            toggle.value = true;

            Assert.That(toggle.value, Is.False, "更新失敗時該列應還原為切換前狀態");

            Label errorLabel = root.Query<Label>()
                .ToList()
                .FirstOrDefault(l => l.text == UpdateFailedMessage);
            Assert.That(errorLabel, Is.Not.Null, "應存在錯誤提示標籤");
            Assert.That(
                errorLabel.style.display.value,
                Is.EqualTo(DisplayStyle.Flex),
                "更新失敗時錯誤提示應顯示");
        }

        /// <summary>
        /// 內建分頁掛入：<see cref="DebugLogSettingsTab"/> 可作為 <see cref="PanelNodeDefinition.Tab"/>（以 object 承載）掛入，
        /// 且取回後可還原為 <see cref="IDevPanelTab"/> 契約使用。
        /// **Validates: Requirements 9.1**
        /// </summary>
        [Test]
        public void DebugLogSettingsTab_CanBeMountedAsPanelNodeDefinitionTab()
        {
            DevLogger logger = CreateLogger();
            var tab = new DebugLogSettingsTab(logger);

            var node = new PanelNodeDefinition("Log Settings", tab, null);

            Assert.That(node.Tab, Is.SameAs(tab));
            Assert.That(node.Tab, Is.InstanceOf<IDevPanelTab>());

            var mounted = (IDevPanelTab)node.Tab;
            Assert.That(mounted.Title, Is.EqualTo("Log Settings"));
            Assert.That(mounted.BuildContent(), Is.Not.Null);
        }

        // =========================================================
        // Helpers
        // =========================================================

        /// <summary>
        /// 建立回傳成功的 <see cref="ILogSink"/> 替身。
        /// </summary>
        /// <returns>寫入恆成功的 <see cref="ILogSink"/>。</returns>
        private static ILogSink CreateSink()
        {
            ILogSink sink = Substitute.For<ILogSink>();
            sink.Write(Arg.Any<string>()).Returns(true);
            return sink;
        }

        /// <summary>
        /// 建立一個以成功 <see cref="ILogSink"/> 替身承接輸出的真實 <see cref="DevLogger"/>。
        /// </summary>
        /// <returns>可供切換與查詢分類開關的 <see cref="DevLogger"/>。</returns>
        private static DevLogger CreateLogger()
        {
            return new DevLogger(CreateSink());
        }

        /// <summary>
        /// 將分頁內容根元素附著於宿主視窗的 panel，使其 <see cref="Toggle"/> 的
        /// <c>value</c> 變更得以派發 <c>ChangeEvent</c>（忠實模擬執行期面板中的使用者操作）。
        /// </summary>
        /// <param name="root">要附著的分頁內容根元素。</param>
        private void AttachToPanel(VisualElement root)
        {
            if (this._host == null)
            {
                this._host = EditorWindow.GetWindow<HostWindow>();
            }

            this._host.rootVisualElement.Clear();
            this._host.rootVisualElement.Add(root);
        }

        /// <summary>
        /// 由分頁內容根元素蒐集各 <see cref="LogCategory"/> 對應的 <see cref="Toggle"/>。
        /// Toggle 標籤文字即分類名稱（<see cref="DebugLogSettingsTab.BuildContent"/> 以 <c>category.ToString()</c> 命名）。
        /// </summary>
        /// <param name="root">分頁內容根元素。</param>
        /// <returns>分類到 Toggle 的對映。</returns>
        private static Dictionary<LogCategory, Toggle> MapToggles(VisualElement root)
        {
            var map = new Dictionary<LogCategory, Toggle>();
            foreach (Toggle toggle in root.Query<Toggle>().ToList())
            {
                if (Enum.TryParse(toggle.label, out LogCategory category))
                {
                    map[category] = toggle;
                }
            }

            return map;
        }

        /// <summary>
        /// 建立所有分類皆停用的設定字典，供載入確定初始狀態。
        /// </summary>
        /// <returns>各分類對映 false 的設定字典。</returns>
        private static Dictionary<LogCategory, bool> AllZeros()
        {
            var config = new Dictionary<LogCategory, bool>();
            foreach (LogCategory category in AllCategories)
            {
                config[category] = false;
            }

            return config;
        }

        /// <summary>
        /// 承載受測分頁內容的 <see cref="EditorWindow"/>，提供真實 UI Toolkit panel 供事件派發。
        /// </summary>
        public sealed class HostWindow : EditorWindow
        {
        }

        /// <summary>
        /// 以固定字典內容回應 <see cref="ILogToggleConfigLoader.Load"/> 的設定載入器，用於建立確定初始開關狀態。
        /// </summary>
        private sealed class MaskConfigLoader : ILogToggleConfigLoader
        {
            private readonly IReadOnlyDictionary<LogCategory, bool> config;

            /// <summary>
            /// Initializes a new instance of the <see cref="MaskConfigLoader"/> class.
            /// </summary>
            /// <param name="config">各分類的初始開關狀態。</param>
            public MaskConfigLoader(IReadOnlyDictionary<LogCategory, bool> config)
            {
                this.config = config;
            }

            /// <inheritdoc/>
            public IReadOnlyDictionary<LogCategory, bool> Load()
            {
                return this.config;
            }
        }
    }
}
