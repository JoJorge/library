namespace Utils.EventSystem.Tests
{
    using System.Linq;
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;

    /// <summary>
    /// 屬性導向測試（Property-Based Tests），使用 FsCheck 3.X 驗證 <see cref="EventArgs"/> 的正確性屬性。
    /// 涵蓋專用欄位往返、Setter 往返、通用路徑往返、型別不匹配與越界防護。
    /// </summary>
    [TestFixture]
    public class EventArgsPropertyTests
    {
        /// <summary>
        /// 專用欄位型別的列舉，用於隨機選取 int/long/float/bool 其中一種。
        /// </summary>
        private enum DedicatedKind
        {
            /// <summary>int 專用欄位。</summary>
            Int,

            /// <summary>long 專用欄位。</summary>
            Long,

            /// <summary>float 專用欄位。</summary>
            Float,

            /// <summary>bool 專用欄位。</summary>
            Bool,
        }

        // ─── 產生器（Generators） ───

        /// <summary>
        /// Property 1: EventArgs Round-Trip（Create路徑）。
        /// 隨機選取 int/long/float/bool 其中一種，產生 1 或 2 個隨機值，
        /// 以對應的 <see cref="EventArgs.Create(int, int)"/> 系列工廠建構，並驗證對應 Getter 往返一致。
        /// </summary>
        // Feature: event-system, Property 1: EventArgs Round-Trip
        [Test]
        public void CreateDedicated_RoundTrips()
        {
            Prop.ForAll(
                GenEventArgsDedicated(),
                sample =>
                {
                    EventArgs args = sample.Args;

                    if (args.ParamCount != 0)
                    {
                        return false;
                    }

                    switch (sample.Kind)
                    {
                        case DedicatedKind.Int:
                            return args.GetInt(0) == (int)sample.V0
                                && args.GetInt(1) == (int)sample.V1;
                        case DedicatedKind.Long:
                            return args.GetLong(0) == (long)sample.V0
                                && args.GetLong(1) == (long)sample.V1;
                        case DedicatedKind.Float:
                            return args.GetFloat(0).Equals((float)sample.V0)
                                && args.GetFloat(1).Equals((float)sample.V1);
                        case DedicatedKind.Bool:
                            return args.GetBool(0) == ((int)sample.V0 != 0)
                                && args.GetBool(1) == ((int)sample.V1 != 0);
                        default:
                            return false;
                    }
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 1: EventArgs Round-Trip（Setter 路徑）。
        /// 以 <see cref="EventArgs.Create(int, int)"/> 建立基底實例後，
        /// 隨機呼叫 SetInt/SetLong/SetFloat/SetBool 組合，並驗證最後一次設定的值可正確取出。
        /// </summary>
        // Feature: event-system, Property 1: EventArgs Round-Trip
        [Test]
        public void SetterPath_RoundTrips()
        {
            Prop.ForAll(
                GenEventArgsSetters(),
                sample =>
                {
                    EventArgs args = sample.Args;

                    if (args.ParamCount != 0)
                    {
                        return false;
                    }

                    switch (sample.Kind)
                    {
                        case DedicatedKind.Int:
                            return args.GetInt(0) == (int)sample.V0
                                && args.GetInt(1) == (int)sample.V1;
                        case DedicatedKind.Long:
                            return args.GetLong(0) == (long)sample.V0
                                && args.GetLong(1) == (long)sample.V1;
                        case DedicatedKind.Float:
                            return args.GetFloat(0).Equals((float)sample.V0)
                                && args.GetFloat(1).Equals((float)sample.V1);
                        case DedicatedKind.Bool:
                            return args.GetBool(0) == ((int)sample.V0 != 0)
                                && args.GetBool(1) == ((int)sample.V1 != 0);
                        default:
                            return false;
                    }
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 1: EventArgs Round-Trip（通用路徑）。
        /// 產生長度 0～12、元素隨機取自 int/long/float/bool/string/object/null 的參數序列，
        /// 透過 params 建構子或 <see cref="EventArgs.SetParams(object[])"/> 建構，
        /// 驗證每個索引以 <c>Get&lt;T&gt;</c> 取出的值與放入值一致，且 <c>ParamCount</c> 等於序列長度。
        /// </summary>
        // Feature: event-system, Property 1: EventArgs Round-Trip
        [Test]
        public void GenericPath_RoundTrips()
        {
            Prop.ForAll(
                GenEventArgsGeneric(),
                sample =>
                {
                    EventArgs args = sample.Args;
                    object[] expected = sample.Values;

                    if (args.ParamCount != expected.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < expected.Length; i++)
                    {
                        if (!RoundTripsAt(args, i, expected[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 2: Type Mismatch Returns Default。
        /// 對於任意包含 int 值的通用路徑 EventArgs 與有效索引，
        /// 以不匹配的型別（string）呼叫 <c>Get&lt;T&gt;</c> 必須回傳 <c>default(T)</c>（即 null）。
        /// </summary>
        // Feature: event-system, Property 2: Type Mismatch Returns Default
        [Test]
        public void TypeMismatch_ReturnsDefault()
        {
            Prop.ForAll(
                Gen.Choose(int.MinValue, int.MaxValue).ToArbitrary(),
                value =>
                {
                    EventArgs args = new EventArgs(value);

                    // 儲存值為 int，但請求 string（無繼承關係），應回傳 default(string) == null。
                    string mismatched = args.GetParam<string>(0);

                    return mismatched == null;
                }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 3: Out-of-Bounds Returns Default。
        /// 對於任意通用路徑 EventArgs 與越界索引（負數或 >= ParamCount），
        /// 以任意型別呼叫 <c>Get&lt;T&gt;</c> 必須回傳 <c>default(T)</c> 且不拋出例外。
        /// </summary>
        // Feature: event-system, Property 3: Out-of-Bounds Returns Default
        [Test]
        public void OutOfBounds_ReturnsDefault()
        {
            Prop.ForAll(
                GenEventArgsGeneric(),
                Gen.Choose(-50, 50).ToArbitrary(),
                (sample, rawIndex) =>
                {
                    EventArgs args = sample.Args;
                    int count = args.ParamCount;

                    // 僅檢驗越界索引；索引落在有效範圍時跳過（回傳 true）。
                    if (rawIndex >= 0 && rawIndex < count)
                    {
                        return true;
                    }

                    // 以參考型別與值型別各驗證一次，確認回傳 default 且不拋例外。
                    string asString = args.GetParam<string>(rawIndex);
                    int asInt = args.GetParam<int>(rawIndex);

                    return asString == null && asInt == default;
                }).QuickCheckThrowOnFailure();
        }

        // ─── 私有輔助方法 ───

        /// <summary>
        /// 針對通用路徑指定索引，依儲存值的實際型別以對應的 <c>Get&lt;T&gt;</c> 驗證往返一致。
        /// null 值以任一型別取出皆應為 default（null）。
        /// </summary>
        /// <param name="args">受測的 <see cref="EventArgs"/> 實例。</param>
        /// <param name="index">參數索引。</param>
        /// <param name="expected">放入該索引的期望值（可能為 null）。</param>
        /// <returns>往返一致回傳 <c>true</c>，否則 <c>false</c>。</returns>
        private static bool RoundTripsAt(EventArgs args, int index, object expected)
        {
            switch (expected)
            {
                case null:
                    return args.GetParam<object>(index) == null;
                case int i:
                    return args.GetParam<int>(index) == i;
                case long l:
                    return args.GetParam<long>(index) == l;
                case float f:
                    return args.GetParam<float>(index).Equals(f);
                case bool b:
                    return args.GetParam<bool>(index) == b;
                case string s:
                    return args.GetParam<string>(index) == s;
                default:
                    return ReferenceEquals(args.GetParam<object>(index), expected);
            }
        }

        /// <summary>
        /// 建立 <see cref="DedicatedSample"/> 產生器：隨機型別 + 1 或 2 個隨機值，經 Create 工廠建構。
        /// </summary>
        /// <returns>專用欄位樣本的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<DedicatedSample> GenEventArgsDedicated()
        {
            Gen<DedicatedSample> gen =
                from kind in Gen.Elements(
                    DedicatedKind.Int,
                    DedicatedKind.Long,
                    DedicatedKind.Float,
                    DedicatedKind.Bool)
                from useSecond in Gen.Elements(false, true)
                from v0 in Gen.Choose(-100000, 100000)
                from v1 in Gen.Choose(-100000, 100000)
                select BuildDedicated(kind, v0, useSecond ? v1 : 0, useSecond, viaSetter: false);

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 建立 <see cref="DedicatedSample"/> 產生器：先以 Create 建立基底，再以對應 Setter 覆寫值。
        /// </summary>
        /// <returns>Setter 路徑樣本的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<DedicatedSample> GenEventArgsSetters()
        {
            Gen<DedicatedSample> gen =
                from kind in Gen.Elements(
                    DedicatedKind.Int,
                    DedicatedKind.Long,
                    DedicatedKind.Float,
                    DedicatedKind.Bool)
                from useSecond in Gen.Elements(false, true)
                from v0 in Gen.Choose(-100000, 100000)
                from v1 in Gen.Choose(-100000, 100000)
                select BuildDedicated(kind, v0, useSecond ? v1 : 0, useSecond, viaSetter: true);

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 依型別與值建構專用欄位樣本；<paramref name="viaSetter"/> 為 true 時走 Setter 路徑，否則走 Create 工廠。
        /// </summary>
        /// <param name="kind">專用欄位型別。</param>
        /// <param name="v0">第一個 slot 的原始整數值（bool 以非零判定）。</param>
        /// <param name="v1">第二個 slot 的原始整數值（bool 以非零判定）。</param>
        /// <param name="useSecond">是否設定第二個 slot（false 時第二 slot 為 default）。</param>
        /// <param name="viaSetter">true 走 Setter，false 走 Create 工廠。</param>
        /// <returns>建構完成的 <see cref="DedicatedSample"/>。</returns>
        private static DedicatedSample BuildDedicated(
            DedicatedKind kind,
            int v0,
            int v1,
            bool useSecond,
            bool viaSetter)
        {
            EventArgs args;

            switch (kind)
            {
                case DedicatedKind.Int:
                    args = viaSetter ? EventArgs.Create(0) : EventArgs.Create(v0, v1);
                    if (viaSetter)
                    {
                        if (useSecond)
                        {
                            args.SetInt(v0, v1);
                        }
                        else
                        {
                            args.SetInt(v0);
                        }
                    }

                    break;

                case DedicatedKind.Long:
                    args = viaSetter ? EventArgs.Create(0L) : EventArgs.Create((long)v0, v1);
                    if (viaSetter)
                    {
                        if (useSecond)
                        {
                            args.SetLong(v0, v1);
                        }
                        else
                        {
                            args.SetLong(v0);
                        }
                    }

                    break;

                case DedicatedKind.Float:
                    args = viaSetter ? EventArgs.Create(0f) : EventArgs.Create((float)v0, v1);
                    if (viaSetter)
                    {
                        if (useSecond)
                        {
                            args.SetFloat(v0, v1);
                        }
                        else
                        {
                            args.SetFloat(v0);
                        }
                    }

                    break;

                default:
                    bool b0 = v0 != 0;
                    bool b1 = v1 != 0;
                    args = viaSetter ? EventArgs.Create(false) : EventArgs.Create(b0, b1);
                    if (viaSetter)
                    {
                        if (useSecond)
                        {
                            args.SetBool(b0, b1);
                        }
                        else
                        {
                            args.SetBool(b0);
                        }
                    }

                    break;
            }

            return new DedicatedSample
            {
                Kind = kind,
                V0 = v0,
                V1 = useSecond ? v1 : 0,
                Args = args,
            };
        }

        /// <summary>
        /// 建立 <see cref="GenericSample"/> 產生器：長度 0～12 的參數序列，
        /// 元素隨機取自 int/long/float/bool/string/object/null，
        /// 隨機選擇以 params 建構子或 <see cref="EventArgs.SetParams(object[])"/> 建構。
        /// </summary>
        /// <returns>通用路徑樣本的 <see cref="Arbitrary{T}"/>。</returns>
        private static Arbitrary<GenericSample> GenEventArgsGeneric()
        {
            Gen<object> elementGen = GenElement();

            Gen<GenericSample> gen =
                from length in Gen.Choose(0, 12)
                from values in elementGen.ListOf(length).Select(list => list.ToArray())
                from viaSetParams in Gen.Elements(false, true)
                select BuildGeneric(values, viaSetParams);

            return gen.ToArbitrary();
        }

        /// <summary>
        /// 依值序列建構通用路徑樣本；<paramref name="viaSetParams"/> 為 true 時以 SetParams 設定，否則走 params 建構子。
        /// </summary>
        /// <param name="values">參數值序列。</param>
        /// <param name="viaSetParams">true 走 SetParams，false 走 params 建構子。</param>
        /// <returns>建構完成的 <see cref="GenericSample"/>。</returns>
        private static GenericSample BuildGeneric(object[] values, bool viaSetParams)
        {
            EventArgs args;

            if (viaSetParams)
            {
                args = EventArgs.Create(0);
                args.SetParams(values);
            }
            else
            {
                args = new EventArgs(values);
            }

            return new GenericSample
            {
                Values = values,
                Args = args,
            };
        }

        /// <summary>
        /// 建立單一通用參數元素的產生器，隨機產生
        /// int/long/float/bool/string/object/null 其中之一。
        /// </summary>
        /// <returns>元素值的 <see cref="Gen{T}"/>。</returns>
        private static Gen<object> GenElement()
        {
            Gen<object> intGen = Gen.Choose(-100000, 100000).Select(i => (object)i);
            Gen<object> longGen = Gen.Choose(-100000, 100000).Select(i => (object)(long)i);
            Gen<object> floatGen = Gen.Choose(-100000, 100000).Select(i => (object)(float)i);
            Gen<object> boolGen = Gen.Elements(false, true).Select(b => (object)b);
            Gen<object> stringGen = Gen.Elements(string.Empty, "abc", "事件", "值").Select(s => (object)s);
            Gen<object> objectGen = Gen.Constant((object)new object());
            Gen<object> nullGen = Gen.Constant((object)null);

            return Gen.OneOf(
                intGen,
                longGen,
                floatGen,
                boolGen,
                stringGen,
                objectGen,
                nullGen);
        }

        /// <summary>
        /// 專用欄位路徑的測試樣本：記錄型別、放入的兩個原始值與建構好的 <see cref="EventArgs"/>。
        /// </summary>
        private struct DedicatedSample
        {
            /// <summary>Gets or sets 專用欄位型別。</summary>
            public DedicatedKind Kind { get; set; }

            /// <summary>Gets or sets 第一個 slot 的原始整數值（bool 以非零判定）。</summary>
            public int V0 { get; set; }

            /// <summary>Gets or sets 第二個 slot 的原始整數值（bool 以非零判定）。</summary>
            public int V1 { get; set; }

            /// <summary>Gets or sets 建構完成的受測實例。</summary>
            public EventArgs Args { get; set; }
        }

        /// <summary>
        /// 通用路徑的測試樣本：記錄放入的值序列與建構好的 <see cref="EventArgs"/>。
        /// </summary>
        private struct GenericSample
        {
            /// <summary>Gets or sets 放入的參數值序列。</summary>
            public object[] Values { get; set; }

            /// <summary>Gets or sets 建構完成的受測實例。</summary>
            public EventArgs Args { get; set; }
        }
    }
}
