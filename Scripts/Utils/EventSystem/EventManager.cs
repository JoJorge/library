namespace Utils.EventSystem
{
    using System;
    using System.Collections.Generic;
    using Utils.Singleton;

    /// <summary>
    /// 集中式事件管理器，採用 Pub-Sub 模式。
    /// 繼承 <see cref="Singleton{T}"/> 基底類別，提供全域唯一存取點。
    /// 透過 <see cref="Init"/>／<see cref="Release"/> 管理系統就緒狀態，
    /// 並以快照派發搭配延遲操作佇列確保派發過程中訂閱／退訂的一致性。
    /// </summary>
    public class EventManager : Singleton<EventManager>
    {
        /// <summary>
        /// 事件註冊表，key 為事件類型，value 為該事件的監聽者列表（維持註冊順序）。
        /// </summary>
        private Dictionary<EventName, List<Action<EventName, EventArgs>>> _listeners;

        /// <summary>
        /// 當前巢狀派發深度，0 表示未在派發中。
        /// </summary>
        private int _dispatchDepth;

        /// <summary>
        /// 延遲操作佇列，僅在 <see cref="_dispatchDepth"/> 大於 0（派發中）時使用。
        /// </summary>
        private List<PendingOperation> _pendingOps;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventManager"/> class.
        /// protected 建構子搭配 <see cref="Singleton{T}"/> 的反射建立機制，禁止外部直接 new。
        /// </summary>
        protected EventManager()
        {
        }

        /// <summary>
        /// Gets a value indicating whether 系統是否已就緒（已 Init 且尚未 Release）。
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// 初始化事件系統，建立內部註冊表與延遲操作佇列並將系統標記為就緒。
        /// 若系統已就緒（重複呼叫）則忽略並記錄警告，不重置既有訂閱。
        /// </summary>
        public void Init()
        {
            if (this.IsReady)
            {
                Logger.LogWarning("[EventManager] Init called while already ready. Ignored.");
                return;
            }

            this._listeners = new Dictionary<EventName, List<Action<EventName, EventArgs>>>();
            this._pendingOps = new List<PendingOperation>();
            this._dispatchDepth = 0;
            this.IsReady = true;
        }

        /// <summary>
        /// 釋放事件系統，清除所有已註冊的監聽者與延遲操作佇列，並將系統標記為未就緒。
        /// </summary>
        public void Release()
        {
            this._listeners?.Clear();
            this._listeners = null;
            this._pendingOps?.Clear();
            this._pendingOps = null;
            this._dispatchDepth = 0;
            this.IsReady = false;
        }

        /// <summary>
        /// 訂閱指定事件。
        /// </summary>
        /// <param name="type">事件類型。</param>
        /// <param name="listener">監聽者委派。</param>
        public void Subscribe(EventName type, Action<EventName, EventArgs> listener)
        {
            if (this.GuardOperation("Subscribe", type) == false)
            {
                return;
            }

            if (listener == null)
            {
                Logger.LogWarning("[EventManager] Subscribe called with null listener. Ignored.");
                return;
            }

            if (this._dispatchDepth > 0)
            {
                this._pendingOps.Add(new PendingOperation
                {
                    OpType = PendingOpType.Add,
                    EventType = type,
                    Listener = listener,
                });
                return;
            }

            this.AddListener(type, listener);
        }

        /// <summary>
        /// 取消訂閱指定事件。
        /// </summary>
        /// <param name="type">事件類型。</param>
        /// <param name="listener">監聽者委派。</param>
        public void Unsubscribe(EventName type, Action<EventName, EventArgs> listener)
        {
            if (this.GuardOperation("Unsubscribe", type) == false)
            {
                return;
            }

            if (listener == null)
            {
                Logger.LogWarning("[EventManager] Unsubscribe called with null listener. Ignored.");
                return;
            }

            if (this._dispatchDepth > 0)
            {
                this._pendingOps.Add(new PendingOperation
                {
                    OpType = PendingOpType.Remove,
                    EventType = type,
                    Listener = listener,
                });
                return;
            }

            this.RemoveListener(type, listener);
        }

        /// <summary>
        /// 移除指定事件的所有監聽者。
        /// </summary>
        /// <param name="type">事件類型。</param>
        public void UnsubscribeAll(EventName type)
        {
            if (this.GuardOperation("UnsubscribeAll", type) == false)
            {
                return;
            }

            if (this._dispatchDepth > 0)
            {
                this._pendingOps.Add(new PendingOperation
                {
                    OpType = PendingOpType.RemoveAll,
                    EventType = type,
                    Listener = null,
                });
                return;
            }

            this._listeners.Remove(type);
        }

        /// <summary>
        /// 發送事件（無參數）。轉呼叫帶參數多載並傳入 <c>default(EventArgs)</c>。
        /// </summary>
        /// <param name="type">事件類型。</param>
        public void Dispatch(EventName type)
        {
            this.Dispatch(type, default(EventArgs));
        }

        /// <summary>
        /// 發送事件（帶參數）。
        /// 通過防護檢查後建立監聽者快照，依註冊順序逐一呼叫；
        /// 單一監聽者拋出的例外會被捕獲並以 <see cref="Debug.LogError"/> 記錄後繼續派發其餘監聽者，
        /// 確保故障隔離且不向呼叫端拋出例外。派發過程支援巢狀呼叫，
        /// 僅在最外層派發結束（深度歸零）時透過 <see cref="ApplyPendingChanges"/> 套用延遲的訂閱／退訂變更。
        /// </summary>
        /// <param name="type">事件類型。</param>
        /// <param name="args">事件參數。</param>
        public void Dispatch(EventName type, EventArgs args)
        {
            if (this.GuardOperation("Dispatch", type) == false)
            {
                return;
            }

            if (this._listeners.TryGetValue(type, out List<Action<EventName, EventArgs>> list) == false
                || list.Count == 0)
            {
                return;
            }

            Action<EventName, EventArgs>[] snapshot = list.ToArray();

            this._dispatchDepth++;
            try
            {
                for (int i = 0; i < snapshot.Length; i++)
                {
                    Action<EventName, EventArgs> listener = snapshot[i];
                    try
                    {
                        listener(type, args);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(
                            $"[EventManager] Listener '{listener.Method.Name}' threw while handling event " +
                            $"'{type}': {ex}");
                    }
                }
            }
            finally
            {
                this._dispatchDepth--;
                if (this._dispatchDepth == 0)
                {
                    this.ApplyPendingChanges();
                }
            }
        }

        /// <summary>
        /// 驗證傳入的 <see cref="EventName"/> 是否為列舉中已定義的合法值。
        /// 透過強制轉型產生的非法整數值將回傳 <c>false</c>。
        /// </summary>
        /// <param name="type">待驗證的事件類型。</param>
        /// <returns>合法值回傳 <c>true</c>，未定義值回傳 <c>false</c>。</returns>
        private bool IsValidEventType(EventName type)
        {
            return Enum.IsDefined(typeof(EventName), type);
        }

        /// <summary>
        /// 公開 API 進入點的共用防護檢查。
        /// 當系統未就緒（<see cref="IsReady"/> 為 <c>false</c>）或傳入非法 <see cref="EventName"/> 時，
        /// 以 <see cref="Debug.LogWarning"/> 記錄操作名稱與傳入值並回傳 <c>false</c>，呼叫端應據此忽略該操作。
        /// </summary>
        /// <param name="operationName">呼叫此防護的公開 API 名稱，用於警告訊息。</param>
        /// <param name="type">待驗證的事件類型。</param>
        /// <returns>通過防護回傳 <c>true</c>；未就緒或非法事件類型回傳 <c>false</c>。</returns>
        private bool GuardOperation(string operationName, EventName type)
        {
            if (this.IsReady == false)
            {
                Logger.LogWarning(
                    $"[EventManager] {operationName} called while not ready (IsReady=false). Ignored.");
                return false;
            }

            if (this.IsValidEventType(type) == false)
            {
                Logger.LogWarning(
                    $"[EventManager] {operationName} called with invalid EventType value {(int)type}. Ignored.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 套用延遲操作佇列中的所有變更並清空佇列。
        /// </summary>
        private void ApplyPendingChanges()
        {
            for (int i = 0; i < this._pendingOps.Count; i++)
            {
                PendingOperation op = this._pendingOps[i];
                switch (op.OpType)
                {
                    case PendingOpType.Add:
                        this.AddListener(op.EventType, op.Listener);
                        break;

                    case PendingOpType.Remove:
                        this.RemoveListener(op.EventType, op.Listener);
                        break;

                    case PendingOpType.RemoveAll:
                        this._listeners.Remove(op.EventType);
                        break;
                }
            }

            this._pendingOps.Clear();
        }

        /// <summary>
        /// 立即將監聽者註冊至指定事件，維持註冊順序並以 <c>Contains</c> 忽略重複註冊。
        /// 供 <see cref="Subscribe"/> 的立即路徑與 <see cref="ApplyPendingChanges"/> 的 Add 分支共用。
        /// </summary>
        /// <param name="type">事件類型。</param>
        /// <param name="listener">監聽者委派。</param>
        private void AddListener(EventName type, Action<EventName, EventArgs> listener)
        {
            if (this._listeners.TryGetValue(type, out List<Action<EventName, EventArgs>> list) == false)
            {
                list = new List<Action<EventName, EventArgs>>();
                this._listeners[type] = list;
            }

            if (list.Contains(listener))
            {
                return;
            }

            list.Add(listener);
        }

        /// <summary>
        /// 立即從指定事件移除監聽者；事件未註冊或監聽者不存在時靜默忽略。
        /// 供 <see cref="Unsubscribe"/> 的立即路徑與 <see cref="ApplyPendingChanges"/> 的 Remove 分支共用。
        /// </summary>
        /// <param name="type">事件類型。</param>
        /// <param name="listener">監聽者委派。</param>
        private void RemoveListener(EventName type, Action<EventName, EventArgs> listener)
        {
            if (this._listeners.TryGetValue(type, out List<Action<EventName, EventArgs>> list))
            {
                list.Remove(listener);
            }
        }

        /// <summary>
        /// 延遲操作類型。
        /// </summary>
        private enum PendingOpType
        {
            /// <summary>
            /// 新增監聽者。
            /// </summary>
            Add,

            /// <summary>
            /// 移除單一監聽者。
            /// </summary>
            Remove,

            /// <summary>
            /// 移除該事件的所有監聽者。
            /// </summary>
            RemoveAll,
        }

        /// <summary>
        /// 延遲操作資料結構，記錄派發中發生的訂閱／退訂操作以待派發結束後套用。
        /// </summary>
        private struct PendingOperation
        {
            /// <summary>
            /// 操作類型（Add／Remove／RemoveAll）。
            /// </summary>
            public PendingOpType OpType;

            /// <summary>
            /// 目標事件類型。
            /// </summary>
            public EventName EventType;

            /// <summary>
            /// 操作目標監聽者（RemoveAll 時為 <c>null</c>）。
            /// </summary>
            public Action<EventName, EventArgs> Listener;
        }
    }
}
