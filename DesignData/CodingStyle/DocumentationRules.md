# Documentation Rules (SA1600-)

驗證程式碼文件的內容與格式應符合規範的規則。

---

## SA1600 — ElementsMustBeDocumented

C# 程式碼元素應包含文件標頭。以下元素需要文件標頭：class、constructor、delegate、enum、event、finalizer、indexer、interface、method、property、record、struct。

Record 的 positional parameter 也應加入 `<param>` 標籤。介面的 `public` 成員預設需要文件；non-public default interface member 依 `documentPrivateElements` 設定決定；明確實作的介面成員繼承介面定義，不需另外寫文件。覆寫可使用 `/// <inheritdoc/>`。

---

## SA1601 — PartialElementsMustBeDocumented

partial 元素（class、method）應包含文件標頭。

建議做法：主要 partial 使用 `<summary>`，其他 partial 改用 `<content>`（不會被 SDK 文件工具收錄，避免重複合併問題）。

**範例：**
```csharp
/// <summary>
/// Represents a customer in the database.
/// </summary>
public partial class Customer { }

/// <content>
/// Contains auto-generated functionality for the Customer class.
/// </content>
public partial class Customer { }
```

---

## SA1604 — ElementDocumentationMustHaveSummary

元素文件標頭應包含 `<summary>` 標籤。

---

## SA1605 — PartialElementDocumentationMustHaveSummary

partial 元素文件標頭應包含 `<summary>` 或 `<content>` 標籤，且標籤應有內容。

---

## SA1606 — ElementDocumentationMustHaveSummaryText

`<summary>` 標籤應有內容。

---

## SA1607 — PartialElementDocumentationMustHaveSummaryText

partial 元素的 `<summary>` 或 `<content>` 標籤應有內容。

---

## SA1608 — ElementDocumentationMustNotHaveDefaultSummary

`<summary>` 應移除 Visual Studio 自動產生的預設說明文字（如 "Summary description for the Example class."）。

---

## SA1609 — PropertyDocumentationMustHaveValue

屬性文件標頭應包含 `<value>` 標籤。

---

## SA1610 — PropertyDocumentationMustHaveValueText

屬性文件標頭的 `<value>` 標籤應有內容。

---

## SA1611 — ElementParametersMustBeDocumented

method、constructor、delegate 或 indexer 應包含所有參數的 `<param>` 文件。

---

## SA1612 — ElementParameterDocumentationMustMatchElementParameters

`<param>` 標籤應與實際參數相符（名稱正確、順序正確、無多餘或遺漏）。

---

## SA1613 — ElementParameterDocumentationMustDeclareParameterName

`<param>` 標籤應包含 `name` 屬性且 `name` 屬性應有內容。

---

## SA1614 — ElementParameterDocumentationMustHaveText

`<param>` 標籤應有內容。

---

## SA1615 — ElementReturnValueMustBeDocumented

有回傳值的元素應包含 `<returns>` 標籤。

---

## SA1616 — ElementReturnValueDocumentationMustHaveText

`<returns>` 標籤應有內容。

---

## SA1617 — VoidReturnValueMustNotBeDocumented

回傳 `void` 的元素不應包含 `<returns>` 標籤，應移除。

---

## SA1618 — GenericTypeParametersMustBeDocumented

泛型元素應包含所有 `<typeparam>` 文件。

---

## SA1619 — GenericTypeParametersMustBeDocumentedPartialClass

generic partial 元素含有 `<summary>`（主要 partial）時，應包含 `<typeparam>` 標籤。若為非主要 partial，請改用 `<content>` 取代 `<summary>`。

---

## SA1620 — GenericTypeParameterDocumentationMustMatchTypeParameters

`<typeparam>` 標籤應與實際泛型型別參數相符（名稱正確、順序正確、無多餘或遺漏）。

---

## SA1621 — GenericTypeParameterDocumentationMustDeclareParameterName

`<typeparam>` 標籤應包含 `name` 屬性且 `name` 屬性應有內容。

---

## SA1622 — GenericTypeParameterDocumentationMustHaveText

`<typeparam>` 標籤應有內容。

---

## SA1623 — PropertySummaryDocumentationMustMatchAccessors

