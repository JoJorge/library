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
    /// 屬性導向測試（Property-Based Tests），使用 FsCheck 3.X 驗證 <see cref="EventManager"/> 的
    /// 訂閱與派發正確性屬性。涵蓋訂閱通知、退訂移除、重複訂閱冪等、註冊順序派發與全部退訂清除。
    /// </summary>
    [TestFixture]
    public class EventManagerPropertyTests
    {
        /// <summary>
        /// 每個測試前重置 <see cref="EventManager"/> 單例並重新初始化，確保測試間狀態隔離。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            SingletonTestHelper.ResetInstance<EventManager>();
            EventManager.Instance.Init();
        }

        /// <summary>
        /// 每個測試後釋放事件系統並再次重置單例，避免狀態外洩至後續測試。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            EventManager.Instance.Release();
            SingletonTestHelper.ResetInstance<EventManager>();
        }

        /// <summary>
        /// Property 4: Subscribe Implies Notification。
        /// 對於任意合法 EventType 與監聽者，訂閱後派發該事件，該監聽者必定恰好被呼叫一次。
        /// </summary>
        // Feature: event-system, Property 4: Subscribe Implies Notification
        [Test]
        public void Subscribe_ImpliesNotification()
        {
            Prop.ForAll(
                GenEventType(),
                type =>
                {
                    int callCount = 0;
                    Action<EventName, EventArgs> listener = (t, a) => callCount++;

                    EventManager.Instance.Subscribe(type, listener);
                    EventManager.Instance.Dispatch(type);

                    // 清理，避免污染後續 iteration（單例在整個測試內共用）。
                    EventManager.Instance.Unsubscribe(type, listener);

                    return callCount == 1;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 5: Unsubscribe Removes Notification。
        /// 對於任意已訂閱的監聽者，退訂後再派發，該監聽者的呼叫次數必為零。
        /// </summary>
        // Feature: event-system, Property 5: Unsubscribe Removes Notification
        [Test]
        public void Unsubscribe_RemovesNotification()
        {
            Prop.ForAll(
                GenEventType(),
                type =>
                {
                    int callCount = 0;
                    Action<EventName, EventArgs> listener = (t, a) => callCount++;

                    EventManager.Instance.Subscribe(type, listener);
                    EventManager.Instance.Unsubscribe(type, listener);
                    EventManager.Instance.Dispatch(type);

                    return callCount == 0;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 6: Idempotent Subscription。
        /// 對於任意合法 EventType 與監聽者，將「同一個」委派實例重複訂閱 n 次（n 介於 2～10），
        /// 派發該事件時該監聽者仍恰好被呼叫一次。
        /// </summary>
        // Feature: event-system, Property 6: Idempotent Subscription
        [Test]
        public void Subscribe_IsIdempotent()
        {
            Prop.ForAll(
                GenEventType(),
                Gen.Choose(2, 10).ToArbitrary(),
                (type, repeat) =>
                {
                    int callCount = 0;
                    Action<EventName, EventArgs> listener = (t, a) => callCount++;

                    // 重複訂閱「同一個」委派實例 repeat 次。
                    for (int i = 0; i < repeat; i++)
                    {
                        EventManager.Instance.Subscribe(type, listener);
                    }

                    EventManager.Instance.Dispatch(type);

                    EventManager.Instance.Unsubscribe(type, listener);

                    return callCount == 1;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 7: Registration-Order Dispatch。
        /// 對於任意合法 EventType 與 N 個不同監聽者（依序註冊），派發時的呼叫順序必與註冊順序一致。
        /// </summary>
        // Feature: event-system, Property 7: Registration-Order Dispatch
        [Test]
        public void Dispatch_FollowsRegistrationOrder()
        {
            Prop.ForAll(
                GenEventType(),
                GenListenerCount(),
                (type, count) =>
                {
                    List<int> invocationOrder = new List<int>();
                    List<Action<EventName, EventArgs>> listeners =
                        new List<Action<EventName, EventArgs>>();

                    // 建立 count 個不同的委派實例，各自捕獲自己的索引後寫入共用順序清單。
                    for (int i = 0; i < count; i++)
                    {
                        int captured = i;
                        Action<EventName, EventArgs> listener =
                            (t, a) => invocationOrder.Add(captured);
                        listeners.Add(listener);
                        EventManager.Instance.Subscribe(type, listener);
                    }

                    EventManager.Instance.Dispatch(type);

                    foreach (Action<EventName, EventArgs> listener in listeners)
                    {
                        EventManager.Instance.Unsubscribe(type, listener);
                    }

                    // 呼叫順序必須為 0, 1, ..., count-1。
                    return invocationOrder.SequenceEqual(Enumerable.Range(0, count));
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 8: UnsubscribeAll Clears Event。
        /// 對於任意合法 EventType 與 N 個已註冊的監聽者，呼叫 UnsubscribeAll 後派發該事件，
        /// 所有監聽者的呼叫次數皆為零。
        /// </summary>
        // Feature: event-system, Property 8: UnsubscribeAll Clears Event
        [Test]
        public void UnsubscribeAll_ClearsEvent()
        {
            Prop.ForAll(
                GenEventType(),
                GenListenerCount(),
                (type, count) =>
                {
                    int callCount = 0;

                    for (int i = 0; i < count; i++)
                    {
                        Action<EventName, EventArgs> listener = (t, a) => callCount++;
                        EventManager.Instance.Subscribe(type, listener);
                    }

                    EventManager.Instance.UnsubscribeAll(type);
                    EventManager.Instance.Dispatch(type);

                    return callCount == 0;
                }).QuickCheckThrowOnFailure();
        }

        // ─── 產生器（Generators） ───

        /// <summary>
        /// 建立合法 <see cref="EventName"/> 產生器，隨機取自 None 以外的已定義成員。
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
        /// 建立非法 <see cref="EventName"/> 產生器，隨機取整數範圍後濾除所有已定義的列舉值，強制轉型為 EventType。
        /// </summary>
        /// <returns>未定義事件類型值的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<EventName> GenInvalidEventType()
        {
            return Gen.Choose(-100, 100)
                .Where(value => Enum.IsDefined(typeof(EventName), value) == false)
                .Select(value => (EventName)value)
                .ToArbitrary();
        }

        /// <summary>
        /// 建立監聽者數量產生器，隨機正整數（1～20）。
        /// </summary>
        /// <returns>監聽者數量的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<int> GenListenerCount()
        {
            return Gen.Choose(1, 20).ToArbitrary();
        }

        /// <summary>
        /// 建立例外索引產生器，於 0～(count-1) 之間隨機選取一個監聽者位置作為拋出例外者。
        /// </summary>
        /// <param name="count">監聽者總數。</param>
        /// <returns>例外索引的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<int> GenExceptionIndex(int count)
        {
            return Gen.Choose(0, Math.Max(0, count - 1)).ToArbitrary();
        }
    }
}
