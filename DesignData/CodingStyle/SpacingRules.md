# Spacing Rules (SA1000-)

強制規範 C# 程式碼中關鍵字與符號周圍的空白格式。

---

## SA1000 — KeywordsMustBeSpacedCorrectly

C# 關鍵字前後的空白不正確。

以下關鍵字後面必須接一個空格：`and`, `await`, `case`, `catch`, `fixed`, `for`, `foreach`, `from`, `group`, `if`, `in`, `is`, `into`, `join`, `let`, `lock`, `not`, `orderby`, `or`, `out`, `ref`, `return`, `select`, `switch`, `using`, `var`, `where`, `while`, `yield`。

以下關鍵字後面不可有空格：`checked`, `default`, `nameof`, `sizeof`, `typeof`, `unchecked`（`default` 用作預設字面運算式時例外）。

`new` / `stackalloc` 後必須有空格，但下列情況除外：隱含型別陣列（`new[]`）、target-typed new（`new()`）、泛型類型限制（`new()`）。

`throw` 後必須有空格，但 re-throw（`throw;`）除外。

C# 9 函式指標語法中，`delegate` 後不可有空格在 `*` 之前，`unmanaged` 後不可有空格在呼叫慣例清單之前。例如：`delegate*<int, void>`、`delegate* unmanaged[Cdecl]<int, void>`。

**範例：**
```csharp
// 正確
if (x) { }
var y = new[] { 1, 2 };
throw new Exception();

// 錯誤
if(x) { }
var y = new [] { 1, 2 };
```

---

## SA1001 — CommasMustBeSpacedCorrectly

逗號前後空白不正確。逗號後應接一個空格，下列情況例外：行尾、`typeof` 中開放泛型型別、字串插補對齊（`$"{x,3}"`）、條件前置處理器指令後。逗號前不可有空格，且不可出現在行首。

**範例：**
```csharp
// 正確
Method(a, b, c);

// 錯誤
Method(a ,b,c);
```

---

## SA1002 — SemicolonsMustBeSpacedCorrectly

分號前後空白不正確。分號後應接一個空格，下列情況例外：行尾、後接右括號。分號前不可有空格，除非是行首。

**範例：**
```csharp
// 正確
for (int i = 0; i < 10; i++) { }

// 錯誤
for (int i = 0 ; i < 10 ; i++) { }
```

---

## SA1003 — SymbolsMustBeSpacedCorrectly

運算子符號前後空白不正確。

二元運算子（算術、指派、條件、邏輯、關係、位移、lambda `=>`）兩側各需一個空格。一元運算子前需一個空格，後不可有空格。符號緊接括號或方括號時，符號與括號間不可有空格。

C# 8 index-from-end 運算子（`^`）視為一元，直接貼著運算元；range 運算子（`..`）視為二元但兩側**不**加空格。

**範例：**
```csharp
// 正確
int x = 4 + y;
bool b = !value;
if (!value) { }
var last = values[^1];
Range r = 1..4;

// 錯誤
int x = 4+y;
bool b = ! value;
```

---

## SA1004 — DocumentationLinesMustBeginWithSingleSpace

文件標頭（`///`）行內容前未以單一空格開頭。

**範例：**
```csharp
// 正確
/// <summary>
/// The summary text.
/// </summary>

// 錯誤
///<summary>
///The summary text.
///</summary>
```

---

## SA1005 — SingleLineCommentsMustBeginWithSingleSpace

單行註解（`//`）內容前未以單一空格開頭。例外：以 `////` 開頭的 comment-out 程式碼可省略空格。

**範例：**
```csharp
// 正確
// A comment.
////int x = 2;

// 錯誤
//A comment.
//   A comment.
```

---

## SA1006 — PreprocessorKeywordsMustNotBePrecededBySpace

前置處理器關鍵字與 `#` 之間有空格。

**範例：**
```csharp
// 正確
#if DEBUG

// 錯誤
# if DEBUG
```

---

## SA1007 — OperatorKeywordMustBeFollowedBySpace

運算子多載方法中的 `operator` 關鍵字後未接空格。

**範例：**
```csharp
// 正確
public MyClass operator +(MyClass a, MyClass b) { }

// 錯誤
public MyClass operator+(MyClass a, MyClass b) { }
```

---

## SA1008 — OpeningParenthesisMustBeSpacedCorrectly

左括號前後空白不正確。左括號前通常不可有空格，除非是行首、後接 `if`/`while`/`for` 等關鍵字，或在運算式中跟在運算子後面。左括號後不可有空格，除非是行尾。

