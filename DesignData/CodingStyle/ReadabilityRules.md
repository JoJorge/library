# Readability Rules (SA1100-)

確保程式碼格式良好、易於閱讀的規則。

---

## SA1100 — DoNotPrefixCallsWithBaseUnlessLocalImplementationExists

呼叫繼承成員時不應使用 `base.` 前綴（除非本地有覆寫）。若本地類別未覆寫基底類別成員，使用 `base.` 呼叫可能造成日後新增覆寫時產生 bug（永遠繞過本地實作）。應改用 `this.` 呼叫。

---

## SA1102 — QueryClauseShouldFollowPreviousClause

LINQ 查詢子句應接在前一個子句的同行或下一行，中間不可有空行。

---

## SA1103 — QueryClausesShouldBeOnSeparateLinesOrAllOnOneLine

LINQ 查詢子句應全部放在同一行或每個子句各佔一行，不應混合排列。

---

## SA1104 — QueryClauseShouldBeginOnNewLineWhenPreviousClauseSpansMultipleLines

前一個查詢子句跨越多行時，下一個子句應從新行開始。

---

## SA1105 — QueryClausesSpanningMultipleLinesShouldBeginOnOwnLine

跨越多行的查詢子句應從自己的行開始。

---

## SA1106 — CodeMustNotContainEmptyStatements

程式碼中不應有多餘的分號（空陳述式）。C# 8 `using var` 宣告結尾的分號不在此限。

---

## SA1107 — CodeMustNotContainMultipleStatementsOnOneLine

每個陳述式應各自起始於新行。

---

## SA1108 — BlockStatementsMustNotContainEmbeddedComments

不應在宣告與左大括號之間插入註解。例外：以 `////` 開頭的 comment-out 程式碼可放在此位置。

**範例：**
```csharp
// 正確（comment-out 例外）
if (x != y)
////if (x == y)
{
}

// 錯誤（註解夾在宣告與大括號間）
if (x != y)
// Make sure x does not equal y
{
}
```

---

## SA1110 — OpeningParenthesisMustBeOnDeclarationLine

方法/索引器呼叫或宣告的左括號應在方法名稱同行。也適用於建構子呼叫與 target-typed `new`。

---

## SA1111 — ClosingParenthesisMustBeOnLineOfLastParameter

方法/索引器的右括號應在最後一個參數的同行。也適用於建構子呼叫與 target-typed `new`。

---

## SA1112 — ClosingParenthesisMustBeOnLineOfOpeningParenthesis

無參數的方法/索引器，右括號應在左括號的同行。也適用於無引數的 `new()`。

---

## SA1113 — CommaMustBeOnSameLineAsPreviousParameter

參數間的逗號應在前一個參數的同行。也適用於 `new(...)` 呼叫。

---

## SA1114 — ParameterListMustFollowDeclaration

參數列表應從左括號同行或下一行開始，不應有空行間隔。

---

## SA1115 — ParameterMustFollowComma

參數應接在前一逗號的同行或下一行，不應有空行間隔。

---

## SA1117 — ParametersMustBeOnSameLineOrSeparateLines

參數應全部放在同一行或各自獨立一行，不應混合排列。

---

## SA1118 — ParameterMustNotSpanMultipleLines

除第一個參數外，其他參數不應跨越多行。以下情況除外：第一個參數、匿名方法/lambda、invocation 運算式、物件/陣列建立運算式、`with` 運算式（C# 9）。

---

## SA1120 — CommentsMustContainText

註解中應包含文字內容。

---

## SA1121 — UseBuiltInTypeAlias

應使用 C# 內建別名（`bool`、`int`、`string`、`long`、`double` 等），而非基本型別的完整名稱或 BCL 名稱（如 `Int32`、`System.String`）。C# 11 + .NET 7 起，`nint`/`nuint` 與 `IntPtr`/`UIntPtr` 同樣適用。

---

## SA1122 — UseStringEmptyForEmptyStrings

應使用 `string.Empty` 表示空字串，不應硬編碼 `""`。

---

## SA1123 — DoNotPlaceRegionsWithinElements

不應在元素 body 內使用 `#region`。隱藏元素內部的程式碼容易導致維護上的錯誤判斷。

---

## SA1125 — UseShorthandForNullableTypes

應使用 `T?` 簡寫，而非 `Nullable<T>` 語法。僅適用於 value type nullable；nullable reference type（如 `string?`）不受此規則影響。

---

## SA1127 — GenericTypeConstraintsMustBeOnOwnLine

泛型型別或方法宣告的每個 `where` 子句應各自獨立一行。

---

## SA1129 — DoNotUseDefaultValueTypeConstructor

應使用 `default(T)` 或 `default` 建立 value type 的預設值，不應使用 `new T()` 語法。`new ImmutableArray<int>()` 看似建立可用的陣列，實際上只是預設實例，使用時會拋出 `NullReferenceException`。

---

## SA1130 — UseLambdaSyntax

應使用 lambda 運算式 `(params) => { }`，而非舊式 `delegate (params) { }` 匿名方法語法。當 `delegate { }` 與 lambda 語意不同時（如作為 `Expression<Action>` 引數），不觸發此規則。

---

## SA1131 — UseReadableConditions

比較運算式中，變數應出現在左側，字面值/常數應出現在右側（禁止 Yoda condition）。字面值/常數包含：數值字面值、字串字面值、`null`、`default(T)`、編譯期常數、`static readonly` 欄位（如 `IntPtr.Zero`）、模式。

---

## SA1132 — DoNotCombineFields

每個欄位應各自獨立宣告，不應在同一個宣告語法中宣告多個欄位。

---

## SA1134 — AttributesMustNotShareLine

每個屬性（Attribute）應各自獨立一行，不應與其他屬性或其修飾的元素放在同一行。例外：參數與型別參數上的屬性不受此限。

---

## SA1135 — UsingDirectivesMustBeQualified

在 namespace 內的 `using` 指示詞應使用完整限定名稱。例外：同一 namespace 內的類別 alias 定義不需要限定。

**範例：**
```csharp
namespace System.Threading
{
    using System.IO;           // 正確：完整限定
    using T = Thread;          // 正確：alias 不需限定
    // using IO;              // 錯誤：未限定
}
```

---

## SA1136 — EnumValuesShouldBeOnSeparateLines

每個 enum 值應各自獨立一行。

---

## SA1137 — ElementsShouldHaveTheSameIndentation

同層級且各自起始於新行的兩個以上元素，縮排層級應一致。僅檢查同一群組內的相對縮排。屬性列表（Attribute list）優先級低於元素本身，由第一個非屬性的同層元素決定縮排基準。`switch` 中的 `case`/`default` 標籤、其他標籤、一般陳述式各自為一組。

---

## SA1139 — UseLiteralsSuffixNotationInsteadOfCasting

應使用字面值後綴標記法（`U`、`L`、`UL`、`F`、`D`、`M`），不應對數值字面值使用型別轉換。

---

## SA1141 — UseTupleSyntax

（C# 7.0+）應使用 tuple 語法 `(T1, T2)`，而非 `ValueTuple<T1, T2>` 型別宣告。

---

## SA1142 — ReferToTupleElementsByName

（C# 7.0+）應以命名的元素名稱存取 tuple 元素，而非使用 metadata 名稱（`Item1`、`Item2` 等）。

---
