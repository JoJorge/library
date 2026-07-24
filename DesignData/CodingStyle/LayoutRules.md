# Layout Rules (SA1500-)

強制規範程式碼的排版與行間距格式。

---

## SA1500 — BracesForMultiLineStatementsMustNotShareLine

多行陳述式的左大括號或右大括號應各自獨立一行（只有註解可以共行）。可透過 `stylecop.json` 的 `allowDoWhileOnClosingBrace` 設定調整。C# 8 switch expression 與 property pattern 的大括號同樣適用此規則。

---

## SA1501 — StatementMustNotBeOnSingleLine

含大括號的 C# 陳述式應展開成多行，左右大括號各自獨立一行。

---

## SA1502 — ElementMustNotBeOnSingleLine

含大括號的 C# 元素（如方法）應展開成多行。例外：屬性、事件、索引子內的短 accessor 允許寫在單行。

---

## SA1503 — BracesMustNotBeOmitted

`if`、`while`、`for` 等陳述式應加上大括號，避免日後新增程式碼時產生難以察覺的 bug。

例外：C# 8 `using var` 宣告不應加大括號（加大括號會改變資源生命週期）。

**範例：**
```csharp
// 正確
if (true)
{
    return this.value;
}

// 正確（using var 例外，不加大括號）
using var stream = new FileStream(path, FileMode.Open);

// 錯誤
if (true)
    return this.value;
```

---

## SA1504 — AllAccessorsMustBeSingleLineOrMultiLine

屬性、索引子或事件的所有 accessor 應統一為單行或統一為多行，不應混合。

---

## SA1505 — OpeningBracesMustNotBeFollowedByBlankLine

左大括號後不應有空白行。

---

## SA1506 — ElementDocumentationHeadersMustNotBeFollowedByBlankLine

文件標頭（`///`）後面不應有空白行，應直接接上所屬元素。

---

## SA1507 — CodeMustNotContainMultipleBlankLinesInARow

程式碼中不應有連續多個空白行，最多只允許一個空白行。

---

## SA1508 — ClosingBracesMustNotBePrecededByBlankLine

右大括號前面不應有空白行。C# 8 switch expression 與 property pattern 同樣適用。

---

## SA1509 — OpeningBracesMustNotBePrecededByBlankLine

左大括號前面不應有空白行。例外：前一行是右大括號時，空白行由 SA1513 要求，此處不算違規。C# 8 switch expression 與 property pattern 同樣適用。

---

## SA1510 — ChainedStatementBlocksMustNotBePrecededByBlankLine

串連的陳述式（`catch`、`finally`、`else`）前面不應有空白行，應直接接在前一個陳述式之後。

---

## SA1511 — WhileDoFooterMustNotBePrecededByBlankLine

`do-while` 陳述式的 `while` 關鍵字前面不應有空白行，`while` 應直接接在 `do` 區塊的右大括號之後。

---

## SA1512 — SingleLineCommentsMustNotBeFollowedByBlankLine

單行註解（`//`）後面不應有空白行。

例外：兩組單行註解區塊之間可以有空白行；以 `////` 開頭的 comment-out 程式碼不受此限。

**範例：**
```csharp
// 正確：註解後直接接程式碼
// Return the value.
return this.enabled;

// 正確：兩組註解間允許空白行
// First comment block.

// Second comment block.
return this.enabled;

// 錯誤：註解後接空白行再接程式碼
// Return the value.

return this.enabled;
```

---

## SA1513 — ClosingBraceMustBeFollowedByBlankLine

右大括號後應有空白行。

例外：右大括號後接 `=>`, `,`, `;`、邏輯運算子、pattern designation，或作為 switch expression/property pattern 的一部分時不需要空白行。連續的右大括號（如巢狀區塊結束）之間也不需要空白行。

**範例：**
```csharp
// 正確
if (condition)
{
    DoSomething();
}

return value;

// 正確（例外：後接 ; 或在 switch expression 中）
var result = x switch
{
    1 => "one",
    _ => "other",
};

// 錯誤
if (condition)
{
    DoSomething();
}
return value;
```

---

## SA1514 — ElementDocumentationHeaderMustBePrecededByBlankLine

文件標頭（`///`）前面應有空白行。例外：文件標頭是其所在範圍的第一個項目時，不需要空白行。

---

## SA1515 — SingleLineCommentMustBePrecededByBlankLine

單行註解（`//`）前面應有空白行。例外：註解是所在範圍的第一個項目；以 `////` 開頭的 comment-out 程式碼不受此限。

---

## SA1516 — ElementsMustBeSeparatedByBlankLine

相鄰的 C# 元素之間應有空白行。多行的 accessor 之間也需要空白行。可透過 `stylecop.json` 的 `blankLinesBetweenUsingGroups` 設定調整 using 群組間的行為。C# 10 file-scoped namespace 宣告後應有空白行。

---

## SA1517 — CodeMustNotContainBlankLinesAtStartOfFile

檔案開頭不應有空白行。

---

## SA1518 — UseLineEndingsCorrectlyAtEndOfFile

檔案結尾的換行符號與專案設定不一致。可透過 `stylecop.json` 設定：`Allow`（預設，允許但不強制結尾換行）、`Require`（必須有結尾換行）、`Omit`（不可有結尾換行）。

---

## SA1519 — BracesMustNotBeOmittedFromMultiLineChildStatement

跨越多行的子陳述式應加上大括號。

---

## SA1520 — UseBracesConsistently

`if`/`else if`/`else` 串連陳述式中，只要任一子句有大括號，所有子句都應統一加上大括號。

---
