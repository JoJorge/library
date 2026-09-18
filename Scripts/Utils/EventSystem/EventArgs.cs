namespace Utils.EventSystem
{
    using System;

    /// <summary>
    /// 事件參數容器（值型別結構體），支援 0～12 個參數，提供型別安全的存取方式。
    /// 常見的 1-2 個 int/long/float/bool 參數使用專用欄位，不產生堆積配置。
    /// </summary>
    public struct EventArgs
    {
        /// <summary>
        /// int 專用欄位（第一個 slot）。
        /// </summary>
        private int _int0;

        /// <summary>
        /// int 專用欄位（第二個 slot）。
        /// </summary>
        private int _int1;

        /// <summary>
        /// long 專用欄位（第一個 slot）。
        /// </summary>
        private long _long0;

        /// <summary>
        /// long 專用欄位（第二個 slot）。
        /// </summary>
        private long _long1;

        /// <summary>
        /// float 專用欄位（第一個 slot）。
        /// </summary>
        private float _float0;

        /// <summary>
        /// float 專用欄位（第二個 slot）。
        /// </summary>
        private float _float1;

        /// <summary>
        /// bool 專用欄位（第一個 slot）。
        /// </summary>
        private bool _bool0;

        /// <summary>
        /// bool 專用欄位（第二個 slot）。
        /// </summary>
        private bool _bool1;

        /// <summary>
        /// 通用參數陣列；<c>null</c> 表示純專用欄位模式，非 null 表示通用路徑可用。
        /// </summary>
        private object[] _params;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs"/> struct.
        /// 通用建構子，用於混合型別或參考型別參數。
        /// </summary>
        /// <param name="args">事件參數（最多 12 個）。</param>
        public EventArgs(params object[] args)
        {
            this._int0 = 0;
            this._int1 = 0;
            this._long0 = 0;
            this._long1 = 0;
            this._float0 = 0f;
            this._float1 = 0f;
            this._bool0 = false;
            this._bool1 = false;
            this._params = args ?? Array.Empty<object>();
        }

        /// <summary>
        /// Gets 通用路徑參數數量。
        /// 通用路徑模式（<c>_params != null</c>）回傳 <c>_params.Length</c>；
        /// 專用欄位模式（<c>_params == null</c>）回傳 0。
        /// 專用欄位（int/long/float/bool）不計入此值，始終可透過 GetInt/GetLong/GetFloat/GetBool 存取。
        /// </summary>
        public int ParamCount => this._params != null
            ? this._params.Length
            : 0;

        /// <summary>
        /// 以 int 專用欄位建構 EventArgs（零堆積配置）。
        /// _params 維持 null，處於純專用欄位模式。
        /// </summary>
        /// <param name="v0">第一個 int slot 值。</param>
        /// <param name="v1">第二個 int slot 值（預設為 0）。</param>
        /// <returns>設定好 int 專用欄位的 <see cref="EventArgs"/> 實例。</returns>
        public static EventArgs Create(int v0, int v1 = default)
            => new EventArgs { _int0 = v0, _int1 = v1 };

        /// <summary>
        /// 以 long 專用欄位建構 EventArgs（零堆積配置）。
        /// _params 維持 null，處於純專用欄位模式。
        /// </summary>
        /// <param name="v0">第一個 long slot 值。</param>
        /// <param name="v1">第二個 long slot 值（預設為 0）。</param>
        /// <returns>設定好 long 專用欄位的 <see cref="EventArgs"/> 實例。</returns>
        public static EventArgs Create(long v0, long v1 = default)
            => new EventArgs { _long0 = v0, _long1 = v1 };

        /// <summary>
        /// 以 float 專用欄位建構 EventArgs（零堆積配置）。
        /// _params 維持 null，處於純專用欄位模式。
        /// </summary>
        /// <param name="v0">第一個 float slot 值。</param>
        /// <param name="v1">第二個 float slot 值（預設為 0）。</param>
        /// <returns>設定好 float 專用欄位的 <see cref="EventArgs"/> 實例。</returns>
        public static EventArgs Create(float v0, float v1 = default)
            => new EventArgs { _float0 = v0, _float1 = v1 };

        /// <summary>
        /// 以 bool 專用欄位建構 EventArgs（零堆積配置）。
        /// _params 維持 null，處於純專用欄位模式。
        /// </summary>
        /// <param name="v0">第一個 bool slot 值。</param>
        /// <param name="v1">第二個 bool slot 值（預設為 false）。</param>
        /// <returns>設定好 bool 專用欄位的 <see cref="EventArgs"/> 實例。</returns>
        public static EventArgs Create(bool v0, bool v1 = default)
            => new EventArgs { _bool0 = v0, _bool1 = v1 };

        /// <summary>
        /// 設定 int 專用欄位值（兩個 slot 同時設定），結構與 Create(int) 一致。
        /// </summary>
        /// <param name="v0">第一個 int slot 值。</param>
        /// <param name="v1">第二個 int slot 值（預設為 0）。</param>
        public void SetInt(int v0, int v1 = default)
        {
            this._int0 = v0;
            this._int1 = v1;
        }

        /// <summary>
        /// 設定 long 專用欄位值（兩個 slot 同時設定），結構與 Create(long) 一致。
        /// </summary>
        /// <param name="v0">第一個 long slot 值。</param>
        /// <param name="v1">第二個 long slot 值（預設為 0）。</param>
        public void SetLong(long v0, long v1 = default)
        {
            this._long0 = v0;
            this._long1 = v1;
        }

        /// <summary>
        /// 設定 float 專用欄位值（兩個 slot 同時設定），結構與 Create(float) 一致。
        /// </summary>
        /// <param name="v0">第一個 float slot 值。</param>
        /// <param name="v1">第二個 float slot 值（預設為 0）。</param>
        public void SetFloat(float v0, float v1 = default)
        {
            this._float0 = v0;
            this._float1 = v1;
        }

        /// <summary>
        /// 設定 bool 專用欄位值（兩個 slot 同時設定），結構與 Create(bool) 一致。
        /// </summary>
        /// <param name="v0">第一個 bool slot 值。</param>
        /// <param name="v1">第二個 bool slot 值（預設為 false）。</param>
        public void SetBool(bool v0, bool v1 = default)
        {
            this._bool0 = v0;
            this._bool1 = v1;
        }

        /// <summary>
        /// 設定通用參數陣列，用於攜帶參考型別或混合型別參數。
        /// 此方法會覆寫現有的 _params 陣列（null 時退回 <see cref="Array.Empty{T}"/>），
        /// ParamCount 將反映新陣列長度。
        /// </summary>
        /// <param name="args">事件參數（最多 12 個）。</param>
        public void SetParams(params object[] args)
        {
            this._params = args ?? Array.Empty<object>();
        }

        // ─── 專用型別存取器（無 boxing） ───

        /// <summary>
        /// 從專用 int 欄位取值。index 僅支援 0 或 1，越界回傳 default 並記錄警告。
        /// </summary>
        /// <param name="index">欄位索引（僅支援 0 或 1）。</param>
        /// <returns>對應 slot 的 int 值；索引越界時回傳 <c>default</c>。</returns>
        public int GetInt(int index)
        {
            if (index == 0)
            {
                return this._int0;
            }

            if (index == 1)
            {
                return this._int1;
            }

            Logger.LogWarning(
                $"[EventArgs] GetInt index {index} out of range (0-1).");
            return default;
        }

        /// <summary>
        /// 從專用 long 欄位取值。index 僅支援 0 或 1，越界回傳 default 並記錄警告。
        /// </summary>
        /// <param name="index">欄位索引（僅支援 0 或 1）。</param>
        /// <returns>對應 slot 的 long 值；索引越界時回傳 <c>default</c>。</returns>
        public long GetLong(int index)
        {
            if (index == 0)
            {
                return this._long0;
            }

            if (index == 1)
            {
                return this._long1;
            }

            Logger.LogWarning(
                $"[EventArgs] GetLong index {index} out of range (0-1).");
            return default;
        }

        /// <summary>
        /// 從專用 float 欄位取值。index 僅支援 0 或 1，越界回傳 default 並記錄警告。
        /// </summary>
        /// <param name="index">欄位索引（僅支援 0 或 1）。</param>
        /// <returns>對應 slot 的 float 值；索引越界時回傳 <c>default</c>。</returns>
        public float GetFloat(int index)
        {
            if (index == 0)
            {
                return this._float0;
            }

            if (index == 1)
            {
                return this._float1;
            }

            Logger.LogWarning(
                $"[EventArgs] GetFloat index {index} out of range (0-1).");
            return default;
        }

        /// <summary>
        /// 從專用 bool 欄位取值。index 僅支援 0 或 1，越界回傳 default 並記錄警告。
        /// </summary>
        /// <param name="index">欄位索引（僅支援 0 或 1）。</param>
        /// <returns>對應 slot 的 bool 值；索引越界時回傳 <c>default</c>。</returns>
        public bool GetBool(int index)
        {
            if (index == 0)
            {
                return this._bool0;
            }

            if (index == 1)
            {
                return this._bool1;
            }

            Logger.LogWarning(
                $"[EventArgs] GetBool index {index} out of range (0-1).");
            return default;
        }

        // ─── 泛型存取器（通用路徑，需 object[] 已配置） ───

        /// <summary>
        /// 以型別安全方式取得指定索引位置的參數值（通用路徑）。
        /// 專用欄位模式（_params == null）會記錄警告並回傳 default；
        /// 索引越界回傳 default 並記錄警告；null 值回傳 default 不記錄警告；
        /// 型別相容（is T）回傳值；型別不匹配回傳 default 並記錄警告。
        /// </summary>
        /// <typeparam name="T">期望的參數型別。</typeparam>
        /// <param name="index">參數索引（從 0 開始）。</param>
        /// <returns>參數值；若索引越界或型別不匹配則回傳 <c>default(T)</c>。</returns>
        public T GetParam<T>(int index)
        {
            if (this._params == null)
            {
                Logger.LogWarning(
                    $"[EventArgs] Get<T> called on dedicated-slot instance. " +
                    $"Use GetInt/GetLong/GetFloat/GetBool instead.");
                return default;
            }

            if (index < 0 || index >= this._params.Length)
            {
                Logger.LogWarning(
                    $"[EventArgs] Index {index} out of range (ParamCount={this._params.Length}).");
                return default;
            }

            object value = this._params[index];

            if (value == null)
            {
                return default;
            }

            if (value is T typed)
            {
                return typed;
            }

            Logger.LogWarning(
                $"[EventArgs] Type mismatch at index {index}: " +
                $"expected {typeof(T).Name}, actual {value.GetType().Name}.");
            return default;
        }
    }
}
