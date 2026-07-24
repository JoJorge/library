# Maintainability Rules (SA1400-)

改善程式碼可維護性的規則。

---

## SA1119 — StatementMustNotUseUnnecessaryParenthesis

陳述式應避免不必要的括號。當括號圍繞的運算式位於陳述式根層級且不需要括號時觸發。C# 8.0 起，switch expression 或 pattern-matching expression 有時需要括號以確保正確性，此類括號不觸發此規則。

---

## SA1400 — AccessModifierMustBeDeclared

元素應明確宣告存取修飾詞。每個元素都應明確指定存取層級，避免讀者依賴隱含規則。例外：`interface` 成員隱含為 `public`，不需要明確宣告（含 C# 8 default interface members）。

---

## SA1402 — FileMayOnlyContainASingleType

C# 檔案應只包含一個型別。每個型別應放在各自的檔案中。預設允許 delegate、enum、struct、interface 與 class 共存；同一 partial 型別的多個部分可以放在同一檔案；以 `file` 修飾符宣告的 file-local 型別不受此規則約束。

---

## SA1403 — FileMayOnlyContainASingleNamespace

C# 檔案應只包含一個 namespace。

---

## SA1404 — CodeAnalysisSuppressionMustHaveJustification

`SuppressMessage` 屬性應提供 `Justification`。每個抑制警告的地方都應說明原因。

---

## SA1405 — DebugAssertMustProvideMessageText

`Debug.Assert` 呼叫應提供描述訊息。應加入說明文字，方便觸發時識別問題。

---

## SA1406 — DebugFailMustProvideMessageText

`Debug.Fail` 呼叫應提供描述訊息。

---

## SA1408 — ConditionalExpressionsMustDeclarePrecedence

複雜條件運算式混用 `&&`/`||`（或 C# 9 的 `and`/`or` pattern combinator）時應加括號明確宣告運算順序。

---

## SA1410 — RemoveDelegateParenthesisWhenPossible

無參數的匿名方法應移除括號。

---

## SA1411 — AttributeConstructorMustNotUseUnnecessaryParenthesis

無參數的 Attribute 宣告應移除括號。

---

## SA1412 — StoreFilesAsUtf8

檔案編碼應使用 UTF-8 with BOM。使用 UTF-8 with BOM 確保跨平台編譯行為一致，也是 Visual Studio 新建 C# 檔案的預設編碼。

---

## SA1413 — UseTrailingCommasInMultiLineInitializers

多行初始化式或清單的最後一個項目應加入結尾逗號。加入結尾逗號可減少日後新增/重排項目時需修改的行數，讓 code review 更精確、`git blame` 結果更準確。

適用範圍：object initializer、collection initializer、enum 成員、C# 8 switch expression arm、多行 property pattern。

**範例：**
```csharp
// 正確（注意最後一項的逗號）
var x = new Barnacle
{
    Age = 100,
    Weight = 0.88M,  // ← trailing comma
};

var result = value switch
{
    1 => "one",
    2 => "two",      // ← trailing comma
};
```

---

## SA1414 — TupleTypesInSignaturesShouldHaveElementNames

（C# 7.0+）成員宣告中的 tuple 型別應提供元素名稱，如 `(int ValueA, int ValueB)` 而非 `(int, int)`。

---
