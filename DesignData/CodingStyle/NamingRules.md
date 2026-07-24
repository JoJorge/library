# Naming Rules (SA1300-)

強制規範成員、型別與變數的命名方式。

---

## SA1300 — ElementMustBeginWithUpperCaseLetter

下列元素名稱必須以大寫字母開頭：Namespace、Class、Enum、Enum 成員、Struct、Delegate、Event、Method、Property、Local function（含 static local function）。

`public`/`internal` 欄位及 `const` 欄位也必須大寫開頭；non-private readonly 欄位同樣需要大寫開頭。

例外：Win32/COM 互通（放在以 `NativeMethods` 結尾的類別中），或在 `allowedNamespaceComponents` 設定中列出的 namespace 元件。

---

## SA1302 — InterfaceNamesMustBeginWithI

介面名稱應以大寫字母 `I` 開頭。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

---

## SA1303 — ConstFieldNamesMustBeginWithUpperCaseLetter

`const` 欄位名稱應以大寫字母開頭。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

---

## SA1304 — NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter

非 private 的 `readonly` 欄位名稱應以大寫字母開頭。Public/protected readonly 欄位由 SA1307 負責報告；static readonly 欄位由 SA1311 負責；SA1304 主要處理 internal readonly 欄位。

---

## SA1305 — FieldNamesMustNotUseHungarianNotation

💡 此規則預設停用。

欄位或變數名稱應避免匈牙利命名法（以一或兩個小寫字母後接大寫字母為前綴，如 `strName`、`iCount`）。可透過 `stylecop.json` 設定允許的前綴。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

---

## SA1306 — FieldNamesMustBeginWithLowerCaseLetter

欄位名稱應以小寫字母開頭。例外：`const`、non-private readonly、static readonly、`public`/`internal` 欄位應以大寫開頭（由其他規則管理）。SA1306 只檢查欄位，不檢查參數或區域變數。例外：Win32/COM 互通。

---

## SA1307 — AccessibleFieldsMustBeginWithUpperCaseLetter

`public` 或 `internal` 欄位名稱應以大寫字母開頭。例外：Win32/COM 互通。

---

## SA1308 — VariableNamesMustNotBePrefixed

欄位名稱不應以 `m_`、`s_` 或 `t_` 開頭。應改用 `this.` 前綴來識別實例成員。例外：Win32/COM 互通。

---

## SX1309 — FieldNamesMustBeginWithUnderscore

private instance 欄位名稱應以底線開頭。

只檢查 private instance 欄位，忽略：`static`、`const`、public/internal 欄位、static `readonly` 欄位（視為常數）。C# 8 readonly struct 成員（方法、屬性等）不受此規則影響。模式比對的捨棄指示符（`_`）不是欄位，不受影響。

⚠️ 此規則與 SA1308（禁止 `m_` 前綴）和 SA1310（禁止底線）**不衝突**：SA1308/SA1310 不檢查 private instance 欄位的前導底線；SX1309 只要求 private instance 欄位以 `_` 開頭。

**範例：**
```csharp
// 正確
private int _count;
private readonly string _name;
private static int s_shared;          // static → SX1309 不管
private const int MaxRetry = 3;       // const → SX1309 不管
private static readonly int Default;  // static readonly → SX1309 不管

// 錯誤
private int count;
private readonly string name;
```

---

## SA1310 — FieldNamesMustNotContainUnderscore

欄位名稱不應包含底線。應使用 camelCase 命名，如 `customerPostCode` 而非 `customer_post_code`。例外：Win32/COM 互通。

---

## SA1311 — StaticReadonlyFieldsMustBeginWithUpperCaseLetter

`static readonly` 欄位名稱應以大寫字母開頭。此規則只針對 static readonly 欄位。

---

## SA1312 — VariableNamesMustBeginWithLowerCaseLetter

區域變數名稱應以小寫字母開頭。例外：Win32/COM 互通。

---

## SA1313 — ParameterNamesMustBeginWithLowerCaseLetter

參數名稱應以小寫字母開頭。例外：Lambda 的捨棄參數（`_`、`__`）、Positional record 的參數（會成為 public 屬性，允許 PascalCase，如 `record Person(string FirstName, int Age)`）、Win32/COM 互通。

---

## SA1314 — TypeParameterNamesMustBeginWithT

型別參數名稱應以大寫字母 `T` 開頭，如 `T`、`TKey`、`TValue`。

---

## SA1316 — TupleElementNamesShouldUseCorrectCasing

（C# 7.0+）Tuple 元素名稱應使用正確的大小寫。預設設定要求使用 PascalCase；可透過 `stylecop.json` 調整。

---
