# Layout Rules (SA1500-)

強制規範程式碼的排版與行間距格式。

---

## SA1500 — BracesForMultiLineStatementsMustNotShareLine

多行陳述式的左大括號或右大括號與其他程式碼共用同一行。大括號必須各自獨立一行（只有註解可以共行）。可透過 `stylecop.json` 的 `allowDoWhileOnClosingBrace` 設定調整。C# 8 switch expression 與 property pattern 的大括號同樣適用此規則。

**範例：**
```csharp
// 正確
lock (this)
{
    return this.value;
}

// 錯誤
lock (this) {
    return this.value;
}
```

---

## SA1501 — StatementMustNotBeOnSingleLine

含大括號的 C# 陳述式寫在同一行。陳述式應展開成多行，左右大括號各自獨立一行。

**範例：**
```csharp
// 正確
lock (this)
{
    return this.value;
}

// 錯誤
lock (this) { return this.value; }
```

---

## SA1502 — ElementMustNotBeOnSingleLine

含大括號的 C# 元素（如方法）寫在同一行。應展開成多行。例外：屬性、事件、索引子內的短 accessor 允許寫在單行。

**範例：**
```csharp
// 正確
public object Method()
{
    return null;
}

// 錯誤
public object Method() { return null; }
```

---

## SA1503 — BracesMustNotBeOmitted

`if`、`while`、`for` 等陳述式省略了大括號。應一律加上大括號，避免日後新增程式碼時產生難以察覺的 bug。例外：C# 8 `using var` 宣告（加大括號會改變資源生命週期）。

**範例：**
```csharp
// 正確
if (true)
{
    return this.value;
}

// 錯誤
if (true)
    return this.value;
```

---

## SA1504 — AllAccessorsMustBeSingleLineOrMultiLine

屬性、索引子或事件的 accessor 中，有的寫單行、有的寫多行（不一致）。所有 accessor 應統一為單行或統一為多行。

**範例：**
```csharp
// 正確（全單行）
public bool Enabled
{
    get { return this.enabled; }
    set { this.enabled = value; }
}

// 錯誤（混合）
public bool Enabled
{
    get { return this.enabled; }
    set
    {
        this.enabled = value;
    }
}
```

---

## SA1505 — OpeningBracesMustNotBeFollowedByBlankLine

左大括號後緊接著空白行。應移除左大括號後的空白行。

**範例：**
```csharp
// 正確
public bool Enabled
{
    get
    {
        return this.enabled;
    }
}

// 錯誤
public bool Enabled
{

    get
    {

        return this.enabled;
    }
}
```

---

## SA1506 — ElementDocumentationHeadersMustNotBeFollowedByBlankLine

文件標頭（`///`）後面緊接著空白行。應移除文件標頭與元素之間的空白行。

**範例：**
```csharp
// 正確
/// <summary>Gets a value.</summary>
public bool Enabled { get; }

// 錯誤
/// <summary>Gets a value.</summary>

public bool Enabled { get; }
```

---

## SA1507 — CodeMustNotContainMultipleBlankLinesInARow

程式碼中連續出現多個空白行。最多只允許一個空白行。

**範例：**
```csharp
// 正確
Console.WriteLine("A");

return this.enabled;

// 錯誤
Console.WriteLine("A");


return this.enabled;
```

---

## SA1508 — ClosingBracesMustNotBePrecededByBlankLine

右大括號前面緊接著空白行。應移除右大括號前的空白行。C# 8 switch expression 與 property pattern 同樣適用。

**範例：**
```csharp
// 正確
public bool Enabled
{
    get
    {
        return this.enabled;
    }
}

// 錯誤
public bool Enabled
{
    get
    {
        return this.enabled;

    }

}
```

---

## SA1509 — OpeningBracesMustNotBePrecededByBlankLine

左大括號前面有空白行。應移除左大括號前的空白行。例外：前一行是右大括號時，空白行由 SA1513 要求，此處不算違規。C# 8 switch expression 與 property pattern 同樣適用。

**範例：**
```csharp
// 正確
public bool Enabled
{
    get
    {
        return this.enabled;
    }
}

// 錯誤
public bool Enabled

{
    get

    {
        return this.enabled;
    }
}
```

---

## SA1510 — ChainedStatementBlocksMustNotBePrecededByBlankLine

串連的陳述式（`catch`、`finally`、`else`）前面有空白行。串連陳述式應直接接在前一個陳述式之後，不可有空白行。