**範例：**
```csharp
// 正確
if (x) { }
Method(a, b);

// 錯誤
if( x) { }
Method( a, b);
```

---

## SA1009 — ClosingParenthesisMustBeSpacedCorrectly

右括號前後空白不正確。右括號前不可有空格。右括號後通常應接一個空格，例外：cast 後、後接特定運算子（`+`、`-`、`:`）。右括號後若有空格，下一個非空白字元不可是括號、方括號、分號或逗號。

**範例：**
```csharp
// 正確
if (x) { }
int y = (int)x;

// 錯誤
if (x ) { }
```

---

## SA1010 — OpeningSquareBracketsMustBeSpacedCorrectly

左方括號前後有空白。左方括號前後均不可有空格，除非是行首或行尾。

**範例：**
```csharp
// 正確
int[] arr = new int[10];
x = arr[0];

// 錯誤
int[] arr = new int[ 10];
x = arr [0];
```

---

## SA1011 — ClosingSquareBracketsMustBeSpacedCorrectly

右方括號前後空白不正確。右方括號前不可有空格，除非是行首。右方括號後應有空格，下列情況除外：行尾、後接右括號或左括號、後接逗號或分號、字串插補對齊/格式化、特定運算子、nullable 標註（`[]?`）、null-forgiving 運算子（`[0]!`）、函式指標呼叫慣例（`[Cdecl]<`）。

**範例：**
```csharp
// 正確
var x = arr[0];
string[]? items = null;

// 錯誤
var x = arr[ 0 ];
```

---

## SA1012 — OpeningBracesMustBeSpacedCorrectly

左大括號前後空白不正確。左大括號前應有一個空格，除非是行首或前接左括號（此時不加空格）。左大括號後應有一個空格，除非是行尾。

**範例：**
```csharp
// 正確
if (x) { }
var d = new Dictionary<int, int> { };

// 錯誤
if (x){ }
if (x) {x = 1;}
```

---

## SA1013 — ClosingBracesMustBeSpacedCorrectly

右大括號前後空白不正確。右大括號後應有一個空格，除非是行尾、後接右括號/逗號/分號，或後接 null-forgiving 運算子（`}!`）。右大括號前應有一個空格，除非是行首。

**範例：**
```csharp
// 正確
if (x) { return; }
new Foo { Value = null }!.Method();

// 錯誤
if (x) {return;}
```

---

## SA1014 — OpeningGenericBracketsMustBeSpacedCorrectly

泛型左角括號（`<`）前後有空白。泛型左角括號前後均不可有空格，除非是行首或行尾。同樣適用於 C# 9 函式指標參數清單的 `<`（如 `delegate*<int, void>`）。

**範例：**
```csharp
// 正確
List<int> list;

// 錯誤
List< int> list;
List <int> list;
```

---

## SA1015 — ClosingGenericBracketsMustBeSpacedCorrectly

泛型右角括號（`>`）前後空白不正確。右角括號前不可有空格，除非是行首。右角括號後應接左括號、右括號、另一個右角括號、nullable 符號、行尾，或單一空格（但不可是空格後接左括號）。

**範例：**
```csharp
// 正確
List<int> list;
List<string?>? items;

// 錯誤
List<int > list;
```

---

## SA1016 — OpeningAttributeBracketsMustBeSpacedCorrectly

屬性左方括號（`[`）後有空白。屬性左方括號後不可有空格，除非是行尾。

**範例：**
```csharp
// 正確
[Obsolete]

// 錯誤
[ Obsolete]
```

---

## SA1017 — ClosingAttributeBracketsMustBeSpacedCorrectly

屬性右方括號（`]`）前有空白。屬性右方括號前不可有空格，除非是行首。

**範例：**
```csharp
// 正確
[Obsolete]

// 錯誤
[Obsolete ]
```

---

## SA1018 — NullableTypeSymbolsMustNotBePrecededBySpace

nullable 型別符號（`?`）前有空格。nullable 型別符號前不可有空格，除非是行首。適用於 nullable value type 與 C# 8 nullable reference type。

**範例：**
```csharp
// 正確
string? value;
List<string?>? list;

// 錯誤
string ? value;
List<string?> ? list;
```

---

## SA1019 — MemberAccessSymbolsMustBeSpacedCorrectly

成員存取符號（`.`、`?.`）兩側有空白。成員存取符號兩側均不可有空格，除非是行首。也適用於緊接 null-forgiving 運算子後的存取，如 `value!.Property`。

