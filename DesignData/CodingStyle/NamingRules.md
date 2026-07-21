# Naming Rules (SA1300-)

強制規範成員、型別與變數的命名方式。

---

## SA1300 — ElementMustBeginWithUpperCaseLetter

下列元素名稱必須以大寫字母開頭：Namespace、Class、Enum、Enum 成員、Struct、Delegate、Event、Method、Property、Local function（含 static local function）。

`public`/`internal` 欄位及 `const` 欄位也必須大寫開頭；non-private readonly 欄位同樣需要大寫開頭。

例外：Win32/COM 互通（放在以 `NativeMethods` 結尾的類別中），或在 `allowedNamespaceComponents` 設定中列出的 namespace 元件。

**範例：**
```csharp
// 正確
public class CustomerOrder { }
public void ProcessOrder() { }
public const int MaxCount = 10;

// 錯誤
public class customerOrder { }
public void processOrder() { }
```

---

## SA1302 — InterfaceNamesMustBeginWithI

介面名稱未以大寫字母 `I` 開頭。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

**範例：**
```csharp
// 正確
public interface ICustomer { }

// 錯誤
public interface Customer { }
```

---

## SA1303 — ConstFieldNamesMustBeginWithUpperCaseLetter

`const` 欄位名稱未以大寫字母開頭。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

**範例：**
```csharp
// 正確
private const int MaxRetryCount = 3;

// 錯誤
private const int maxRetryCount = 3;
```

---

## SA1304 — NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter

非 private 的 `readonly` 欄位名稱未以大寫字母開頭。Public/protected readonly 欄位由 SA1307 負責報告；static readonly 欄位由 SA1311 負責；SA1304 主要處理 internal readonly 欄位。

**範例：**
```csharp
// 正確
internal readonly int MaxSize = 100;

// 錯誤
internal readonly int maxSize = 100;
```

---

## SA1305 — FieldNamesMustNotUseHungarianNotation

💡 此規則預設停用。

欄位或變數名稱使用了匈牙利命名法（以一或兩個小寫字母後接大寫字母為前綴，如 `strName`、`iCount`）。可透過 `stylecop.json` 設定允許的前綴，例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

```json
{
  "settings": {
    "namingRules": {
      "allowedHungarianPrefixes": [ "on", "my" ]
    }
  }
}
```

**範例：**
```csharp
// 正確
private string customerName;

// 錯誤
private string strCustomerName;
private int iCount;
```

---

## SA1306 — FieldNamesMustBeginWithLowerCaseLetter

欄位名稱未以小寫字母開頭。例外：`const`、non-private readonly、static readonly、`public`/`internal` 欄位應以大寫開頭（由其他規則管理）。注意：SA1306 只檢查欄位，不檢查參數或區域變數。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

**範例：**
```csharp
// 正確
private int count;
private string name;

// 錯誤
private int Count;
private string Name;
```

---

## SA1307 — AccessibleFieldsMustBeginWithUpperCaseLetter

`public` 或 `internal` 欄位名稱未以大寫字母開頭。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

**範例：**
```csharp
// 正確
public int Count;
internal string Name;

// 錯誤
public int count;
internal string name;
```

---

## SA1308 — VariableNamesMustNotBePrefixed

欄位名稱以 `m_`、`s_` 或 `t_` 開頭。應改用 `this.` 前綴來識別實例成員。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

**範例：**
```csharp
// 正確
private int count;

// 錯誤
private int m_count;
private int s_count;
```

---

## SX1309 — FieldNamesMustBeginWithUnderscore

private instance 欄位名稱未以底線開頭。

只檢查 private instance 欄位，忽略：`static`、`const`、public/internal 欄位、static `readonly` 欄位（視為常數）。C# 8 readonly struct 成員（方法、屬性等）不受此規則影響。模式比對的捨棄指示符（`_`）不是欄位，不受影響。

**範例：**
```csharp
// 正確
private int _count;
private readonly string _name;

// 錯誤
private int count;
private readonly string name;
```

---

## SA1310 — FieldNamesMustNotContainUnderscore

欄位名稱包含底線。應使用 camelCase 命名，如 `customerPostCode` 而非 `customer_post_code`。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

**範例：**
```csharp
// 正確
private int customerPostCode;

// 錯誤
private int customer_post_code;
```

---

## SA1311 — StaticReadonlyFieldsMustBeginWithUpperCaseLetter

`static readonly` 欄位名稱未以大寫字母開頭。此規則只針對 static readonly 欄位，不影響 readonly 方法/屬性/索引子等。

**範例：**
```csharp
// 正確
private static readonly int MaxSize = 100;

// 錯誤
private static readonly int maxSize = 100;
```

---

## SA1312 — VariableNamesMustBeginWithLowerCaseLetter

區域變數名稱未以小寫字母開頭。例外：Win32/COM 互通（放在 `NativeMethods` 類別中）。

**範例：**
```csharp
// 正確
int count = 0;
string name = "test";

// 錯誤
int Count = 0;
string Name = "test";
```

---

## SA1313 — ParameterNamesMustBeginWithLowerCaseLetter

參數名稱未以小寫字母開頭。例外：
- Lambda 的捨棄參數（`_`、`__`）
- Positional record 的參數（會成為 public 屬性，允許 PascalCase）
- Win32/COM 互通（放在 `NativeMethods` 類別中）

**範例：**
```csharp
// 正確
public void Method(string firstName, int count) { }
public record Person(string FirstName, int Age); // positional record 例外

// 錯誤
public void Method(string FirstName, int Count) { }
```

---

## SA1314 — TypeParameterNamesMustBeginWithT

型別參數名稱未以大寫字母 `T` 開頭。型別參數應以 `T` 開頭，如 `T`、`TKey`、`TValue`。

**範例：**
```csharp
// 正確
public class Repository<T> { }
public class Dictionary<TKey, TValue> { }

// 錯誤
public class Repository<Entity> { }
public class Dictionary<Key, Value> { }
```

---

## SA1316 — TupleElementNamesShouldUseCorrectCasing

（C# 7.0+）Tuple 元素名稱的大小寫不正確。預設設定要求使用 PascalCase；可透過 `stylecop.json` 調整。

**範例（預設 PascalCase）：**
```csharp
// 正確
public (int ValueA, int ValueB) GetValues() { }

// 錯誤
public (int valueA, int valueB) GetValues() { }
```

---

*來源：[`documentation/NamingRules.md`](../documentation/NamingRules.md) 及對應各規則 `.md` 檔。*
