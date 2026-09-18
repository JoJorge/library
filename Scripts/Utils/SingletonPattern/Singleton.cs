namespace SingletonPattern
{
    using System;

    /// <summary>
    /// 可繼承的泛型抽象單例基底類別。
    /// 子類別僅需繼承即可獲得完整的單例功能（延遲初始化、執行緒安全、反射防護）。
    /// </summary>
    /// <typeparam name="T">子類別型別，受限為 <see cref="Singleton{T}"/> 的子類別。</typeparam>
    public abstract class Singleton<T>
        where T : Singleton<T>
    {
        /// <summary>
        /// 唯一實例參考。volatile 確保多執行緒環境下的可見性與有序性。
        /// </summary>
        private static volatile T _instance;

        /// <summary>
        /// 專用鎖定物件，避免對 typeof(T) 加鎖造成潛在死鎖。
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Singleton{T}"/> class.
        /// 防禦反射攻擊：若實例已存在，拋出例外阻止第二個實例產生。
        /// </summary>
        protected Singleton()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException(
                    $"{typeof(T).Name} 實例已存在，禁止建立第二個實例。請使用 {typeof(T).Name}.Instance。");
            }
        }

        /// <summary>
        /// Gets 唯一的 <typeparamref name="T"/> 實例，採用雙重檢查鎖定確保執行緒安全的延遲初始化。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                        {
                            _instance = (T)Activator.CreateInstance(typeof(T), true);
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
