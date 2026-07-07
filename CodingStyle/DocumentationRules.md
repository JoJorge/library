# Documentation Rules (SA1600-)

驗證程式碼文件的內容與格式的規則。

---

## SA1600 — ElementsMustBeDocumented

C# 程式碼元素缺少文件標頭。以下元素需要文件標頭：class、constructor、delegate、enum、event、finalizer、indexer、interface、method、property、record、struct。

Record 的 positional parameter 也應加入 `<param>` 標籤。介面的 `public` 成員預設需要文件；non-public default interface member 依 `documentPrivateElements` 設定決定；明確實作的介面成員繼承介面定義，不需另外寫文件。

**範例：**
```csharp
/// <summary>
/// Joins a first name and a last name together into a single string.
/// </summary>
/// <param name="firstName">The first name to join.</param>
/// <param name="lastName">The last name to join.</param>
/// <returns>The joined names.</returns>
public string JoinNames(string firstName, string lastName)
    => firstName + " " + lastName;

// 覆寫可使用 inheritdoc
/// <inheritdoc/>
public override void Accelerate() { }
```

---

## SA1601 — PartialElementsMustBeDocumented

partial 元素（class、method）缺少文件標頭。

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

元素文件標頭缺少 `<summary>` 標籤。

**範例：**
```csharp
// 錯誤（只有 remarks，沒有 summary）
/// <remarks>A remark.</remarks>
public class Customer { }
```

---

## SA1605 — PartialElementDocumentationMustHaveSummary

partial 元素文件標頭缺少 `<summary>` 或 `<content>` 標籤，或標籤為空。

---

## SA1606 — ElementDocumentationMustHaveSummaryText

`<summary>` 標籤內容為空。

**範例：**
```csharp
// 錯誤
/// <summary>  </summary>
public Customer FindCustomer(int id) { }
```

---

## SA1607 — PartialElementDocumentationMustHaveSummaryText

partial 元素的 `<summary>` 或 `<content>` 標籤內容為空。

---

## SA1608 — ElementDocumentationMustNotHaveDefaultSummary

`<summary>` 仍包含 Visual Studio 自動產生的預設說明文字（如 "Summary description for the Example class."）。

---

## SA1609 — PropertyDocumentationMustHaveValue

屬性文件標頭缺少 `<value>` 標籤。

**範例：**
```csharp
/// <summary>Gets the name of the customer.</summary>
/// <value>The name of the customer.</value>
public string Name { get; }
```

---

## SA1610 — PropertyDocumentationMustHaveValueText

屬性文件標頭的 `<value>` 標籤內容為空。

---

## SA1611 — ElementParametersMustBeDocumented

method、constructor、delegate 或 indexer 缺少一個或多個參數的 `<param>` 文件。

**範例：**
```csharp
/// <summary>Joins names.</summary>
/// <param name="firstName">The first name to join.</param>
/// <param name="lastName">The last name to join.</param>
/// <returns>The joined names.</returns>
public string JoinNames(string firstName, string lastName) => firstName + " " + lastName;
```

---

## SA1612 — ElementParameterDocumentationMustMatchElementParameters

`<param>` 標籤與實際參數不符（名稱錯誤、順序不對、多餘或遺漏）。

---

## SA1613 — ElementParameterDocumentationMustDeclareParameterName

`<param>` 標籤缺少 `name` 屬性或 `name` 屬性為空。

---

## SA1614 — ElementParameterDocumentationMustHaveText

`<param>` 標籤內容為空。

---

## SA1615 — ElementReturnValueMustBeDocumented

有回傳值的元素缺少 `<returns>` 標籤。

---

## SA1616 — ElementReturnValueDocumentationMustHaveText

`<returns>` 標籤內容為空。

---

## SA1617 — VoidReturnValueMustNotBeDocumented

回傳 `void` 的元素卻包含 `<returns>` 標籤，應移除。

---

## SA1618 — GenericTypeParametersMustBeDocumented

泛型元素缺少一個或多個 `<typeparam>` 文件。

**範例：**
```csharp
/// <summary>A sample generic class.</summary>
/// <typeparam name="S">The first generic type parameter.</typeparam>
/// <typeparam name="T">The second generic type parameter.</typeparam>
public class Class1<S, T> { }
```

---

## SA1619 — GenericTypeParametersMustBeDocumentedPartialClass

generic partial 元素含有 `<summary>`（主要 partial），但缺少 `<typeparam>` 標籤。若為非主要 partial，請改用 `<content>` 取代 `<summary>`。

---

## SA1620 — GenericTypeParameterDocumentationMustMatchTypeParameters

`<typeparam>` 標籤與實際泛型型別參數不符（名稱錯誤、順序不對、多餘或遺漏）。

---

## SA1621 — GenericTypeParameterDocumentationMustDeclareParameterName

