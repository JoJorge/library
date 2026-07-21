# Ordering Rules (SA1200-)

強制規範 C# 程式碼內容的標準排列順序。

---

## SA1200 — UsingDirectivesMustBePlacedCorrectly

`using` 指示詞放在 namespace 外部。預設要求放在 namespace 內部；可透過 `stylecop.json` 的 `usingDirectivesPlacement` 調整。

C# 9 top-level statements 中，`using` 必須在所有可執行程式碼之前，視為正確。C# 10 global using 永遠在 namespace 外，不觸發此規則。

將 `using` 放在 namespace 內的好處：
1. 避免 using-alias 與本地型別同名時產生混淆的編譯行為
2. 多 namespace 檔案中，可將 `using` 的作用域限縮在各自的 namespace

**範例：**
```csharp
// 正確
namespace Microsoft.Sample
{
    using System;
    using Guid = System.Guid;
}

// 錯誤
using System;
using Guid = System.Guid;

namespace Microsoft.Sample { }
```

---

## SA1201 — ElementsMustAppearInTheCorrectOrder

程式碼元素的排列順序不符合標準。

**檔案根層級 / namespace 內的順序：**
1. Extern Alias Directives
2. Using Directives
3. Namespaces
4. Delegates
5. Enums
6. Interfaces
7. Structs
8. Classes（C# 9 record 視同 class）

**class / struct / interface 內的順序：**
1. Enums
2. Structs
3. Classes
4. Fields
5. Constructors
6. Finalizers
7. Delegates
8. Events
9. Interfaces
10. Properties
11. Indexers
12. Methods

若實作介面需要混合不同型別的元素，建議使用 partial class 分離。

---

## SA1202 — ElementsMustBeOrderedByAccess

同型別的相鄰元素未依存取層級排序。存取層級由高到低的順序：

1. `public`
2. `internal`
3. `protected internal`
4. `protected`
5. `private protected`
6. `private`

靜態建構子與明確實作的介面成員視為 `public`。適用於 class、struct、interface（含 default interface members）及 record。

**範例：**
```csharp
// 正確
public void PublicMethod() { }
internal void InternalMethod() { }
private void PrivateMethod() { }

// 錯誤
private void PrivateMethod() { }
public void PublicMethod() { }
```

---

## SA1203 — ConstantsMustAppearBeforeFields

常數欄位（`const`）排在非常數欄位之後。常數應排在欄位之前，因兩者性質不同（命名規範、編譯行為等）。

**範例：**
```csharp
// 正確
private const int MaxCount = 10;
private int count;

// 錯誤
private int count;
private const int MaxCount = 10;
```

---

## SA1204 — StaticElementsMustAppearBeforeInstanceElements

靜態元素排在同型別的實例元素之後。所有靜態元素應排在對應型別的實例元素之前。適用於 class、struct 及 interface（含 C# 8 default interface members）。

**範例：**
```csharp
// 正確
public static void Helper() { }
public void Instance() { }

// 錯誤
public void Instance() { }
public static void Helper() { }
```

---

## SA1205 — PartialElementsMustDeclareAccess

partial 元素未宣告存取修飾詞。C# 9 partial method 的存取修飾詞由編譯器強制，SA1205 不重複報告。

**範例：**
```csharp
// 正確
public partial class MyClass { }

// 錯誤
partial class MyClass { }
```

---

## SA1206 — DeclarationKeywordsMustFollowOrder

元素宣告中的關鍵字順序不符合標準。正確順序：

1. 存取修飾詞（`public`、`private` 等）
2. `static`
3. 其他關鍵字（`readonly`、`virtual`、`override`、`abstract` 等）

**範例：**
```csharp
// 正確
public static readonly int Value = 1;

// 錯誤
static public readonly int Value = 1;
```

---

## SA1207 — ProtectedMustComeBeforeInternal

`protected internal` 寫成了 `internal protected`，或 `private protected` 寫成了 `protected private`。

**範例：**
```csharp
// 正確
protected internal void Method() { }
private protected void Method2() { }

// 錯誤
internal protected void Method() { }
protected private void Method2() { }
```

---

## SA1208 — SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives

`System` 命名空間的 `using` 指示詞排在非 `System` 的 `using` 之後。C# 10 global using 與一般 using 分開分析。

**範例：**
```csharp
// 正確
using System;
using System.Collections.Generic;
using MyCompany.Utilities;

// 錯誤
using MyCompany.Utilities;
using System;
```

---

## SA1209 — UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectives

using-alias 指示詞排在一般 `using` 之前。using-alias 應排在所有一般 `using` 之後。C# 10 global using 分開分析。

**範例：**
```csharp
// 正確
using System;
using MyCompany.Utilities;
using Guid = System.Guid;

// 錯誤
using Guid = System.Guid;
using System;
```

---

## SA1210 — UsingDirectivesMustBeOrderedAlphabeticallyByNamespace

`using` 指示詞未依命名空間字母排序。`System` 命名空間例外，永遠排在最前面（見 SA1208）。C# 10 global using 分開分析。

**範例：**
```csharp
// 正確
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MyCompany.Utilities;

// 錯誤
using MyCompany.Utilities;
using Microsoft.Extensions.Logging;
using System;
```

---

## SA1211 — UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasName

using-alias 指示詞未依別名字母排序。C# 10 global using 分開分析。

**範例：**
```csharp
// 正確
using Action = System.Action;
using Guid = System.Guid;

// 錯誤
using Guid = System.Guid;
using Action = System.Action;
```

---

## SA1212 — PropertyAccessorsMustFollowOrder

屬性或索引子中，`get` 存取子排在 `set`/`init` 之後。C# 8 `readonly get` 同樣應排在 `set`/`init` 之前。

**範例：**
```csharp
// 正確
public string Name
{
    get { return this.name; }
    set { this.name = value; }
}

// 錯誤
public string Name
{
    set { this.name = value; }
    get { return this.name; }
}
```

---

## SA1213 — EventAccessorsMustFollowOrder

事件中，`add` 存取子排在 `remove` 之後。`add` 應排在 `remove` 之前。

**範例：**
```csharp
// 正確
public event EventHandler NameChanged
{
    add { this.nameChanged += value; }
    remove { this.nameChanged -= value; }
}

// 錯誤
public event EventHandler NameChanged
{
    remove { this.nameChanged -= value; }
    add { this.nameChanged += value; }
}
```

---

## SA1214 — ReadonlyElementsMustAppearBeforeNonReadonlyElements

`readonly` 欄位排在非 `readonly` 欄位之後。僅適用於欄位宣告；C# 8 readonly 成員（方法、屬性等）不受此規則約束。

**範例：**
```csharp
// 正確
private readonly int id;
private int count;

// 錯誤
private int count;
private readonly int id;
```

---

## SA1216 — UsingStaticDirectivesMustBePlacedAtTheCorrectLocation

`using static` 指示詞放錯位置。正確順序：一般 `using` → `using static` → using-alias。C# 10 global using 分開分析。

**範例：**
```csharp
// 正確
using System;
using static System.Math;
using Env = System.Environment;

// 錯誤
using static System.Math;
using System;
```

---

## SA1217 — UsingStaticDirectivesMustBeOrderedAlphabetically

`using static` 指示詞未依完整型別名稱字母排序。C# 10 global using 分開分析。

**範例：**
```csharp
// 正確
using static System.Math;
using static System.String;

// 錯誤
using static System.String;
using static System.Math;
```

---

*來源：[`documentation/OrderingRules.md`](../documentation/OrderingRules.md) 及對應各規則 `.md` 檔。*
