namespace DevSystem.Unity.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;

    /// <summary>
    /// DevSystem 公開 API 的呼叫端範例，作為需求 1.4、1.5 的編譯期驗證。
    /// 本範例的呼叫語句於 **未定義 <c>DEV_MODE</c>** 的組態下編譯並執行：其目的在於證明外部呼叫端在未定義
    /// <c>DEV_MODE</c> 的組態下仍可引用並呼叫 DevSystem 的所有公開 API，且編譯零錯誤、零警告（需求 1.4），
    /// 同時所有具回傳值的 API 皆回傳其型別的 <c>default</c>（需求 1.5）。
    /// </summary>
    /// <remarks>
    /// 此為編譯導向的煙霧測試（smoke test），非行為測試；DevSystem 的行為驗證見 checkpoint 任務 9。
    /// <para>
    /// <strong>DEV_MODE-guarding 策略：</strong>專案的實際組態定義了 <c>DEV_MODE</c>，此時各 API 執行真實邏輯，
    /// <c>default</c> 回傳等價性不成立，且 <see cref="DevSystemManager.Init"/> 會存取尚未初始化的
    /// <c>InputSystemManager</c> 等外部相依而不適合於此煙霧測試中執行。故本範例以
    /// <c>#if !DEV_MODE ... #else Assert.Ignore(...) #endif</c> 閘控：
    /// </para>
    /// <list type="bullet">
    /// <item>未定義 <c>DEV_MODE</c>（本範例的目標組態）：執行呼叫端語句並斷言回傳 <c>default</c>（需求 1.4、1.5）。</item>
    /// <item>已定義 <c>DEV_MODE</c>：呼叫端語句仍存在於原始碼（需求 1.4 的編譯可引用性由 DEV_MODE-undefined 建置驗證），
    /// 執行期斷言以 <see cref="Assert.Ignore(string)"/> 略過。</item>
    /// </list>
    /// </remarks>
    [TestFixture]
    public class DevSystemApiCallerSample
    {
        /// <summary>
        /// 呼叫 <see cref="DevSystemManager"/> 的生命週期與屬性 API。
        /// 驗證未定義 <c>DEV_MODE</c> 時，具回傳值的成員皆回傳 <c>default</c>（需求 1.5）。
        /// </summary>
        [Test]
        public void DevSystemManagerApis_CompileAndReturnDefaults()
        {
#if !DEV_MODE
            DevSystemManager manager = DevSystemManager.Instance;

            var definition = new PanelDefinition(new List<PanelNodeDefinition>
            {
                new PanelNodeDefinition("Sample Tab", null, null),
            });

            bool initialized = manager.Init(definition);
            bool released = manager.Release();
            DevSystemState state = manager.State;

            DevLogger logger = manager.Logger;
            DevKeyBinder keyBinder = manager.KeyBinder;
            DevPanel panel = manager.Panel;

            // 需求 1.5：未定義 DEV_MODE 時所有具回傳值成員回傳 default。
            Assert.AreEqual(default(bool), initialized);
            Assert.AreEqual(default(bool), released);
            Assert.AreEqual(default(DevSystemState), state);
            Assert.AreEqual(default(DevLogger), logger);
            Assert.AreEqual(default(DevKeyBinder), keyBinder);
            Assert.AreEqual(default(DevPanel), panel);
#else
            Assert.Ignore("Caller sample default-return assertions only apply when DEV_MODE is undefined.");
#endif
        }

        /// <summary>
        /// 引用 <see cref="DevLogger"/> 的所有公開 API，驗證呼叫端可編譯（需求 1.4）。
        /// </summary>
        [Test]
        public void DevLoggerApis_CompileCleanly()
        {
#if !DEV_MODE
            DevLogger logger = DevSystemManager.Instance.Logger;

            // Log 標註 [Conditional("DEV_MODE")]；未定義時呼叫點於編譯期移除，仍保留以驗證可被引用。
            logger?.Log(LogCategory.General, "sample message");

            bool setResult = logger?.SetCategoryEnabled(LogCategory.Network, false) ?? default;
            bool isEnabled = logger?.IsCategoryEnabled(LogCategory.Gameplay) ?? default;
            IReadOnlyList<LogCategory> categories = logger?.GetCategories();
            logger?.LoadToggleConfig(null);

            Assert.AreEqual(default(bool), setResult);
            Assert.AreEqual(default(bool), isEnabled);
            Assert.AreEqual(default(IReadOnlyList<LogCategory>), categories);
#else
            Assert.Ignore("Caller sample default-return assertions only apply when DEV_MODE is undefined.");
#endif
        }

        /// <summary>
        /// 引用 <see cref="DevKeyBinder"/> 的所有公開 API，驗證呼叫端可編譯（需求 1.4）。
        /// </summary>
        [Test]
        public void DevKeyBinderApis_CompileCleanly()
        {
#if !DEV_MODE
            DevKeyBinder keyBinder = DevSystemManager.Instance.KeyBinder;

            DevKeyBindResult registerResult =
                keyBinder?.RegisterDevKey(DevAction.DevTogglePanel, "<Keyboard>/f1", "Toggle panel");
            bool unbindResult = keyBinder?.Unbind(DevAction.DevReloadScene) ?? default;
            IReadOnlyList<DevKeyRegistration> registrations = keyBinder?.GetRegistrations();
            bool clearResult = keyBinder?.ClearAll() ?? default;

            Assert.AreEqual(default(DevKeyBindResult), registerResult);
            Assert.AreEqual(default(bool), unbindResult);
            Assert.AreEqual(default(IReadOnlyList<DevKeyRegistration>), registrations);
            Assert.AreEqual(default(bool), clearResult);
#else
            Assert.Ignore("Caller sample default-return assertions only apply when DEV_MODE is undefined.");
#endif
        }

        /// <summary>
        /// 引用 <see cref="DevPanel"/> 的所有公開 API，驗證呼叫端可編譯（需求 1.4）。
        /// </summary>
        [Test]
        public void DevPanelApis_CompileCleanly()
        {
#if !DEV_MODE
            DevPanel panel = DevSystemManager.Instance.Panel;

            var definition = new PanelDefinition(new List<PanelNodeDefinition>
            {
                new PanelNodeDefinition("Sample Tab", null, null),
            });

            bool buildResult = panel?.Build(definition) ?? default;
            bool releaseResult = panel?.Release() ?? default;
            IReadOnlyList<PanelNode> tabs = panel?.GetTabs();

            Assert.AreEqual(default(bool), buildResult);
            Assert.AreEqual(default(bool), releaseResult);
            Assert.AreEqual(default(IReadOnlyList<PanelNode>), tabs);
#else
            Assert.Ignore("Caller sample default-return assertions only apply when DEV_MODE is undefined.");
#endif
        }
    }
}