`<typeparam>` 標籤缺少 `name` 屬性或 `name` 屬性為空。

---

## SA1622 — GenericTypeParameterDocumentationMustHaveText

`<typeparam>` 標籤內容為空。

---

## SA1623 — PropertySummaryDocumentationMustMatchAccessors

屬性 `<summary>` 開頭文字與 accessor 組合不符。規則如下：

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
```

---

## SA1624 — PropertySummaryDocumentationMustOmitSetAccessorWithRestrictedAccess

屬性 `<summary>` 提到了 set accessor，但 set accessor 的存取層級比 get 更受限（如 `private set`），外部呼叫者無法存取，應從 summary 文字中省略。

判斷是否需要在 summary 中提及 set 的規則：
1. set 與 get 存取層級相同 → 需要提及
2. 屬性只能從 assembly 內部存取，且 set 為 internal → 需要提及
3. 屬性在 private class 內，set 為任何非 private → 需要提及（等效同層級）
4. set 為 protected 或 protected internal → 需要提及（子類別可存取）

其他情況（如 public 屬性 + private set）→ 省略 set，summary 只寫 "Gets"。

---

## SA1625 — ElementDocumentationMustNotBeCopiedAndPasted

文件中有兩個或以上相同內容的標籤，顯示文件是複製貼上的。例外：參數說明為 "The parameter is not used." 時允許重複。

**範例：**
```csharp
// 錯誤（兩個 param 說明相同）
/// <param name="firstName">Part of the name.</param>
/// <param name="lastName">Part of the name.</param>

// 正確
/// <param name="firstName">The first name to join.</param>
/// <param name="lastName">The last name to join.</param>
```

---

## SA1626 — SingleLineCommentsMustNotUseDocumentationStyleSlashes

單行註解以三個斜線（`///`）開頭。三斜線保留給 XML 文件標頭；一般單行註解應以 `//` 開頭，comment-out 程式碼用 `////`。

**範例：**
```csharp
// 正確的單行註解
// Join the names together.

////fullName = commented_out_code;

// 錯誤
/// Trim the name.
```

---

## SA1627 — DocumentationTextMustNotBeEmpty

文件標頭中有空的標籤（如 `<remarks></remarks>`）。

---

## SA1634 — FileHeaderMustShowCopyright

檔案標頭缺少 `<copyright>` 標籤。

---

## SA1635 — FileHeaderMustHaveCopyrightText

檔案標頭的 `<copyright>` 標籤內容為空。

---

## SA1636 — FileHeaderCopyrightTextMustMatch

檔案標頭中的著作權文字與 `stylecop.json` 中設定的 `copyrightText` 不符。需設定 `copyrightText` 才會啟用此規則。支援多種 comment 風格（`//`、`/* */`）。

---

## SA1637 — FileHeaderMustContainFileName

檔案標頭的 `<copyright>` 標籤缺少 `file` 屬性。

---

## SA1638 — FileHeaderFileNameDocumentationMustMatchFileName

檔案標頭中 `file` 屬性的值與實際檔案名稱不符。

---

## SA1640 — FileHeaderMustHaveValidCompanyText

檔案標頭的 `<copyright>` 標籤中 `company` 屬性為空。

---

## SA1641 — FileHeaderCompanyNameTextMustMatch

檔案標頭中 `company` 屬性的值與 `stylecop.json` 中設定的 `companyName` 不符。需設定 `companyName` 且 `xmlHeader` 為 true 才會啟用此規則。

---

## SA1642 — ConstructorSummaryDocumentationMustBeginWithStandardText

建構子的 `<summary>` 未以標準文字開頭。

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

finalizer（解構子）的 `<summary>` 未以標準文字開頭。標準格式：`Finalizes an instance of the <see cref="ClassName"/> class.`

**範例：**
```csharp
/// <summary>
/// Finalizes an instance of the <see cref="Customer"/> class.
/// </summary>
~Customer() { }
```

---

## SA1648 — InheritDocMustBeUsedWithInheritingClass

`<inheritdoc>` 用在沒有繼承任何 base class 或 interface 的元素上。例外：`<inheritdoc cref="..."/>` 明確指定來源時允許。

---

## SA1649 — FileNameMustMatchTypeName

檔案名稱與檔案中第一個宣告的型別名稱不符。泛型型別的檔案名稱格式依 `fileNamingConvention` 設定（如 `Class1{T}.cs` 或 ``Class1`1.cs``）。partial class 不適用此規則。

---

## SA1651 — DoNotUsePlaceholderElements

文件中包含 `<placeholder>` 標籤，應審閱並移除。

**範例：**
```csharp
// 錯誤
/// <summary>This method <placeholder>performs some operation</placeholder>.</summary>
public void SomeOperation() { }
```

---

*來源：[`documentation/DocumentationRules.md`](../documentation/DocumentationRules.md) 及對應各規則 `.md` 檔。*
