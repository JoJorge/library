# Maintainability Rules (SA1400-)

改善程式碼可維護性的規則。

---

## SA1119 — StatementMustNotUseUnnecessaryParenthesis

陳述式中含有不必要的括號。當括號圍繞的運算式位於陳述式根層級且不需要括號時觸發。C# 8.0 起，switch expression 或 pattern-matching expression 有時需要括號以確保正確性，此類括號不觸發此規則。

**範例：**
```csharp
// 正確
int x = 5 + b;
string y = this.Method().ToString();
return x.Value;

// 錯誤
int x = (5 + b);
string y = (this.Method()).ToString();
return (x.Value);
```

---

## SA1400 — AccessModifierMustBeDeclared

元素未明確宣告存取修飾詞。每個元素都應明確指定存取層級，避免讀者依賴隱含規則。例外：`interface` 成員隱含為 `public`，不需要明確宣告（含 C# 8 default interface members）。

**範例：**
```csharp
// 正確
public class MyClass { }
private int count;

// 錯誤
class MyClass { }
int count;
```

---

## SA1402 — FileMayOnlyContainASingleType

C# 檔案包含超過一個型別。每個型別應放在各自的檔案中。預設允許 delegate、enum、struct、interface 與 class 共存；同一 partial 型別的多個部分可以放在同一檔案；以 `file` 修飾符宣告的 file-local 型別不受此規則約束。

**範例：**
```csharp
// 錯誤（兩個獨立 class 在同一檔案）
public class ClassA { }
public class ClassB { }
```

---

## SA1403 — FileMayOnlyContainASingleNamespace

C# 檔案包含超過一個 namespace。每個檔案最多只能有一個 namespace。

**範例：**
```csharp
// 錯誤
namespace NamespaceA { }
namespace NamespaceB { }
```

---

## SA1404 — CodeAnalysisSuppressionMustHaveJustification

`SuppressMessage` 屬性未提供 `Justification`。每個抑制警告的地方都應說明原因。

**範例：**
```csharp
// 正確
[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Used during unit testing")]

// 錯誤
[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
```

---

## SA1405 — DebugAssertMustProvideMessageText

`Debug.Assert` 呼叫未提供描述訊息。應加入說明文字，方便觸發時識別問題。

**範例：**
```csharp
// 正確
Debug.Assert(value != null, "The value must not be null.");

// 錯誤
Debug.Assert(value != null);
```

---

## SA1406 — DebugFailMustProvideMessageText

`Debug.Fail` 呼叫未提供描述訊息。應加入說明文字。

**範例：**
```csharp
// 正確
Debug.Fail("The code should never reach this point.");

// 錯誤
Debug.Fail("");
```

---

## SA1408 — ConditionalExpressionsMustDeclarePrecedence

複雜條件運算式混用了 `&&`/`||`（或 C# 9 的 `and`/`or` pattern combinator）卻未加括號。應加括號明確宣告運算順序。

**範例：**
```csharp
// 正確
if ((x || y) && z && (a || b)) { }

// 錯誤
if (x || y && z && a || b) { }
```

---

## SA1410 — RemoveDelegateParenthesisWhenPossible

無參數的匿名方法仍保留了括號。無參數時括號是多餘的，應移除。

**範例：**
```csharp
// 正確
this.Method(delegate { return 2; });

// 錯誤
this.Method(delegate() { return 2; });
```

---

## SA1411 — AttributeConstructorMustNotUseUnnecessaryParenthesis

無參數的 Attribute 宣告仍保留了括號。無參數時括號是多餘的，應移除。

**範例：**
```csharp
// 正確
[Serializable]

// 錯誤
[Serializable()]
```

---

## SA1412 — StoreFilesAsUtf8

檔案編碼不是 UTF-8 with BOM。使用 UTF-8 with BOM 確保跨平台編譯行為一致，也是 Visual Studio 新建 C# 檔案的預設編碼。

---

## SA1413 — UseTrailingCommasInMultiLineInitializers

多行初始化式或清單的最後一個項目缺少結尾逗號。加入結尾逗號可減少日後新增/重排項目時需修改的行數，讓 code review 更精確、`git blame` 結果更準確。適用於 object initializer、enum、C# 8 switch expression arm、多行 property pattern。

**範例：**
```csharp
// 正確
var x = new Barnacle
{
    Age = 100,
    Height = 0.2M,
    Weight = 0.88M,
};

// 錯誤
var x = new Barnacle
{
    Age = 100,
    Height = 0.2M,
    Weight = 0.88M
};
```

---

## SA1414 — TupleTypesInSignaturesShouldHaveElementNames

（C# 7.0+）成員宣告中的 tuple 型別未提供元素名稱。

**範例：**
```csharp
// 正確
public (int ValueA, int ValueB) GetValues() { }

// 錯誤
public (int, int) GetValues() { }
```

---

*來源：[`documentation/MaintainabilityRules.md`](../documentation/MaintainabilityRules.md) 及對應各規則 `.md` 檔。*
