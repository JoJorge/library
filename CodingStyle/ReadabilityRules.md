# Readability Rules (SA1100-)

確保程式碼格式良好、易於閱讀的規則。

---

## SA1100 — DoNotPrefixCallsWithBaseUnlessLocalImplementationExists

呼叫繼承成員時使用了 `base.` 前綴，但本地類別並未覆寫該成員。

若本地類別未覆寫基底類別成員，使用 `base.` 呼叫可能造成日後新增覆寫時產生 bug（永遠繞過本地實作）。應改用 `this.` 呼叫。

**範例：**
```csharp
// 正確（本地未覆寫時用 this.）
string name = this.JoinName("John", "Doe");

// 錯誤
string name = base.JoinName("John", "Doe");
```

---

## SA1102 — QueryClauseShouldFollowPreviousClause

LINQ 查詢子句未接在前一個子句的同行或下一行。

**範例：**
```csharp
// 正確
var x = from num in numbers select num;

var x =
    from num in numbers
    select num;

// 錯誤（中間有空行）
var x = from num in numbers

    select num;
```

---

## SA1103 — QueryClausesShouldBeOnSeparateLinesOrAllOnOneLine

LINQ 查詢子句未全部放在同一行，也未每個子句各佔一行。

**範例：**
```csharp
// 正確（全同行）
var x = from num in numbers select num;

// 正確（每句各一行）
var x =
    from num in numbers
    select num;

// 錯誤（混合）
var x = from num in numbers
    select num;
```

---

## SA1104 — QueryClauseShouldBeginOnNewLineWhenPreviousClauseSpansMultipleLines

前一個查詢子句跨越多行時，下一個子句卻接在前一子句的結尾同行。

**範例：**
```csharp
// 正確
var names =
    from element in GetElements(
        12, 45)
    select element.Name;

// 錯誤
var names =
    from element in GetElements(
        12, 45) select element.Name;
```

---

## SA1105 — QueryClausesSpanningMultipleLinesShouldBeginOnOwnLine

跨越多行的查詢子句未從自己的行開始。

**範例：**
```csharp
// 正確
var result =
    from element in elements
    select GenerateDescription(
        element);

// 錯誤
var result =
    from element in elements select GenerateDescription(
        element);
```

---

## SA1106 — CodeMustNotContainEmptyStatements

程式碼中含有多餘的分號（空陳述式）。C# 8 `using var` 宣告結尾的分號不在此限。

**範例：**
```csharp
// 正確
int x = 1;

// 錯誤
int x = 1;;
```

---

## SA1107 — CodeMustNotContainMultipleStatementsOnOneLine

同一行放了多個陳述式。每個陳述式應各自起始於新行。

**範例：**
```csharp
// 正確
int x = 1;
int y = 2;

// 錯誤
int x = 1; int y = 2;
```

---

## SA1108 — BlockStatementsMustNotContainEmbeddedComments

區塊陳述式宣告與左大括號之間插入了一般註解。

例外：以 `////` 開頭的 comment-out 程式碼可放在此位置。

**範例：**
```csharp
// 正確（註解在上方）
// Make sure x does not equal y
if (x != y)
{
}

// 正確（comment-out）
if (x != y)
////if (x == y)
{
}

// 錯誤
if (x != y)
// Make sure x does not equal y
{
}
```

---

## SA1110 — OpeningParenthesisMustBeOnDeclarationLine

方法/索引器呼叫或宣告的左括號不在方法名稱同行。也適用於建構子呼叫與 target-typed `new`。

**範例：**
```csharp
// 正確
public string JoinName(string first, string last) { }
string name = JoinStrings(first, last);

// 錯誤
public string JoinName
    (string first, string last) { }
```

---

## SA1111 — ClosingParenthesisMustBeOnLineOfLastParameter

方法/索引器的右括號不在最後一個參數的同行。也適用於建構子呼叫與 target-typed `new`。

**範例：**
```csharp
// 正確
string name = JoinStrings(
    first,
    last);

// 錯誤
string name = JoinStrings(
    first,
    last
);
```

---

## SA1112 — ClosingParenthesisMustBeOnLineOfOpeningParenthesis

無參數的方法/索引器，右括號不在左括號的同行。也適用於無引數的 `new()`。

**範例：**
```csharp
// 正確
string name = GetName();

// 錯誤
string name = GetName(
);
```

---

## SA1113 — CommaMustBeOnSameLineAsPreviousParameter

參數間的逗號不在前一個參數的同行。也適用於 `new(...)` 呼叫。

