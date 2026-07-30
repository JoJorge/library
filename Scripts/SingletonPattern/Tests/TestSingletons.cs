namespace SingletonPattern.Tests
{
    /// <summary>
    /// 基本測試用單例子類別，用於驗證 <see cref="Singleton{T}"/> 的核心功能。
    /// </summary>
    public class TestSingleton : Singleton<TestSingleton>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestSingleton"/> class.
        /// </summary>
        protected TestSingleton()
        {
        }
    }

    /// <summary>
    /// 第二個測試用單例子類別，用於驗證子類別實例隔離性（Property 6）。
    /// </summary>
    public class AnotherTestSingleton : Singleton<AnotherTestSingleton>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnotherTestSingleton"/> class.
        /// </summary>
        protected AnotherTestSingleton()
        {
        }
    }

    /// <summary>
    /// 測試用單例子類別，僅提供帶參數的建構子（無無參建構子），
    /// 用於驗證 <see cref="Singleton{T}"/> 在子類別缺少無參建構子時拋出 <see cref="MissingMethodException"/>。
    /// </summary>
    public class NoParameterlessCtorSingleton : Singleton<NoParameterlessCtorSingleton>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoParameterlessCtorSingleton"/> class.
        /// 此建構子需要參數，故 <c>Activator.CreateInstance(typeof(T), true)</c> 無法找到無參建構子。
        /// </summary>
        /// <param name="value">初始化用的值。</param>
        protected NoParameterlessCtorSingleton(int value)
        {
        }
    }

    /// <summary>
    /// 含執行緒安全狀態屬性的測試用單例子類別，用於驗證並發狀態修改的安全性（Property 5）。
    /// </summary>
    /// <remarks>
    /// <para><c>State</c> 屬性的讀取透過 <c>volatile</c> 保證跨執行緒可見性。</para>
    /// <para><c>State</c> 屬性的寫入透過 <c>lock(StateLock)</c> 保證原子性。</para>
    /// </remarks>
    public class StatefulSingleton : Singleton<StatefulSingleton>
    {
        /// <summary>
        /// 狀態鎖定物件，確保 <see cref="State"/> setter 的原子性。
        /// </summary>
        private static readonly object StateLock = new object();

        /// <summary>
        /// 內部狀態欄位。volatile 確保跨執行緒讀取的可見性。
        /// </summary>
        private volatile string _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatefulSingleton"/> class.
        /// </summary>
        protected StatefulSingleton()
        {
        }

        /// <summary>
        /// Gets or sets 單例的內部狀態。
        /// get 由 volatile 保證跨執行緒可見性；set 由 lock 保證原子性。
        /// </summary>
        public string State
        {
            get
            {
                return this._state;
            }

            set
            {
                lock (StateLock)
                {
                    this._state = value;
                }
            }
        }
    }
}
