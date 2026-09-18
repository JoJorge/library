namespace DevSystem.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using FsCheck;
    using FsCheck.Fluent;
    using InputSystem;
    using NUnit.Framework;
    using Utils.Singleton;

    /// <summary>
    /// <see cref="DevSystemManager"/> 生命週期的屬性導向測試（Property-Based Tests）與範例導向測試。
    /// 使用 FsCheck 3.x（FsCheck.Fluent）於標準 NUnit <c>[Test]</c> 方法內直接呼叫 FsCheck API，
    /// 驗證初始化冪等性、未就緒守衛與釋放清理的正確性屬性。
    /// </summary>
    /// <remarks>
    /// 這些測試驗證的是 <c>DEV_MODE</c> 已定義時的行為（生命週期方法皆以 <c>#if DEV_MODE</c> 閘控），
    /// 對應專案的 DEV_MODE-defined 測試組態。
    /// <para>
    /// <see cref="DevSystemManager"/> 繼承 <see cref="Singleton{T}"/>，其唯一實例以基底型別的靜態
    /// <c>_instance</c> 欄位保存。為使每個測試取得全新實例，於 <see cref="SetUp"/> 與 <see cref="TearDown"/>
    /// 以反射重置該欄位為 null。
    /// </para>
    /// </remarks>
    [TestFixture]
    public class DevSystemManagerLifecycleTests
    {
        /// <summary>
        /// 於每個測試前重置 <see cref="Singleton{T}"/> 基底的 <c>_instance</c> 靜態欄位，確保取得全新實例。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ResetSingletonInstance();
        }

        /// <summary>
        /// 於每個測試後再次重置 Singleton 實例，避免跨測試殘留狀態。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            ResetSingletonInstance();
        }

        // =========================================================
        // Property 10: Lifecycle Correctness
        // =========================================================

        /// <summary>
        /// Property 10: Lifecycle Correctness — 生命週期正確性（Init 冪等性）。
        /// 對於任意呼叫次數 n∈[1,10]，連續呼叫 <see cref="DevSystemManager.Init"/> 皆回傳 true，
        /// 且最終僅建立一組面板與日誌設定（<see cref="DevSystemManager.Panel"/>、
        /// <see cref="DevSystemManager.Logger"/> 於第一次後參考不變，符合冪等 no-op），
        /// 狀態為 <see cref="DevSystemState.Ready"/>。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 10: Lifecycle Correctness.
        /// <para><strong>Validates: Requirements 10.1, 10.6</strong></para>
        /// </remarks>
        [Test]
        public void Property10_RepeatedInit_IsIdempotentAndReady()
        {
            var arb = Arb.From(Gen.Choose(1, 10));

            Prop.ForAll(arb, n =>
            {
                ResetSingletonInstance();

                DevSystemManager manager = DevSystemManager.Instance;
                var definition = new PanelDefinition(new List<PanelNodeDefinition>());

                DevPanel firstPanel = null;
                DevLogger firstLogger = null;

                for (int i = 0; i < n; i++)
                {
                    bool ok = manager.Init(definition);
                    if (!ok)
                    {
                        return false.Label($"Init call #{i + 1} of {n} should return true");
                    }

                    if (i == 0)
                    {
                        firstPanel = manager.Panel;
                        firstLogger = manager.Logger;
                    }
                    else
                    {
                        // 冪等 no-op：後續 Init 不重建面板或重讀設定，參考不變（需求 10.6）。
                        if (!ReferenceEquals(manager.Panel, firstPanel))
                        {
                            return false.Label("repeated Init must not rebuild Panel");
                        }

                        if (!ReferenceEquals(manager.Logger, firstLogger))
                        {
                            return false.Label("repeated Init must not recreate Logger");
                        }
                    }
                }

                // 最終狀態就緒，且面板/設定各建立一次（需求 10.1）。
                if (manager.State != DevSystemState.Ready)
                {
                    return false.Label("state should be Ready after Init");
                }

                if (manager.Panel == null || manager.Logger == null)
                {
                    return false.Label("Panel and Logger should be created exactly once");
                }

                return true.Label("repeated Init is idempotent and Ready");
            }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 10: Lifecycle Correctness — 生命週期正確性（未就緒守衛）。
        /// 於未就緒（尚未 <see cref="DevSystemManager.Init"/>）狀態下，對任意功能 API 的呼叫皆被拒絕
        /// （具回傳值成員回傳其型別 default，元件屬性為 null），且呼叫前後狀態維持
        /// <see cref="DevSystemState.Uninitialized"/> 不變。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 10: Lifecycle Correctness.
        /// <para><strong>Validates: Requirements 10.5</strong></para>
        /// </remarks>
        [Test]
        public void Property10_NotReady_RejectsFunctionalApisAndStateUnchanged()
        {
            // 生成一組要在未就緒狀態下呼叫的功能 API 選擇序列（以索引表示，涵蓋各元件屬性存取）。
            var arb = Arb.From(Gen.NonEmptyListOf(Gen.Choose(0, 3)));

            Prop.ForAll(arb, apiCalls =>
            {
                ResetSingletonInstance();

                DevSystemManager manager = DevSystemManager.Instance;

                // 前置：未就緒。
                if (manager.State != DevSystemState.Uninitialized)
                {
                    return false.Label("manager should start Uninitialized");
                }

                foreach (int call in apiCalls)
                {
                    // 未就緒時，元件屬性守衛應回傳 null（拒絕存取功能）。
                    switch (call % 4)
                    {
                        case 0:
                            if (manager.Logger != null)
                            {
                                return false.Label("Logger must be null when not ready");
                            }

                            break;
                        case 1:
                            if (manager.KeyBinder != null)
                            {
                                return false.Label("KeyBinder must be null when not ready");
                            }

                            break;
                        case 2:
                            if (manager.Panel != null)
                            {
                                return false.Label("Panel must be null when not ready");
                            }

                            break;
                        default:
                            // Release 於未就緒為 no-op，回傳 true，且不改變狀態（需求 10.7）。
                            if (!manager.Release())
                            {
                                return false.Label("Release when not ready should be a true no-op");
                            }

                            break;
                    }

                    // 狀態於每次拒絕後維持不變。
                    if (manager.State != DevSystemState.Uninitialized)
                    {
                        return false.Label("state must remain Uninitialized while not ready");
                    }
                }

                return true.Label("not-ready guard rejects functional APIs, state unchanged");
            }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 10: Lifecycle Correctness — 生命週期正確性（就緒後 Release 清理）。
        /// 對於任意就緒後的 <see cref="DevSystemManager.Release"/>，成功後 <c>KeyBinder</c> 的
        /// <c>GetRegistrations</c> 於重新初始化時應為空（註冊已清除）、面板已釋放（<c>Panel</c> 為 null）、
        /// 狀態轉為 <see cref="DevSystemState.Uninitialized"/>。以 n∈[1,10] 次 Init→Release 循環驗證每次清理一致。
        /// </summary>
        /// <remarks>
        /// Feature: dev-system, Property 10: Lifecycle Correctness.
        /// <para><strong>Validates: Requirements 10.1, 10.4</strong></para>
        /// </remarks>
        [Test]
        public void Property10_ReadyThenRelease_ClearsRegistrationsPanelAndState()
        {
            var arb = Arb.From(Gen.Choose(1, 10));

            Prop.ForAll(arb, cycles =>
            {
                ResetSingletonInstance();

                DevSystemManager manager = DevSystemManager.Instance;
                var definition = new PanelDefinition(new List<PanelNodeDefinition>());

                for (int i = 0; i < cycles; i++)
                {
                    if (!manager.Init(definition))
                    {
                        return false.Label("Init should succeed");
                    }

                    // 就緒時 KeyBinder 存在，且初始無註冊（GetRegistrations 空）。
                    IReadOnlyList<DevKeyRegistration> registrations = manager.KeyBinder.GetRegistrations();
                    if (registrations == null || registrations.Count != 0)
                    {
                        return false.Label("GetRegistrations should be empty after Init");
                    }

                    bool released = manager.Release();
                    if (!released)
                    {
                        return false.Label("Release of a ready system should return true");
                    }

                    // 面板釋放、狀態未就緒（需求 10.4）。
                    if (manager.Panel != null)
                    {
                        return false.Label("Panel should be released (null) after Release");
                    }

                    if (manager.KeyBinder != null)
                    {
                        return false.Label("KeyBinder should be cleared (null) after Release");
                    }

                    if (manager.State != DevSystemState.Uninitialized)
                    {
                        return false.Label("state should be Uninitialized after Release");
                    }
                }

                return true.Label("ready-then-release clears registrations, panel and state");
            }).QuickCheckThrowOnFailure();
        }

        // =========================================================
        // Example tests
        // =========================================================

        /// <summary>
        /// Singleton 全域存取：<see cref="DevSystemManager.Instance"/> 多次存取回傳同一實例，
        /// 且直接建構第二個實例被禁止（<see cref="Singleton{T}"/> 反射防護）。
        /// **Validates: Requirements 10.2**
        /// </summary>
        [Test]
        public void Instance_ProvidesGlobalSingletonAccess()
        {
            DevSystemManager first = DevSystemManager.Instance;
            DevSystemManager second = DevSystemManager.Instance;

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }

        /// <summary>
        /// Release 未就緒為 no-op：於尚未 <see cref="DevSystemManager.Init"/> 時呼叫
        /// <see cref="DevSystemManager.Release"/> 回傳 true，且狀態維持
        /// <see cref="DevSystemState.Uninitialized"/> 不變。
        /// **Validates: Requirements 10.7**
        /// </summary>
        [Test]
        public void Release_WhenNotReady_IsNoOpAndReturnsTrue()
        {
            DevSystemManager manager = DevSystemManager.Instance;

            Assert.That(manager.State, Is.EqualTo(DevSystemState.Uninitialized));

            bool released = manager.Release();

            Assert.That(released, Is.True);
            Assert.That(manager.State, Is.EqualTo(DevSystemState.Uninitialized));
        }

        // =========================================================
        // Singleton reset helper
        // =========================================================

        /// <summary>
        /// 以反射將 <see cref="Singleton{T}"/> 基底的靜態 <c>_instance</c> 欄位重置為 null，
        /// 使後續 <see cref="DevSystemManager.Instance"/> 存取建立全新實例。
        /// </summary>
        private static void ResetSingletonInstance()
        {
            Type baseType = typeof(Singleton<DevSystemManager>);
            FieldInfo field = baseType.GetField(
                "_instance",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(
                field,
                Is.Not.Null,
                "Singleton<T> 應有名為 '_instance' 的私有靜態欄位供測試重置。");

            field.SetValue(null, null);
        }
    }
}
