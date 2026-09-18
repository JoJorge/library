namespace DevSystem.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DevSystem.Unity;
    using FsCheck;
    using FsCheck.Fluent;
    using InputSystem;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    /// <summary>
    /// <see cref="DevKeyListTab"/> 的屬性導向測試（PBT）與範例導向測試。
    /// 這些屬性驗證的是 <c>DEV_MODE</c> 已定義時的行為（分頁的顯示邏輯僅於 <c>DEV_MODE</c> 生效時存在）。
    /// 透過 NSubstitute 對 <see cref="BindingManager"/> 與 <see cref="ConflictChecker"/> 建立測試替身，
    /// 組出真實 <see cref="DevKeyBinder"/> 作為資料來源，並以 UI Toolkit <see cref="Label"/> 的文字內容
    /// 驗證分頁顯示與 <see cref="DevKeyBinder.GetRegistrations"/> 的一致性。
    ///
    /// Feature: dev-system, Property 8: Dev Key List Ordering and Currency.
    /// </summary>
    [TestFixture]
    public class DevKeyListTabTests
    {
        /// <summary>
        /// 單筆項目中綁定按鍵與功能敘述之間的分隔字串（對應 <see cref="DevKeyListTab"/> 內部常數）。
        /// </summary>
        private const string EntrySeparator = " — ";

        /// <summary>
        /// 清單為空時 <see cref="DevKeyListTab"/> 顯示的提示訊息（對應其內部常數，需求 8.4）。
        /// </summary>
        private const string EmptyMessage = "目前沒有任何已註冊的開發者按鍵。";

        /// <summary>
        /// 全部可用的 <see cref="DevAction"/> 列舉值，作為唯一 action 的來源。
        /// </summary>
        private static readonly DevAction[] AllActions =
            (DevAction[])Enum.GetValues(typeof(DevAction));

        /// <summary>
        /// 有效的按鍵路徑集合，皆可經路徑前綴解析為有效裝置類型，供 <see cref="DevKeyBinder.RegisterDevKey"/> 成功寫入。
        /// 特意涵蓋不同 ordinal 排序關係（大小寫、符號）以驗證排序行為。
        /// </summary>
        private static readonly string[] ValidKeyboardPaths =
        {
            "<Keyboard>/a",
            "<Keyboard>/B",
            "<Keyboard>/c",
            "<Keyboard>/Z",
            "<Keyboard>/space",
            "<Keyboard>/enter",
            "<Mouse>/leftButton",
            "<Gamepad>/buttonSouth",
        };

        // =========================================================
        // Property 8: Dev Key List Ordering and Currency
        // =========================================================

        /// <summary>
        /// Property 8: Dev Key List Ordering and Currency — 按鍵列表排序與即時性。
        /// 對於任意 <see cref="DevKeyRegistration"/> 集合，<see cref="DevKeyListTab"/> 顯示時列出的項目
        /// 應與 <see cref="DevKeyBinder.GetRegistrations"/> 於顯示當下回傳的集合一一對應，並依各項
        /// <see cref="DevKeyRegistration.BindingPath"/> 以 ordinal 升冪排序；於顯示前後變更清單並再次顯示，
        /// 列出的項目恆反映顯示當下的最新註冊清單。
        /// **Validates: Requirements 8.2, 8.3, 8.5**
        /// </summary>
        [Test]
        public void Property8_ListReflectsSortedCurrentRegistrations()
        {
            // 生成兩批「具唯一 DevAction、有效路徑」的註冊請求：第一批用於初次顯示，
            // 第二批用於顯示後變更清單，藉此驗證再次顯示時反映最新（需求 8.5）。
            var arb = Arb.From(
                from firstBatch in GenRegistrationBatch()
                from secondBatch in GenRegistrationBatch()
                select (firstBatch, secondBatch));

            Prop.ForAll(arb, data =>
            {
                DevKeyBinder binder = CreateBinder();
                var tab = new DevKeyListTab(binder);
                VisualElement root = tab.BuildContent();

                // 第一批註冊 → 顯示 → 驗證與當下 GetRegistrations 排序後一一對應。
                RegisterAll(binder, data.firstBatch);
                tab.OnShown();
                if (!VerifyListMatchesRegistrations(root, binder))
                {
                    return false.Label("first display must match sorted current registrations");
                }

                // 顯示後變更清單：清掉並套用第二批，再次顯示 → 驗證反映最新（需求 8.5）。
                binder.ClearAll();
                RegisterAll(binder, data.secondBatch);
                tab.OnShown();
                if (!VerifyListMatchesRegistrations(root, binder))
                {
                    return false.Label("re-display must reflect the latest registrations");
                }

                return true.Label("list reflects sorted current registrations");
            }).QuickCheckThrowOnFailure();
        }

        // =========================================================
        // Example tests
        // =========================================================

        /// <summary>
        /// 空清單提示：無任何註冊時顯示空清單提示訊息，且僅有該一筆提示、無任何項目列。
        /// **Validates: Requirements 8.4**
        /// </summary>
        [Test]
        public void EmptyRegistrations_ShowsEmptyMessageAndZeroEntries()
        {
            DevKeyBinder binder = CreateBinder();
            var tab = new DevKeyListTab(binder);
            VisualElement root = tab.BuildContent();

            tab.OnShown();

            List<string> labels = CollectLabelTexts(root);
            Assert.That(labels, Has.Count.EqualTo(1), "空清單應僅顯示一則提示訊息");
            Assert.That(labels[0], Is.EqualTo(EmptyMessage));
            Assert.That(labels.Any(t => t.Contains(EntrySeparator)), Is.False, "空清單不應出現任何項目列");
        }

        /// <summary>
        /// 內建分頁掛入：<see cref="DevKeyListTab"/> 可作為 <see cref="PanelNodeDefinition.Tab"/>（以 object 承載）掛入，
        /// 且取回後可還原為 <see cref="IDevPanelTab"/> 契約使用。
        /// **Validates: Requirements 8.1**
        /// </summary>
        [Test]
        public void DevKeyListTab_CanBeMountedAsPanelNodeDefinitionTab()
        {
            DevKeyBinder binder = CreateBinder();
            var tab = new DevKeyListTab(binder);

            var node = new PanelNodeDefinition("Dev Keys", tab, null);

            Assert.That(node.Tab, Is.SameAs(tab));
            Assert.That(node.Tab, Is.InstanceOf<IDevPanelTab>());

            var mounted = (IDevPanelTab)node.Tab;
            Assert.That(mounted.Title, Is.EqualTo("Dev Keys"));
            Assert.That(mounted.BuildContent(), Is.Not.Null);
        }

        // =========================================================
        // Helpers
        // =========================================================

        /// <summary>
        /// 收集 <paramref name="root"/> 子樹中所有 <see cref="Label"/> 的文字，依 UI 層級順序回傳。
        /// </summary>
        /// <param name="root">內容根元素。</param>
        /// <returns>依顯示順序排列的 Label 文字清單。</returns>
        private static List<string> CollectLabelTexts(VisualElement root)
        {
            var texts = new List<string>();
            foreach (Label label in root.Query<Label>().ToList())
            {
                texts.Add(label.text);
            }

            return texts;
        }

        /// <summary>
        /// 驗證分頁目前顯示的項目列與 <paramref name="binder"/> 當下的註冊清單依 ordinal 升冪排序後一一對應。
        /// </summary>
        /// <param name="root">分頁內容根元素。</param>
        /// <param name="binder">作為資料來源的按鍵綁定器。</param>
        /// <returns>顯示內容與排序後註冊清單完全對應時回傳 <see langword="true"/>。</returns>
        private static bool VerifyListMatchesRegistrations(VisualElement root, DevKeyBinder binder)
        {
            IReadOnlyList<DevKeyRegistration> registrations = binder.GetRegistrations();
            List<string> labels = CollectLabelTexts(root);

            // 空清單為合法邊界（需求 8.4）：分頁顯示單一空狀態提示、無任何項目列。
            if (registrations.Count == 0)
            {
                return labels.Count == 1 && labels[0] == EmptyMessage;
            }

            List<string> expected = registrations
                .OrderBy(r => r.BindingPath, StringComparer.Ordinal)
                .Select(r => r.BindingPath + EntrySeparator + r.Description)
                .ToList();

            return labels.SequenceEqual(expected);
        }

        /// <summary>
        /// 依序將一批註冊請求寫入 <paramref name="binder"/>。
        /// </summary>
        /// <param name="binder">目標按鍵綁定器。</param>
        /// <param name="batch">註冊請求批次。</param>
        private static void RegisterAll(DevKeyBinder binder, List<RegistrationRequest> batch)
        {
            foreach (RegistrationRequest req in batch)
            {
                binder.RegisterDevKey(req.Action, req.Path, req.Description);
            }
        }

        /// <summary>
        /// 建立一個以「無衝突」替身閘控的真實 <see cref="DevKeyBinder"/>，作為分頁的資料來源。
        /// </summary>
        /// <returns>可成功接受有效唯一註冊的 <see cref="DevKeyBinder"/>。</returns>
        private static DevKeyBinder CreateBinder()
        {
            var checker = Substitute.For<ConflictChecker>();
            checker.CheckConflict(Arg.Any<Enum>(), Arg.Any<string>(), Arg.Any<InputDeviceType>())
                .Returns(Array.Empty<BindingConflict>());

            var binding = Substitute.For<BindingManager>();
            return new DevKeyBinder(binding, checker);
        }

        /// <summary>
        /// 生成一批具「唯一 <see cref="DevAction"/>、有效路徑、任意敘述」的註冊請求。
        /// 以 action 的隨機排列子集合確保 action 唯一，避免重複 action 被 <see cref="DevKeyBinder"/> 拒絕。
        /// </summary>
        /// <returns>註冊請求批次的產生器。</returns>
        private static Gen<List<RegistrationRequest>> GenRegistrationBatch()
        {
            return
                from take in Gen.Choose(0, AllActions.Length)
                from shuffled in GenShuffle(AllActions)
                from pathIndices in Gen.ArrayOf(Gen.Choose(0, ValidKeyboardPaths.Length - 1), take)
                from descriptions in Gen.ArrayOf(
                    Gen.Elements("desc-a", "desc-b", "desc-c", string.Empty), take)
                select Enumerable.Range(0, take)
                    .Select(i => new RegistrationRequest(
                        shuffled[i],
                        ValidKeyboardPaths[pathIndices[i]],
                        descriptions[i]))
                    .ToList();
        }

        /// <summary>
        /// 產生一個陣列的隨機排列。
        /// </summary>
        /// <typeparam name="T">元素型別。</typeparam>
        /// <param name="source">來源陣列。</param>
        /// <returns>隨機排列後的陣列產生器。</returns>
        private static Gen<T[]> GenShuffle<T>(T[] source)
        {
            return Gen.ArrayOf(Gen.Choose(0, int.MaxValue), source.Length)
                .Select(keys => source
                    .Select((item, i) => new { item, key = keys[i] })
                    .OrderBy(x => x.key)
                    .Select(x => x.item)
                    .ToArray());
        }

        /// <summary>
        /// 單筆註冊請求（測試用資料）。
        /// </summary>
        private sealed class RegistrationRequest
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrationRequest"/> class.
            /// </summary>
            /// <param name="action">開發者動作。</param>
            /// <param name="path">綁定路徑。</param>
            /// <param name="description">功能敘述。</param>
            public RegistrationRequest(DevAction action, string path, string description)
            {
                this.Action = action;
                this.Path = path;
                this.Description = description;
            }

            /// <summary>Gets 開發者動作。</summary>
            public DevAction Action { get; }

            /// <summary>Gets 綁定路徑。</summary>
            public string Path { get; }

            /// <summary>Gets 功能敘述。</summary>
            public string Description { get; }
        }
    }
}
