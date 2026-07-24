# Ordering Rules (SA1200-)

強制規範 C# 程式碼內容的標準排列順序。

---

## SA1200 — UsingDirectivesMustBePlacedCorrectly

`using` 指示詞應放在 namespace 內部。預設要求放在 namespace 內部；可透過 `stylecop.json` 的 `usingDirectivesPlacement` 調整。

C# 9 top-level statements 中，`using` 必須在所有可執行程式碼之前，視為正確。C# 10 global using 永遠在 namespace 外，不觸發此規則。

將 `using` 放在 namespace 內的好處：
1. 避免 using-alias 與本地型別同名時產生混淆的編譯行為
2. 多 namespace 檔案中，可將 `using` 的作用域限縮在各自的 namespace

---

## SA1201 — ElementsMustAppearInTheCorrectOrder

程式碼元素應依照標準順序排列。

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

同型別的相鄰元素應依存取層級排序。存取層級由高到低的順序：`public` → `internal` → `protected internal` → `protected` → `private protected` → `private`。

靜態建構子與明確實作的介面成員視為 `public`。適用於 class、struct、interface（含 default interface members）及 record。

---

## SA1203 — ConstantsMustAppearBeforeFields

常數欄位（`const`）應排在非常數欄位之前。

---

## SA1204 — StaticElementsMustAppearBeforeInstanceElements

靜態元素應排在同型別的實例元素之前。適用於 class、struct 及 interface（含 C# 8 default interface members）。

---

## SA1205 — PartialElementsMustDeclareAccess

partial 元素應宣告存取修飾詞。C# 9 partial method 的存取修飾詞由編譯器強制，SA1205 不重複報告。

---

## SA1206 — DeclarationKeywordsMustFollowOrder

元素宣告中的關鍵字應符合標準順序：存取修飾詞（`public`、`private` 等）→ `static` → 其他關鍵字（`readonly`、`virtual`、`override`、`abstract` 等）。

---

## SA1207 — ProtectedMustComeBeforeInternal

`protected internal` 應寫為 `protected internal`（而非 `internal protected`），`private protected` 應寫為 `private protected`（而非 `protected private`）。

---

## SA1208 — SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives

`System` 命名空間的 `using` 指示詞應排在非 `System` 的 `using` 之前。C# 10 global using 與一般 using 分開分析。

---

## SA1209 — UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectives

using-alias 指示詞應排在所有一般 `using` 之後。C# 10 global using 分開分析。

---

## SA1210 — UsingDirectivesMustBeOrderedAlphabeticallyByNamespace

`using` 指示詞應依命名空間字母排序。`System` 命名空間例外，永遠排在最前面（見 SA1208）。C# 10 global using 分開分析。

---

## SA1211 — UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasName

using-alias 指示詞應依別名字母排序。C# 10 global using 分開分析。

---

## SA1212 — PropertyAccessorsMustFollowOrder

屬性或索引子中，`get` 存取子應排在 `set`/`init` 之前。C# 8 `readonly get` 同樣應排在 `set`/`init` 之前。

---

## SA1213 — EventAccessorsMustFollowOrder

事件中，`add` 存取子應排在 `remove` 之前。

---

## SA1214 — ReadonlyElementsMustAppearBeforeNonReadonlyElements

`readonly` 欄位應排在非 `readonly` 欄位之前。僅適用於欄位宣告；C# 8 readonly 成員（方法、屬性等）不受此規則約束。

---

## SA1216 — UsingStaticDirectivesMustBePlacedAtTheCorrectLocation

`using static` 指示詞應放在正確位置。正確順序：一般 `using` → `using static` → using-alias。C# 10 global using 分開分析。

---

## SA1217 — UsingStaticDirectivesMustBeOrderedAlphabetically

`using static` 指示詞應依完整型別名稱字母排序。C# 10 global using 分開分析。

---
