namespace Utils.EventSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;
    using Utils.Singleton.Tests;
    using EventArgs = Utils.EventSystem.EventArgs;
    using EventName = Utils.EventSystem.EventName;

    /// <summary>
    /// 屬性導向測試（Property-Based Tests），使用 FsCheck 3.X 驗證 <see cref="EventManager"/>
    /// 在派發過程中的延遲變更行為與各項防護機制的正確性屬性。
    /// 涵蓋派發中延遲訂閱／退訂、故障隔離、非法列舉拒絕與未就緒防護。
    /// </summary>
    /// <remarks>
    /// 本 fixture 與 <c>EventManagerPropertyTests</c> 分屬不同檔案與類別，以避免平行任務的檔案衝突。
    /// 由於部分屬性會刻意觸發 <see cref="Debug.LogWarning"/> 與 <see cref="Debug.LogError"/>，
    /// 在 <c>[SetUp]</c> 中啟用 <see cref="LogAssert.ignoreFailingMessages"/> 以避免預期的錯誤日誌使測試失敗
    /// （每次迭代的訊息數量不固定，故不採用逐則 <c>LogAssert.Expect</c>）。
    /// </remarks>
    [TestFixture]
    public class EventManagerDispatchPropertyTests
    {
        /// <summary>
        /// 每個測試前重置 <see cref="EventManager"/> 單例並初始化。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            SingletonTestHelper.ResetInstance<EventManager>();
            EventManager.Instance.Init();
        }

        /// <summary>
        /// 每個測試後釋放 <see cref="EventManager"/> 並重置單例，確保測試間狀態隔離。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            EventManager.Instance.Release();
            SingletonTestHelper.ResetInstance<EventManager>();
        }

        /// <summary>
        /// Property 9: Deferred Subscribe During Dispatch。
        /// 某監聽者在派發回呼中對同一 <see cref="EventName"/> 訂閱一個新監聽者時，
        /// 新監聽者在當次派發中不得被呼叫（呼叫次數為 0），但在下一次 Dispatch 時必定被呼叫。
        /// </summary>
        /// <remarks>Validates: Requirements 3.6.</remarks>
        // Feature: event-system, Property 9: Deferred Subscribe During Dispatch
        [Test]
        public void DeferredSubscribeDuringDispatch()
        {
            Prop.ForAll(
                GenEventType(),
                type =>
                {
                    SingletonTestHelper.ResetInstance<EventManager>();
                    EventManager em = EventManager.Instance;
                    em.Init();

                    int newListenerCalls = 0;
                    Action<EventName, EventArgs> newListener = (t, a) => newListenerCalls++;

                    // 首個監聽者在派發過程中訂閱新監聽者（僅訂閱一次，避免重複）。
                    bool subscribed = false;
                    Action<EventName, EventArgs> subscriber = (t, a) =>
                    {
                        if (subscribed == false)
                        {
                            em.Subscribe(type, newListener);
                            subscribed = true;
                        }
                    };

                    em.Subscribe(type, subscriber);

                    // 當次派發：新監聽者為延遲訂閱，不應被呼叫。
                    em.Dispatch(type);
                    bool notCalledThisDispatch = newListenerCalls == 0;

                    // 下一次派發：新監聽者應已生效並被呼叫一次。
                    em.Dispatch(type);
                    bool calledNextDispatch = newListenerCalls == 1;

                    em.Release();

                    return notCalledThisDispatch && calledNextDispatch;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 10: Deferred Unsubscribe During Dispatch。
        /// 某監聽者在派發回呼中對同一 <see cref="EventName"/> 退訂另一個已註冊的監聽者時，
        /// 因為使用快照，被移除的監聽者在當次派發中仍會被呼叫，但在下一次 Dispatch 時不再被呼叫。
        /// </summary>
        /// <remarks>Validates: Requirements 4.6.</remarks>
        // Feature: event-system, Property 10: Deferred Unsubscribe During Dispatch
        [Test]
        public void DeferredUnsubscribeDuringDispatch()
        {
            Prop.ForAll(
                GenEventType(),
                type =>
                {
                    SingletonTestHelper.ResetInstance<EventManager>();
                    EventManager em = EventManager.Instance;
                    em.Init();

                    int targetCalls = 0;
                    Action<EventName, EventArgs> target = (t, a) => targetCalls++;

                    // 首個監聽者在派發過程中退訂 target（僅退訂一次）。
                    bool unsubscribed = false;
                    Action<EventName, EventArgs> remover = (t, a) =>
                    {
                        if (unsubscribed == false)
                        {
                            em.Unsubscribe(type, target);
                            unsubscribed = true;
                        }
                    };

                    // remover 先註冊，target 後註冊，確保退訂發生在 target 被快照呼叫之前。
                    em.Subscribe(type, remover);
                    em.Subscribe(type, target);

                    // 當次派發：target 因快照仍被呼叫一次。
                    em.Dispatch(type);
                    bool calledThisDispatch = targetCalls == 1;

                    // 下一次派發：target 已於派發結束後被實際移除，不再被呼叫。
                    em.Dispatch(type);
                    bool notCalledNextDispatch = targetCalls == 1;

                    em.Release();

                    return calledThisDispatch && notCalledNextDispatch;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 11: Exception Isolation。
        /// 註冊 N 個監聽者，其中位於 <c>GenExceptionIndex</c> 的監聽者拋出例外時，
        /// 其餘所有監聽者仍必須被正常呼叫，且 Dispatch 本身不向呼叫端拋出例外。
        /// </summary>
        /// <remarks>Validates: Requirements 5.4, 7.4, 7.5.</remarks>
        // Feature: event-system, Property 11: Exception Isolation
        [Test]
        public void ExceptionIsolation()
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
#endif

            Prop.ForAll(
                GenExceptionCase(),
                sample =>
                {
                    int count = sample.Count;
                    int index = sample.ThrowIndex;

                    SingletonTestHelper.ResetInstance<EventManager>();
                    EventManager em = EventManager.Instance;
                    em.Init();

                    EventName type = EventName.Test1;
                    var callFlags = new bool[count];

                    for (int i = 0; i < count; i++)
                    {
                        int captured = i;
                        Action<EventName, EventArgs> listener = (t, a) =>
                        {
                            callFlags[captured] = true;
                            if (captured == index)
                            {
                                throw new InvalidOperationException(
                                    $"Intentional test exception at index {captured}.");
                            }
                        };

                        em.Subscribe(type, listener);
                    }

                    bool threwToCaller = false;
                    try
                    {
                        em.Dispatch(type);
                    }
                    catch (Exception)
                    {
                        threwToCaller = true;
                    }

                    // 所有監聽者（含拋例外者在拋出前）皆被呼叫，且 Dispatch 未向呼叫端拋出例外。
                    bool allCalled = callFlags.All(flag => flag);

                    em.Release();

                    return allCalled && threwToCaller == false;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 12: Invalid Enum Rejection。
        /// 對於未定義於 <see cref="EventName"/> 的整數值（強制轉型），
        /// Subscribe／Unsubscribe／Dispatch 皆被忽略、不改變內部狀態、不拋出例外。
        /// </summary>
        /// <remarks>Validates: Requirements 1.4, 7.3.</remarks>
        // Feature: event-system, Property 12: Invalid Enum Rejection
        [Test]
        public void InvalidEnumRejection()
        {
            Prop.ForAll(
                GenInvalidEventType(),
                invalidType =>
                {
                    SingletonTestHelper.ResetInstance<EventManager>();
                    EventManager em = EventManager.Instance;
                    em.Init();

                    int calls = 0;
                    Action<EventName, EventArgs> listener = (t, a) => calls++;

                    bool threw = false;
                    try
                    {
                        // 非法事件類型：所有操作皆應被忽略。
                        em.Subscribe(invalidType, listener);
                        em.Dispatch(invalidType);
                        em.Unsubscribe(invalidType, listener);
                        em.UnsubscribeAll(invalidType);
                    }
                    catch (Exception)
                    {
                        threw = true;
                    }

                    // 監聽者從未被呼叫（訂閱被忽略），且無例外拋出。
                    bool noStateChange = calls == 0;

                    em.Release();

                    return noStateChange && threw == false;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 13: Not-Ready Guard。
        /// 在 <see cref="EventManager"/> 尚未 Init 或已 Release（<c>IsReady == false</c>）的狀態下，
        /// Subscribe／Unsubscribe／UnsubscribeAll／Dispatch 皆無操作效果、不拋出例外；
        /// 且 Init 後建立的訂閱在 Release 後全部失效（先前的訂閱關係變為 inert）。
        /// </summary>
        /// <remarks>Validates: Requirements 6.3, 6.4.</remarks>
        // Feature: event-system, Property 13: Not-Ready Guard
        [Test]
        public void NotReadyGuard()
        {
            Prop.ForAll(
                GenEventType(),
                Gen.Elements(false, true).ToArbitrary(),
                (type, releaseFirst) =>
                {
                    SingletonTestHelper.ResetInstance<EventManager>();
                    EventManager em = EventManager.Instance;

                    int calls = 0;
                    Action<EventName, EventArgs> listener = (t, a) => calls++;

                    if (releaseFirst)
                    {
                        // 情境 A：先 Init 並訂閱，再 Release，驗證先前訂閱失效。
                        em.Init();
                        em.Subscribe(type, listener);
                        em.Release();
                    }

                    // 此時 IsReady == false（未 Init，或已 Release）。
                    bool notReady = em.IsReady == false;

                    bool threw = false;
                    try
                    {
                        em.Subscribe(type, listener);
                        em.Dispatch(type);
                        em.Unsubscribe(type, listener);
                        em.UnsubscribeAll(type);
                    }
                    catch (Exception)
                    {
                        threw = true;
                    }

                    // 未就緒時所有操作皆無效果，監聽者從未被呼叫。
                    bool noEffect = calls == 0;

                    SingletonTestHelper.ResetInstance<EventManager>();

                    return notReady && noEffect && threw == false;
                }).QuickCheckThrowOnFailure();
        }

        // ─── 產生器（Generators） ───

        /// <summary>
        /// 隨機合法的 <see cref="EventName"/> enum 值（排除 None，聚焦於實際事件成員）。
        /// </summary>
        /// <returns>合法事件類型的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<EventName> GenEventType()
        {
            return Gen.Elements(
                EventName.Test1,
                EventName.Test2,
                EventName.Test3,
                EventName.Test4).ToArbitrary();
        }

        /// <summary>
        /// 隨機未定義於 <see cref="EventName"/> 中的整數值（強制轉型為非法列舉值）。
        /// 產生範圍廣泛的整數並過濾掉所有已定義的值。
        /// </summary>
        /// <returns>非法事件類型的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<EventName> GenInvalidEventType()
        {
            HashSet<int> defined = Enum.GetValues(typeof(EventName))
                .Cast<int>()
                .ToHashSet();

            Gen<EventName> gen = Gen.Choose(-10000, 10000)
                .Where(value => defined.Contains(value) == false)
                .Select(value => (EventName)value);

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 產生故障隔離案例：先由 <c>GenListenerCount</c> 決定監聽者數量（1～20），
        /// 再由 <c>GenExceptionIndex</c> 於有效範圍內選取拋出例外的監聽者索引。
        /// </summary>
        /// <returns>故障隔離案例的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<ExceptionCase> GenExceptionCase()
        {
            Gen<ExceptionCase> gen =
                from count in GenListenerCount()
                from throwIndex in GenExceptionIndex(count)
                select new ExceptionCase
                {
                    Count = count,
                    ThrowIndex = throwIndex,
                };

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 隨機正整數（1～20），代表訂閱者數量。
        /// </summary>
        /// <returns>監聽者數量的 <see cref="Gen{T}"/>。</returns>
        private static Gen<int> GenListenerCount()
        {
            return Gen.Choose(1, 20);
        }

        /// <summary>
        /// 隨機選取某個 listener 位置拋出例外的索引（0 至 <paramref name="count"/> - 1）。
        /// </summary>
        /// <param name="count">監聽者數量，用於界定索引上界。</param>
        /// <returns>例外索引的 <see cref="Gen{T}"/>。</returns>
        private static Gen<int> GenExceptionIndex(int count)
        {
            return Gen.Choose(0, Math.Max(0, count - 1));
        }

        /// <summary>
        /// 故障隔離案例樣本：記錄監聽者總數與拋出例外的監聽者索引。
        /// </summary>
        private struct ExceptionCase
        {
            /// <summary>Gets or sets 監聽者總數。</summary>
            public int Count { get; set; }

            /// <summary>Gets or sets 拋出例外的監聽者索引。</summary>
            public int ThrowIndex { get; set; }
        }
    }
}
