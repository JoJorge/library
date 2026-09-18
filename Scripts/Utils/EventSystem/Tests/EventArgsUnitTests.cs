namespace Utils.EventSystem.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// EventArgs 的範例導向單元測試。
    /// 驗證專用 Setter 的單/雙參數設定、混合型別組合存取、SetParams 覆寫、
    /// null 值取出不記錄警告，以及 GetInt 系列越界回傳 default 的具體場景。
    /// </summary>
    [TestFixture]
    public class EventArgsUnitTests
    {
        // ─── SetInt / SetLong / SetFloat / SetBool 單參數（slot 1 預設 0/false） ───

        /// <summary>
        /// 驗證 SetInt 只傳一個參數時，slot 0 設為指定值、slot 1 預設為 0。
        /// </summary>
        [Test]
        public void SetInt_SingleParam_Slot1DefaultsToZero()
        {
            EventArgs args = default;
            args.SetInt(42);

            Assert.That(args.GetInt(0), Is.EqualTo(42), "slot 0 應為傳入值");
            Assert.That(args.GetInt(1), Is.EqualTo(0), "slot 1 應預設為 0");
        }

        /// <summary>
        /// 驗證 SetInt 傳入兩個參數時，兩個 slot 皆設為指定值。
        /// </summary>
        [Test]
        public void SetInt_TwoParams_BothSlotsSet()
        {
            EventArgs args = default;
            args.SetInt(7, 13);

            Assert.That(args.GetInt(0), Is.EqualTo(7), "slot 0 應為第一個傳入值");
            Assert.That(args.GetInt(1), Is.EqualTo(13), "slot 1 應為第二個傳入值");
        }

        /// <summary>
        /// 驗證 SetLong 只傳一個參數時，slot 0 設為指定值、slot 1 預設為 0。
        /// </summary>
        [Test]
        public void SetLong_SingleParam_Slot1DefaultsToZero()
        {
            EventArgs args = default;
            args.SetLong(9_000_000_000L);

            Assert.That(args.GetLong(0), Is.EqualTo(9_000_000_000L), "slot 0 應為傳入值");
            Assert.That(args.GetLong(1), Is.EqualTo(0L), "slot 1 應預設為 0");
        }

        /// <summary>
        /// 驗證 SetLong 傳入兩個參數時，兩個 slot 皆設為指定值。
        /// </summary>
        [Test]
        public void SetLong_TwoParams_BothSlotsSet()
        {
            EventArgs args = default;
            args.SetLong(100L, -200L);

            Assert.That(args.GetLong(0), Is.EqualTo(100L), "slot 0 應為第一個傳入值");
            Assert.That(args.GetLong(1), Is.EqualTo(-200L), "slot 1 應為第二個傳入值");
        }

        /// <summary>
        /// 驗證 SetFloat 只傳一個參數時，slot 0 設為指定值、slot 1 預設為 0。
        /// </summary>
        [Test]
        public void SetFloat_SingleParam_Slot1DefaultsToZero()
        {
            EventArgs args = default;
            args.SetFloat(3.14f);

            Assert.That(args.GetFloat(0), Is.EqualTo(3.14f), "slot 0 應為傳入值");
            Assert.That(args.GetFloat(1), Is.EqualTo(0f), "slot 1 應預設為 0");
        }

        /// <summary>
        /// 驗證 SetFloat 傳入兩個參數時，兩個 slot 皆設為指定值。
        /// </summary>
        [Test]
        public void SetFloat_TwoParams_BothSlotsSet()
        {
            EventArgs args = default;
            args.SetFloat(1.5f, -2.5f);

            Assert.That(args.GetFloat(0), Is.EqualTo(1.5f), "slot 0 應為第一個傳入值");
            Assert.That(args.GetFloat(1), Is.EqualTo(-2.5f), "slot 1 應為第二個傳入值");
        }

        /// <summary>
        /// 驗證 SetBool 只傳一個參數時，slot 0 設為指定值、slot 1 預設為 false。
        /// </summary>
        [Test]
        public void SetBool_SingleParam_Slot1DefaultsToFalse()
        {
            EventArgs args = default;
            args.SetBool(true);

            Assert.That(args.GetBool(0), Is.True, "slot 0 應為傳入值");
            Assert.That(args.GetBool(1), Is.False, "slot 1 應預設為 false");
        }

        /// <summary>
        /// 驗證 SetBool 傳入兩個參數時，兩個 slot 皆設為指定值。
        /// </summary>
        [Test]
        public void SetBool_TwoParams_BothSlotsSet()
        {
            EventArgs args = default;
            args.SetBool(true, false);

            Assert.That(args.GetBool(0), Is.True, "slot 0 應為第一個傳入值");
            Assert.That(args.GetBool(1), Is.False, "slot 1 應為第二個傳入值");
        }

        // ─── 混合型別組合存取 ───

        /// <summary>
        /// 驗證同一實例上設定 int 與 float 專用欄位後，
        /// GetInt 與 GetFloat 皆能取回各自正確的值（專用欄位彼此獨立共存）。
        /// </summary>
        [Test]
        public void MixedTypes_CreateIntThenSetFloat_BothAccessorsReturnCorrectValues()
        {
            EventArgs args = EventArgs.Create(55);
            args.SetFloat(6.28f, 9.81f);

            Assert.That(args.GetInt(0), Is.EqualTo(55), "int slot 0 應維持 Create 設定值");
            Assert.That(args.GetFloat(0), Is.EqualTo(6.28f), "float slot 0 應為 SetFloat 設定值");
            Assert.That(args.GetFloat(1), Is.EqualTo(9.81f), "float slot 1 應為 SetFloat 設定值");
        }

        /// <summary>
        /// 驗證同一實例上同時設定 int/long/float/bool 專用欄位後，各存取器互不干擾。
        /// </summary>
        [Test]
        public void MixedTypes_AllDedicatedSetters_ReturnIndependentValues()
        {
            EventArgs args = default;
            args.SetInt(1, 2);
            args.SetLong(3L, 4L);
            args.SetFloat(5f, 6f);
            args.SetBool(true, true);

            Assert.That(args.GetInt(0), Is.EqualTo(1));
            Assert.That(args.GetInt(1), Is.EqualTo(2));
            Assert.That(args.GetLong(0), Is.EqualTo(3L));
            Assert.That(args.GetLong(1), Is.EqualTo(4L));
            Assert.That(args.GetFloat(0), Is.EqualTo(5f));
            Assert.That(args.GetFloat(1), Is.EqualTo(6f));
            Assert.That(args.GetBool(0), Is.True);
            Assert.That(args.GetBool(1), Is.True);
        }

        // ─── SetParams 覆寫通用路徑 ───

        /// <summary>
        /// 驗證呼叫 SetParams 後，ParamCount 反映新陣列長度，且 Get&lt;T&gt; 取回正確值。
        /// </summary>
        [Test]
        public void SetParams_OverwritesGenericPath_ParamCountAndGetReflectNewArray()
        {
            EventArgs args = default;
            args.SetParams(10, "hello", 2.5f);

            Assert.That(args.ParamCount, Is.EqualTo(3), "ParamCount 應反映新陣列長度");
            Assert.That(args.GetParam<int>(0), Is.EqualTo(10), "index 0 應取回 int 值");
            Assert.That(args.GetParam<string>(1), Is.EqualTo("hello"), "index 1 應取回 string 值");
            Assert.That(args.GetParam<float>(2), Is.EqualTo(2.5f), "index 2 應取回 float 值");
        }

        /// <summary>
        /// 驗證重複呼叫 SetParams 時，後一次會覆寫前一次，ParamCount 反映最後的陣列長度。
        /// </summary>
        [Test]
        public void SetParams_CalledTwice_LastCallWins()
        {
            EventArgs args = new EventArgs(1, 2, 3, 4);
            args.SetParams("only-one");

            Assert.That(args.ParamCount, Is.EqualTo(1), "ParamCount 應為最後一次陣列長度");
            Assert.That(args.GetParam<string>(0), Is.EqualTo("only-one"), "index 0 應取回最後一次設定值");
        }

        // ─── null 值取出不記錄警告（Requirement 2.9） ───

        /// <summary>
        /// 驗證 object[] 中存入 null，透過 Get&lt;T&gt; 取出時回傳 null 且不記錄任何警告。
        /// </summary>
        [Test]
        public void GetGeneric_NullStoredValue_ReturnsNullWithoutWarning()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);
            EventArgs args = default;
            args.SetParams(new object[] { null });

            object result = args.GetParam<object>(0);

            Assert.That(result, Is.Null, "存入 null 應取回 null");

            // 若 Get<T> 對 null 記錄了警告，此檢查會失敗。
            Assert.That(warningLog, Is.Empty, "取出 null 不應記錄任何警告");
        }

        // ─── GetInt/GetLong/GetFloat/GetBool 越界回傳 default ───

        /// <summary>
        /// 驗證 GetInt 索引非 0/1 時回傳 default（0）並記錄警告。
        /// </summary>
        [Test]
        public void GetInt_OutOfBoundsIndex_ReturnsDefaultAndLogsWarning()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);

            EventArgs args = EventArgs.Create(99);

            Assert.That(args.GetInt(2), Is.EqualTo(0), "越界索引應回傳 default");
            Assert.That(warningLog, Does.Contain("GetInt index 2 out of range"), "應記錄警告訊息");
        }

        /// <summary>
        /// 驗證 GetLong 索引非 0/1 時回傳 default（0）並記錄警告。
        /// </summary>
        [Test]
        public void GetLong_OutOfBoundsIndex_ReturnsDefaultAndLogsWarning()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);

            EventArgs args = EventArgs.Create(99L);

            Assert.That(args.GetLong(-1), Is.EqualTo(0L), "越界索引應回傳 default");
            Assert.That(warningLog, Does.Contain("GetLong index -1 out of range"), "應記錄警告訊息");
        }

        /// <summary>
        /// 驗證 GetFloat 索引非 0/1 時回傳 default（0）並記錄警告。
        /// </summary>
        [Test]
        public void GetFloat_OutOfBoundsIndex_ReturnsDefaultAndLogsWarning()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);

            EventArgs args = EventArgs.Create(1.23f);

            Assert.That(args.GetFloat(5), Is.EqualTo(0f), "越界索引應回傳 default");
            Assert.That(warningLog, Does.Contain("GetFloat index 5 out of range"), "應記錄警告訊息");
        }

        /// <summary>
        /// 驗證 GetBool 索引非 0/1 時回傳 default（false）並記錄警告。
        /// </summary>
        [Test]
        public void GetBool_OutOfBoundsIndex_ReturnsDefaultAndLogsWarning()
        {
            var warningLog = string.Empty;
            Logger.SetLogWarningAction(message => warningLog += message);

            EventArgs args = EventArgs.Create(true);

            Assert.That(args.GetBool(3), Is.False, "越界索引應回傳 default");
            Assert.That(warningLog, Does.Contain("GetBool index 3 out of range"), "應記錄警告訊息");
        }
    }
}