屬性 `<summary>` 開頭文字應與 accessor 組合相符。規則如下：

| Accessor 組合 | summary 開頭 |
|---|---|
| 只有 `get` | "Gets" |
| 只有 `set` | "Sets" |
| `get` + `set` | "Gets or sets" |
| `get` + `init` | "Gets" 或 "Gets or initializes" |
| Boolean 屬性 | 加上 "a value indicating whether" |

set accessor 存取層級比 get 更嚴格時，依 SA1624 規則決定是否在 summary 中省略 set。

**範例：**
```csharp
/// <summary>Gets or sets the name of the customer.</summary>
public string Name { get; set; }

/// <summary>Gets a value indicating whether the item is enabled.</summary>
public bool Enabled { get; }

/// <summary>Gets the age of the customer.</summary>
public int Age { get; private set; }  // private set → summary 只寫 "Gets"
```

---

## SA1624 — PropertySummaryDocumentationMustOmitSetAccessorWithRestrictedAccess

屬性 `<summary>` 不應提及 set accessor，當 set accessor 的存取層級比 get 更受限（如 `private set`）時，外部呼叫者無法存取，應從 summary 文字中省略。

判斷是否需要在 summary 中提及 set 的規則：
1. set 與 get 存取層級相同 → 需要提及
2. 屬性只能從 assembly 內部存取，且 set 為 internal → 需要提及
3. 屬性在 private class 內，set 為任何非 private → 需要提及（等效同層級）
4. set 為 protected 或 protected internal → 需要提及（子類別可存取）

其他情況（如 public 屬性 + private set）→ 省略 set，summary 只寫 "Gets"。

---

## SA1625 — ElementDocumentationMustNotBeCopiedAndPasted

文件中不應有兩個或以上相同內容的標籤，應確保每個標籤的文件描述為獨立撰寫。例外：參數說明為 "The parameter is not used." 時允許重複。

---

## SA1626 — SingleLineCommentsMustNotUseDocumentationStyleSlashes

單行註解不應以三個斜線（`///`）開頭。三斜線應僅用於 XML 文件標頭；一般單行註解應以 `//` 開頭，comment-out 程式碼用 `////`。

---

## SA1627 — DocumentationTextMustNotBeEmpty

文件標頭中不應有空的標籤（如 `<remarks></remarks>`），標籤應有內容。

---

## SA1642 — ConstructorSummaryDocumentationMustBeginWithStandardText

建構子的 `<summary>` 應以標準文字開頭。

| 建構子類型 | 標準開頭 |
|---|---|
| instance constructor | `Initializes a new instance of the <see cref="ClassName"/> class.` |
| instance constructor（struct）| `Initializes a new instance of the <see cref="StructName"/> struct.` |
| static constructor | `Initializes static members of the <see cref="ClassName"/> class.` |
| private instance constructor（相容舊版）| `Prevents a default instance of the <see cref="ClassName"/> class from being created.` |

**範例：**
```csharp
/// <summary>
/// Initializes a new instance of the <see cref="Customer"/> class.
/// </summary>
public Customer() { }

/// <summary>
/// Initializes static members of the <see cref="Customer"/> class.
/// </summary>
static Customer() { }
```

---

## SA1643 — DestructorSummaryDocumentationMustBeginWithStandardText

finalizer（解構子）的 `<summary>` 應以標準文字開頭。標準格式：`Finalizes an instance of the <see cref="ClassName"/> class.`

**範例：**
```csharp
/// <summary>
/// Finalizes an instance of the <see cref="Customer"/> class.
/// </summary>
~Customer() { }
```

---

## SA1648 — InheritDocMustBeUsedWithInheritingClass

`<inheritdoc>` 應僅用於有繼承 base class 或 interface 的元素上。例外：`<inheritdoc cref="..."/>` 明確指定來源時允許。

---

## SA1649 — FileNameMustMatchTypeName

檔案名稱應與檔案中第一個宣告的型別名稱相符。泛型型別的檔案名稱格式依 `fileNamingConvention` 設定（如 `Class1{T}.cs` 或 ``Class1`1.cs``）。partial class 不適用此規則。

---

## SA1651 — DoNotUsePlaceholderElements

文件中不應包含 `<placeholder>` 標籤，應審閱並移除。

---