**範例：**
```csharp
// 正確
x.Method();
value?.Property;

// 錯誤
x .Method();
value ?. Property;
```

---

## SA1020 — IncrementDecrementSymbolsMustBeSpacedCorrectly

遞增/遞減符號（`++`、`--`）與運算元之間有空白。遞增/遞減符號與運算元之間不可有空格。

**範例：**
```csharp
// 正確
i++;
--j;

// 錯誤
i ++;
-- j;
```

---

## SA1021 — NegativeSignsMustBeSpacedCorrectly

負號（一元 `-`）前後空白不正確。負號前應有一個空格，除非前接左方括號、括號、插補字串大括號、行首，或在字串插補對齊中。負號後不可有空格，且不可是行尾。

**範例：**
```csharp
// 正確
int x = -y;
int z = a + -b;

// 錯誤
int x = - y;
```

---

## SA1022 — PositiveSignsMustBeSpacedCorrectly

正號（一元 `+`）前後空白不正確。規則與 SA1021 相同，方向為正號。

**範例：**
```csharp
// 正確
int x = +y;

// 錯誤
int x = + y;
```

---

## SA1023 — DereferenceAndAccessOfMustBeSpacedCorrectly

解參考（`*`）或取址（`&`）符號前後空白不正確。

型別宣告中使用時：符號後應有空格（除非是行尾、後接 `[` 或 `(`），符號前不可有空格，且不可是行首。例：`object* x = null;`。

型別宣告外使用時（如運算式）：符號前應有空格（除非是行首、前接 `[`、`(` 或同類符號），符號後不可有空格，且不可是行尾。例：`y = *x;`。

**範例：**
```csharp
// 正確（型別宣告）
object* x = null;

// 正確（運算式）
y = *x;
z = &x;
```

---

## SA1024 — ColonsMustBeSpacedCorrectly

冒號前後空白不正確，規則依冒號用途而異：

- 元素宣告（繼承、建構子呼叫）：兩側各一個空格，例：`class Foo : Bar`、`public Foo(int x) : base(x)`。
- 標籤或 `case`：後接空格或行尾，前不可有空格，例：`case 2:`。
- 字串插補格式化（`$"{x:N}"`）：前不可有空格。
- 屬性模式（`{ Prop: value }`）：前不可有空格，後接一個空格。
- 條件運算式（`y ? 2 : 3`）：兩側各一個空格。

**範例：**
```csharp
public class Foo : Bar
{
    public Foo(int x) : base(x) { }
}

int x = y ? 2 : 3;
var s = $"{x:N}";
```

---

## SA1025 — CodeMustNotContainMultipleWhitespaceInARow

程式碼中連續出現多個空白字元。行首/行尾，或逗號、分號後方除外。

**範例：**
```csharp
// 正確
int x = 1;

// 錯誤
int  x  =  1;
```

---

## SA1026 — CodeMustNotContainSpaceAfterNewKeywordInImplicitlyTypedArrayAllocation

隱含型別陣列配置中，`new` 或 `stackalloc` 關鍵字與左方括號之間有空格。

**範例：**
```csharp
// 正確
var a = new[] { 1, 10, 100 };
Span<int> b = stackalloc[] { 1, 2, 3 };

// 錯誤
var a = new [] { 1, 10, 100 };
```

---

## SA1027 — UseTabsCorrectly

程式碼中 tab 或空格的使用與專案設定不一致。預設不允許 tab 字元（可透過 `stylecop.json` 調整）。無論設定為何，非行首位置的 tab 一律違規。例外：字串字面值、字元字面值、comment-out 程式碼（`////`）、停用的程式碼區塊。

**範例（預設設定，不允許 tab）：**
```csharp
// 正確（使用空格縮排）
public void Method()
{
    int x = 1;
}

// 錯誤（使用 tab 縮排）
public void Method()
{
	int x = 1;
}
```

---

## SA1028 — CodeMustNotContainTrailingWhitespace

行尾有多餘的空白字元（空格、tab 等）。適用於一般程式碼、行/區塊/文件註解、前置處理器指令；不適用於逐字字串字面值（`@""`）及停用的程式碼區塊。

**範例：**
```csharp
// 正確
int x = 1;

// 錯誤（行尾有空格）
int x = 1;   
```

---

*來源：[`documentation/SpacingRules.md`](../documentation/SpacingRules.md) 及對應各規則 `.md` 檔。*
