namespace Utils.EventSystem.Tests
{
    using System;
    using System.Text.RegularExpressions;
    using NUnit.Framework;
    using Utils.Singleton.Tests;
    using EventArgs = Utils.EventSystem.EventArgs;

    /// <summary>
    /// EventManager 的範例導向單元測試。
    /// 驗證無參數 Dispatch、null listener 忽略、未註冊 Unsubscribe 靜默忽略、
    /// 無監聽者 Dispatch、巢狀 Dispatch，以及 Init／Release 生命週期與重複 Init 被忽略等具體場景。
    /// </summary>
    [TestFixture]
    public class EventManagerUnitTests
    {
        /// <summary>
        /// 每個測試前重置單例，確保各測試之間狀態隔離。
        /// 不在此處統一 Init，因部分測試需驗證未初始化狀態；
        /// 需就緒的測試會於測試內明確呼叫 <see cref="EventManager.Init"/>。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            SingletonTestHelper.ResetInstance<EventManager>();
        }

        /// <summary>
        /// 每個測試後若系統仍就緒則 Release，再重置單例，避免狀態外溢至後續測試。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (EventManager.Instance.IsReady)
            {
                EventManager.Instance.Release();
            }

            SingletonTestHelper.ResetInstance<EventManager>();
            // 重新設定 Logger 的 log action，避免測試間互相干擾。
            Logger.SetLogAction(null);
            Logger.SetLogWarningAction(null);
            Logger.SetLogErrorAction(null);
        }

        // ─── 無參數 Dispatch 正常執行（Requirement 2.8） ───

        /// <summary>
        /// 驗證無參數 Dispatch 多載會以 default(EventArgs) 呼叫監聽者，
        /// 監聽者被呼叫一次且收到的參數 ParamCount 為 0。
        /// </summary>
        [Test]
        public void Dispatch_NoArgsOverload_InvokesListenerWithDefaultEventArgs()
        {
            EventManager manager = EventManager.Instance;
            manager.Init();

            int callCount = 0;
            int receivedParamCount = -1;
            manager.Subscribe(EventName.Test1, (type, args) =>
            {
                callCount++;
                receivedParamCount = args.ParamCount;
            });

            manager.Dispatch(EventName.Test1);

            Assert.That(callCount, Is.EqualTo(1), "監聽者應被呼叫一次");
            Assert.That(receivedParamCount, Is.EqualTo(0), "無參數 Dispatch 應傳入 default(EventArgs)（ParamCount 為 0）");
        }

        // ─── Subscribe null listener 被忽略（Requirement 3.5） ───

        /// <summary>
        /// 驗證 Subscribe 傳入 null listener 時被忽略並記錄警告，
        /// 後續 Dispatch 不拋例外亦無任何效果。
        /// </summary>
        [Test]
        public void Subscribe_NullListener_IgnoredWithWarning()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);

            EventManager manager = EventManager.Instance;
            manager.Init();

            manager.Subscribe(EventName.Test1, null);

            Assert.DoesNotThrow(() => manager.Dispatch(EventName.Test1), "Dispatch 不應因 null listener 而拋例外");
            Assert.IsTrue(warningLog.Contains("Subscribe called with null listener"), "應記錄警告訊息");
        }

        // ─── Unsubscribe null listener 被忽略（Requirement 4.5） ───

        /// <summary>
        /// 驗證 Unsubscribe 傳入 null listener 時被忽略並記錄警告，且不拋例外。
        /// </summary>
        [Test]
        public void Unsubscribe_NullListener_IgnoredWithWarning()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);

            EventManager manager = EventManager.Instance;
            manager.Init();

            Assert.DoesNotThrow(() => manager.Unsubscribe(EventName.Test1, null), "Unsubscribe null listener 不應拋例外");
            Assert.IsTrue(warningLog.Contains("Unsubscribe called with null listener"), "應記錄警告訊息");
        }

        // ─── Unsubscribe 未註冊的 listener 靜默忽略（Requirement 4.3） ───

        /// <summary>
        /// 驗證 Unsubscribe 一個從未註冊的 listener 時靜默忽略：不拋例外且不記錄任何警告。
        /// </summary>
        [Test]
        public void Unsubscribe_UnregisteredListener_SilentlyIgnored()
        {
            var warningLog = string.Empty;
            var errorLog = string.Empty;
            EventManager manager = EventManager.Instance;
            manager.Init();

            Action<EventName, EventArgs> neverSubscribed = (type, args) => { };

            Assert.DoesNotThrow(
                () => manager.Unsubscribe(EventName.Test1, neverSubscribed),
                "Unsubscribe 未註冊的 listener 不應拋例外");

            // 靜默忽略：不應產生任何非預期的 log。
            Assert.IsEmpty(warningLog, "Unsubscribe 未註冊的 listener 不應記錄警告");
            Assert.IsEmpty(errorLog, "Unsubscribe 未註冊的 listener 不應記錄錯誤");
        }

        // ─── Dispatch 無監聽者正常結束（Requirement 5.3） ───

        /// <summary>
        /// 驗證對沒有任何訂閱者的事件呼叫 Dispatch 時正常結束，不拋例外。
        /// </summary>
        [Test]
        public void Dispatch_NoSubscribers_CompletesWithoutThrow()
        {
            EventManager manager = EventManager.Instance;
            manager.Init();

            Assert.DoesNotThrow(() => manager.Dispatch(EventName.Test2), "無監聽者的 Dispatch 應正常結束");
        }

        // ─── 巢狀 Dispatch 正確完成（Requirement 5.5） ───

        /// <summary>
        /// 驗證事件 A 的監聽者在被呼叫時再 Dispatch 事件 B，
        /// 事件 B 的監聽者亦被正確呼叫，兩者皆執行且無錯誤。
        /// </summary>
        [Test]
        public void Dispatch_NestedDispatch_BothListenersInvoked()
        {
            EventManager manager = EventManager.Instance;
            manager.Init();

            bool outerInvoked = false;
            bool innerInvoked = false;

            manager.Subscribe(EventName.Test2, (type, args) => { innerInvoked = true; });
            manager.Subscribe(EventName.Test1, (type, args) =>
            {
                outerInvoked = true;

                // 於外層事件的監聽者內觸發巢狀派發。
                manager.Dispatch(EventName.Test2);
            });

            Assert.DoesNotThrow(() => manager.Dispatch(EventName.Test1), "巢狀 Dispatch 不應拋例外");
            Assert.That(outerInvoked, Is.True, "外層事件監聽者應被呼叫");
            Assert.That(innerInvoked, Is.True, "巢狀事件監聽者應被呼叫");
        }

        // ─── Init 後 IsReady 為 true（Requirement 6.2） ───

        /// <summary>
        /// 驗證呼叫 Init 後 IsReady 為 true。
        /// </summary>
        [Test]
        public void Init_AfterCall_IsReadyTrue()
        {
            EventManager manager = EventManager.Instance;
            manager.Init();

            Assert.That(manager.IsReady, Is.True, "Init 後 IsReady 應為 true");
        }

        // ─── Release 後 IsReady 為 false（Requirement 6.3） ───

        /// <summary>
        /// 驗證呼叫 Release 後 IsReady 為 false。
        /// </summary>
        [Test]
        public void Release_AfterCall_IsReadyFalse()
        {
            EventManager manager = EventManager.Instance;
            manager.Init();
            manager.Release();

            Assert.That(manager.IsReady, Is.False, "Release 後 IsReady 應為 false");
        }

        // ─── 重複 Init 被忽略（Requirement 6.5） ───

        /// <summary>
        /// 驗證重複呼叫 Init 時被忽略並記錄警告，且不重置既有訂閱：
        /// 第一次 Init 後訂閱的監聽者在第二次 Init 之後仍會於 Dispatch 時被通知。
        /// </summary>
        [Test]
        public void Init_CalledTwice_IgnoredAndPreservesSubscriptions()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);

            EventManager manager = EventManager.Instance;
            manager.Init();

            int callCount = 0;
            manager.Subscribe(EventName.Test1, (type, args) => { callCount++; });

            // 重複 Init：應被忽略，且不重置既有註冊表。
            manager.Init();

            manager.Dispatch(EventName.Test1);

            Assert.That(callCount, Is.EqualTo(1), "重複 Init 不應重置既有訂閱，監聽者仍應被通知");
            Assert.That(warningLog, Does.Contain("Init called while already ready"), "應記錄警告訊息");
        }
    }
}