**範例：**
```csharp
// 正確
try
{
    this.SomeMethod();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

// 錯誤
try
{
    this.SomeMethod();
}

catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
```

---

## SA1511 — WhileDoFooterMustNotBePrecededByBlankLine

`do-while` 陳述式的 `while` 關鍵字前面有空白行。`while` 應直接接在 `do` 區塊的右大括號之後。

**範例：**
```csharp
// 正確
do
{
    Console.WriteLine("Loop");
}
while (true);

// 錯誤
do
{
    Console.WriteLine("Loop");
}

while (true);
```

---

## SA1512 — SingleLineCommentsMustNotBeFollowedByBlankLine

單行註解（`//`）後面緊接著空白行。例外：兩組單行註解區塊之間可以有空白行；以 `////` 開頭的 comment-out 程式碼不受此限。

**範例：**
```csharp
// 正確
// Return the value.
return this.enabled;

// 也正確（兩組註解間有空白行）
// First comment block.

// Second comment block.
return this.enabled;

// 錯誤
// Return the value.

return this.enabled;
```

---

## SA1513 — ClosingBraceMustBeFollowedByBlankLine

右大括號後沒有空白行。例外：右大括號後接 `=>`, `,`, `;`、邏輯運算子、pattern designation，或作為 switch expression/property pattern 的一部分時不需要空白行。

**範例：**
```csharp
// 正確
if (condition)
{
    DoSomething();
}

return value;

// 錯誤
if (condition)
{
    DoSomething();
}
return value;
```

---

## SA1514 — ElementDocumentationHeaderMustBePrecededByBlankLine

文件標頭（`///`）前面沒有空白行。例外：文件標頭是其所在範圍的第一個項目時，不需要空白行。

**範例：**
```csharp
// 正確
public bool Visible { get; }

/// <summary>Gets whether enabled.</summary>
public bool Enabled { get; }

// 例外（第一個項目，不需空白行）
public class MyClass
{
    /// <summary>Gets whether enabled.</summary>
    public bool Enabled { get; }
}

// 錯誤
public bool Visible { get; }
/// <summary>Gets whether enabled.</summary>
public bool Enabled { get; }
```

---

## SA1515 — SingleLineCommentMustBePrecededByBlankLine

單行註解（`//`）前面沒有空白行。例外：註解是所在範圍的第一個項目；以 `////` 開頭的 comment-out 程式碼不受此限。

**範例：**
```csharp
// 正確
Console.WriteLine("Getting flag.");

// Return the value.
return this.enabled;

// 例外（第一個項目）
{
    // Return the value.
    return this.enabled;
}

// 錯誤
Console.WriteLine("Getting flag.");
// Return the value.
return this.enabled;
```

---

## SA1516 — ElementsMustBeSeparatedByBlankLine

相鄰的 C# 元素之間沒有空白行。多行的 accessor 之間也需要空白行。可透過 `stylecop.json` 的 `blankLinesBetweenUsingGroups` 設定調整 using 群組間的行為。C# 10 file-scoped namespace 宣告後應有空白行。

**範例：**
```csharp
// 正確
public void Method1() { }

public bool Property { get; }

// 錯誤
public void Method1() { }
public bool Property { get; }
```

---

## SA1517 — CodeMustNotContainBlankLinesAtStartOfFile

檔案開頭有空白行。應移除檔案開頭的所有空白行。

---

## SA1518 — UseLineEndingsCorrectlyAtEndOfFile

檔案結尾的換行符號與專案設定不一致。可透過 `stylecop.json` 設定：`Allow`（預設，允許但不強制結尾換行）、`Require`（必須有結尾換行）、`Omit`（不可有結尾換行）。

---

## SA1519 — BracesMustNotBeOmittedFromMultiLineChildStatement

跨越多行的子陳述式省略了大括號。當子陳述式跨行時，必須加上大括號。

**範例：**
```csharp
// 正確
if (true)
{
    return
        this.value;
}

// 錯誤
if (true)
    return
        this.value;
```

---

## SA1520 — UseBracesConsistently

`if`/`else if`/`else` 串連陳述式中，部分子句有大括號、部分沒有（不一致）。只要任一子句有大括號，所有子句都必須有大括號。

**範例：**
```csharp
// 正確
if (true)
{
    return this.value;
}
else
{
    return that.value;
}

// 錯誤
if (true)
    return this.value;
else
{
    return that.value;
}
```

---

*來源：[`documentation/LayoutRules.md`](../documentation/LayoutRules.md) 及對應各規則 `.md` 檔。*