**範例：**
```csharp
// 正確
JoinName(
    first,
    last);

// 錯誤
JoinName(
    first
    , last);
```

---

## SA1114 — ParameterListMustFollowDeclaration

參數列表的起始位置與左括號之間有空行。參數列表應從左括號同行或下一行開始。

**範例：**
```csharp
// 正確
public string JoinName(string first, string last) { }
public string JoinName(
    string first, string last) { }

// 錯誤
public string JoinName(

    string first, string last) { }
```

---

## SA1115 — ParameterMustFollowComma

參數與前一個逗號之間有空行。參數應接在前一逗號的同行或下一行。

**範例：**
```csharp
// 正確
public string JoinName(
    string first,
    string last) { }

// 錯誤
public string JoinName(
    string first,

    string last) { }
```

---

## SA1117 — ParametersMustBeOnSameLineOrSeparateLines

參數既不全在同一行，也不各自獨立一行（混合排列）。

**範例：**
```csharp
// 正確（全同行）
public string JoinName(string first, string middle, string last) { }

// 正確（各自一行）
public string JoinName(
    string first,
    string middle,
    string last) { }

// 錯誤
public string JoinName(string first, string middle,
    string last) { }
```

---

## SA1118 — ParameterMustNotSpanMultipleLines

除第一個參數外，其他參數跨越多行。以下情況除外：第一個參數、匿名方法/lambda、invocation 運算式、物件/陣列建立運算式、`with` 運算式（C# 9）。

**範例：**
```csharp
// 正確
string last = "Smith" + " Doe";
return JoinStrings("John", last);

// 錯誤（第二個參數跨行）
return JoinStrings(
    "John",
    "Smith" +
    " Doe");
```

---

## SA1120 — CommentsMustContainText

註解中沒有任何文字內容。

**範例：**
```csharp
// 錯誤
//
/* */
```

---

## SA1121 — UseBuiltInTypeAlias

使用了基本型別的完整名稱或 BCL 名稱，而非 C# 內建別名。

| 應使用 | 不應使用 |
|--------|---------|
| `bool` | `Boolean`, `System.Boolean` |
| `int` | `Int32`, `System.Int32` |
| `string` | `String`, `System.String` |
| `long` | `Int64`, `System.Int64` |
| `double` | `Double`, `System.Double` |
| （其餘類推） | |

C# 11 + .NET 7 起，`nint`/`nuint` 與 `IntPtr`/`UIntPtr` 同樣適用此規則。

**範例：**
```csharp
// 正確
int x = 1;
string name = "test";

// 錯誤
Int32 x = 1;
System.String name = "test";
```

---

## SA1122 — UseStringEmptyForEmptyStrings

程式碼中含有硬編碼的空字串 `""`，應改用 `string.Empty`。

**範例：**
```csharp
// 正確
string s = string.Empty;

// 錯誤
string s = "";
```

---

## SA1123 — DoNotPlaceRegionsWithinElements

在程式碼元素（方法、屬性等）的 body 內使用了 `#region`。隱藏元素內部的程式碼容易導致維護上的錯誤判斷。

**範例：**
```csharp
// 錯誤
public void Method()
{
    #region Logic
    int x = 1;
    #endregion
}
```

---

## SA1125 — UseShorthandForNullableTypes

使用了 `Nullable<T>` 語法而非 C# 簡寫的 `T?`。僅適用於 value type nullable；nullable reference type（如 `string?`）不受此規則影響。

**範例：**
```csharp
// 正確
int? count;
DateTime? timestamp;

// 錯誤
Nullable<int> count;
System.Nullable<DateTime> timestamp;
```

---

## SA1127 — GenericTypeConstraintsMustBeOnOwnLine

泛型型別或方法宣告的 `where` 子句與宣告放在同一行。每個 `where` 子句應各自獨立一行。

**範例：**
```csharp
// 正確
private void Method<T, R>()
    where T : class
    where R : class, new()
{
}

// 錯誤
private void Method<T, R>() where T : class where R : class, new()
{
}
```

---

## SA1129 — DoNotUseDefaultValueTypeConstructor

使用了 `new T()` 語法建立 value type 的預設值，應改用 `default(T)` 或 `default`。

`new ImmutableArray<int>()` 看似建立可用的陣列，實際上只是預設實例，使用時會拋出 `NullReferenceException`。

**範例：**
```csharp
// 正確
ImmutableArray<int> array = default(ImmutableArray<int>);

// 錯誤
ImmutableArray<int> array = new ImmutableArray<int>();
```

---

## SA1130 — UseLambdaSyntax

使用了舊式 `delegate (params) { }` 匿名方法語法，應改用 lambda 運算式 `(params) => { }`。

注意：當 `delegate { }` 與 lambda 語意不同時（如作為 `Expression<Action>` 引數），不觸發此規則。

**範例：**
```csharp
// 正確
Action a = () => { x = 0; };
Func<int, int, int> c = (m, n) => m + n;

// 錯誤
Action a = delegate { x = 0; };
Func<int, int, int> c = delegate(int m, int n) { return m + n; };
```

---

## SA1131 — UseReadableConditions

比較運算式中，變數出現在右側、字面值/常數出現在左側（Yoda condition）。

字面值/常數包含：數值字面值、字串字面值、`null`、`default(T)`、編譯期常數、`static readonly` 欄位（如 `IntPtr.Zero`）、模式。

**範例：**
```csharp
// 正確
if (value == null) { }

// 錯誤（Yoda condition）
if (null == value) { }
```

---

## SA1132 — DoNotCombineFields

兩個以上的欄位在同一個欄位宣告語法中宣告。

**範例：**
```csharp
// 正確
private int field1;
private int field2;

// 錯誤
private int field1, field2;
```

---

## SA1134 — AttributesMustNotShareLine

多個屬性（Attribute）放在同一行，或屬性與其修飾的元素放在同一行。例外：參數與型別參數上的屬性不受此限。

**範例：**
```csharp
// 正確
[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class MyProvider : CodeFixProvider { }

// 錯誤
[ExportCodeFixProvider(LanguageNames.CSharp)][Shared]
public class MyProvider : CodeFixProvider { }
```

---

## SA1135 — UsingDirectivesMustBeQualified

在 namespace 內的 `using` 指示詞未使用完整限定名稱。同一 namespace 內的類別 alias 定義不需要限定。

**範例：**
```csharp
// 正確
namespace System.Threading
{
    using System.IO;
    using System.Threading.Tasks;
    using T = Thread; // alias 不需限定
}

// 錯誤
namespace System.Threading
{
    using IO;
    using Tasks;
}
```

---

## SA1136 — EnumValuesShouldBeOnSeparateLines

多個 enum 值放在同一行。每個 enum 值應各自獨立一行。

**範例：**
```csharp
// 正確
public enum ExampleEnum
{
    FirstValue,
    SecondValue,
}

// 錯誤
public enum ExampleEnum
{
    FirstValue, SecondValue,
}
```

---

## SA1137 — ElementsShouldHaveTheSameIndentation

同層級且各自起始於新行的兩個以上元素，縮排層級不一致。

注意：僅檢查同一群組內的相對縮排，不同群組（不同方法）之間的縮排差異不觸發此規則。

屬性列表（Attribute list）優先級低於元素本身，由第一個非屬性的同層元素決定縮排基準。`switch` 中的 `case`/`default` 標籤、其他標籤、一般陳述式各自為一組。

**範例：**
```csharp
// 正確
public void Method()
{
    A();
    B();
}

// 錯誤
public void Method()
{
    A();
   B(); // 縮排不一致
}
```

---

## SA1139 — UseLiteralsSuffixNotationInsteadOfCasting

對數值字面值使用了型別轉換，應改用字面值後綴標記法。

後綴：`U`（uint）、`L`（long）、`UL`（ulong）、`F`（float）、`D`（double）、`M`（decimal）（不區分大小寫）。

**範例：**
```csharp
// 正確
var x = 1L;
var y = 1.0f;
var z = 1.0m;

// 錯誤
var x = (long)1;
var y = (float)1.0;
```

---

## SA1141 — UseTupleSyntax

（C# 7.0+）使用了 `ValueTuple<T1, T2>` 型別宣告，應改用 tuple 語法 `(T1, T2)`。

**範例：**
```csharp
// 正確
(int, int) x;

// 錯誤
ValueTuple<int, int> x;
```

---

## SA1142 — ReferToTupleElementsByName

（C# 7.0+）以 metadata 名稱（`Item1`、`Item2` 等）存取 tuple 元素，而非使用已命名的元素名稱。

**範例：**
```csharp
// 正確
(int valueA, int valueB) x;
var y = x.valueA;

// 錯誤
var y = x.Item1;
```

---

*來源：[`documentation/ReadabilityRules.md`](../documentation/ReadabilityRules.md) 及對應各規則 `.md` 檔。*
