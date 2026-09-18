# 開發流程指南（Development Workflow Guide）

本文件說明Unity專案在 Kiro 環境下的開發流程。所有功能開發都採用 **Kiro Spec 工作流程**，並遵循 `.kiro/steering/` 下的專案規範。新進開發者請先閱讀本文件，再開始接手任何功能。

---

## Overview

本專案使用 Kiro 的 **Spec（規格）工作流程** 來開發每個功能。一個 Spec 就是一個功能的完整文件與實作記錄，依序經過三個階段產出三份文件：

1. **Requirements（需求）** — 定義「要做什麼」
2. **Design（設計）** — 定義「怎麼做」
3. **Tasks（任務）** — 拆解成「可逐步實作與勾選的步驟」

每個 Spec 存放於 `.kiro/specs/<feature-name>/`，包含 `requirements.md`、`design.md`、`tasks.md` 三份文件。現有範例可參考 `singleton-pattern`、`event-system`、`fsm-system`、`input-system`。

開發的核心原則是**先對齊需求與設計，再動手實作**，避免寫出方向錯誤的程式碼。

---

## 專案環境速覽

> **給新專案開發者：** 以下為專案環境設定範本，請依你的專案實際狀況填寫，並將對應內容同步到 `.kiro/steering/unity-env.md`（讓 Kiro 在設計與實作時遵循）。填寫完成後可移除本引言與 `<...>` 佔位符。

進行任何設計與實作前，請先確立並記錄以下環境設定：

| 項目 | 內容（請自行填入） |
|------|------|
| Unity 版本 | `<例：Unity 6000.x.xf1>` |
| Input | `<例：New Input System / 舊版 Input Manager>` |
| UI | `<例：UI Toolkit / uGUI>` |
| 測試框架 | `<例：NUnit + FsCheck 3.X，透過 Unity Test Runner 執行>` |
| 其他套件 | `<例：UniTask、Addressable、DOTween、Odin Inspector>` |
| 語言 / 平台 | `<例：C# / Unity 相容的 C# 版本>` |

依專案需求補充的重點提醒（範例，請調整為你的專案規則）：

- 測試框架的使用限制與命名空間慣例（例如若使用 FsCheck，是否直接呼叫 API、對應的 C# 命名空間為何）。
- 測試類別與功能類別的 assembly 拆分方式，以配合測試執行器。
- 套件安裝流程：誰負責安裝、開發者是否可自行安裝新套件。
- 其他專案特有的平台 / 版本 / 相依限制。

---

## Spec 三階段工作流程

### 階段一：Requirements（需求）

**目標：** 釐清功能要解決的問題與驗收標準，不談實作細節。

文件位置：`.kiro/specs/<feature-name>/requirements.md`

必要章節（標題用英文，內容用繁體中文）：

- `## Introduction` — 功能簡介與範圍
- `## Glossary` — 專有名詞定義
- `## Requirements` — 逐條需求
  - `### Requirement N: [英文標題]`
  - `#### Acceptance Criteria` — 驗收標準

**驗收標準採用 EARS 語法**，關鍵字（`WHEN`、`WHERE`、`WHILE`、`IF`、`THEN`、`THE`、`SHALL`）保持英文，其餘用中文。例如：

> WHEN 單例第一次被存取時，THE 延遲初始化單例 SHALL 建立實例

每條驗收標準都要能被後續的測試對應驗證，因此撰寫時要具體、可觀察、可測。

**完成準則：** 需求涵蓋主要情境與邊界條件，並取得需求確認後再進入設計階段。

---

### 階段二：Design（設計）

**目標：** 定義技術方案、架構、元件介面、資料模型與正確性屬性。

文件位置：`.kiro/specs/<feature-name>/design.md`

設計文件的格式規範較嚴格（詳見 `.kiro/steering/spec-design-format.md`），重點如下：

- 文件標題（`# ...`）後、`## Overview` 前，必須有 `## 目錄`（Table of Contents），列出所有 `##` 與 `###` 標題的錨點連結，`###` 縮排兩格，並以 `---` 與 `## Overview` 分隔。
- 每個 `##` 段落（`## 目錄` 除外）結尾要放返回頂端連結，格式為 `[↩](#目錄)` 緊接一行 `---`。
- 標準章節：`## Overview`、`## Architecture`、`## Components and Interfaces`、`## Data Models`、`## Correctness Properties`、`## Error Handling`、`## Testing Strategy`。

**Correctness Properties（正確性屬性）** 是設計的重點，用於驅動屬性導向測試（PBT）：

- 每個屬性用 `### Property N: <English title>`（可加中文副標，以破折號分隔）。
- 屬性描述後緊接一行 `**Validates: Requirements X.X, Y.Y**`，標明對應的需求編號，建立需求到測試的追溯性。

**UML / 架構圖** 使用 Mermaid（`classDiagram`）。依總行數決定放置方式：

- 少於 999 行：可放在 `DesignData/` 下的單一 `.mmd` 檔，於 `design.md` 以相對路徑引用；或直接內嵌在 Architecture 段落。
- 大於等於 1000 行：拆成多個檔案放在 `DesignData/<FeatureName>/`，以 `_2`、`_3` 數字後綴命名，每個檔案都要是可獨立編譯的 Mermaid 圖。

圖的區塊順序、成員語法（attribute-first、return-type-last）、修改標記（新增 `❇️`、修改 `🔄`、移除 `❌`，類別層級 `NewClass`/`ModifiedClass`/`RemovedClass` 樣式）等細節，請對照 steering 規範撰寫。

**命名慣例：**

- 頂層 manager（通常是 singleton）命名為 `XxxManager`。
- Manager / controller 的初始化與釋放 API 命名為 `Init` 與 `Release`。

**完成準則：** 設計涵蓋所有需求，正確性屬性對應到具體需求，並取得設計確認後再進入任務拆解。

---

### 階段三：Tasks（任務）

**目標：** 把設計拆成可逐步實作、可勾選、可追溯的任務清單。

文件位置：`.kiro/specs/<feature-name>/tasks.md`

任務清單特性：

- 以核取方塊列出任務（`- [ ]` 未完成、`- [x]` 完成、`- [ ]*` 標示可選 / MVP 可略過）。
- 每個任務標註對應需求，例如 `_Requirements: 1.1, 2.3_`，維持追溯性。
- 適當設置 **checkpoint** 任務做增量驗證（例如「確認所有測試通過」）。
- 可附 **Task Dependency Graph**（JSON 格式的 waves），描述任務可平行執行的批次。

**測試任務處理原則**（詳見 `.kiro/steering/task-test-handling.md`）：

- 將「測試任務」與「測試實作任務」拆開，避免測試失敗阻擋實作進度。
- 為控制任務數量，可把測試任務併入 checkpoint 任務。
- 測試方法**預設不加** `Category` 屬性，除非明確要求。
- 需要新套件時**不要自行安裝**，請通知負責人。

**逐一實作與勾選：** 依任務順序實作，完成一項就勾選一項。若使用者要求跳過某步驟，該步驟**不可**標記為完成。

---

## 最終 Checkpoint 應產出的文件

在最後一個 checkpoint 任務，需依序處理以下兩件事（皆為輸出/報告性質，**不得**因此調整或新增任何測試或功能程式碼）：

### 1. Requirement Test Coverage（需求測試覆蓋文件）

- 檔名固定為 `test-coverage.md`，放在與該 Spec 的 `design.md` 同一資料夾。
- 內容呈現整體需求覆蓋率，以及每條需求對應的測試方法（PBT 用 `Property N`，單元測試用反引號包住的方法名，未覆蓋用 `-`）。
- 格式固定，包含「Overall Coverage」與「Coverage by Requirement」表格（欄位：Requirement、Description、Test Methods、Status）。此任務僅列印文件供參考。

### 2. Mutation Testing（變異測試，使用 Stryker）

- 使用 `dotnet stryker` 執行變異測試，結果交由負責人決定後續處理。此任務**不得**調整或新增任何測試 / 功能程式碼。
- **前置條件：** 只有當受測功能**不相依 `UnityEngine`** 時才執行。Stryker 建置的是純 .NET 專案，無法使用 `UnityEngine` 型別。若功能相依 `UnityEngine`（出現 `using UnityEngine;` 或任何 `UnityEngine.*`），則**略過**此任務，並在 `mutation-test-result.md` 或向負責人說明中註記略過原因。
- 資源與慣例：
  - 變異測試資源放在 `TestProj/`，各功能的產物放在 `TestProj/<FeatureName>/`（功能測試資料夾）。
  - 複製 `TestProj/TemplateTestProj.csproj` 與 `TestProj/template-stryker-config.json` 作為範本，分別改名為 `TestProj.csproj` 與 `stryker-config.json`，並調整 `ProjectReference`、`Compile` 路徑（因資料夾深一層，相對路徑需多一層 `../`）與 `mutate` glob（排除測試程式）。
- 結果報告匯出為 `mutation-test-result.md`，放在與 `design.md` 同一資料夾，需包含整體 mutation score 與具風險的 Undetected mutation 清單（位置、可能風險、建議處理）。
- 若缺少 Stryker CLI 等工具，**不要自行安裝**，請通知負責人。

---

## 程式碼規範（Coding Conventions）

詳見 `.kiro/steering/coding-conventions.md`，重點如下：

### 程式放置位置

- 程式根目錄：workspace 下若有 `Assets` 資料夾則為 `Assets/Scripts`，否則為 `Scripts/`。
- 生成的程式依 Spec 名稱拆分子資料夾（例如 `Scripts/SingletonPattern/`），測試放在該功能的 `Tests/` 子資料夾。

### Namespace 與 Assembly 拆分

依功能與 `UnityEngine` 的相依情況拆分 namespace 與 asmdef：

- **不相依 `UnityEngine`** 的純邏輯、資料模型、演算法，獨立成專屬 namespace 與 asmdef，使其可在純 .NET 環境（單元測試、PBT、mutation testing）編譯與測試。獨立部分不得 `using UnityEngine` 或引用任何 `UnityEngine.*` 型別。
- **相依 `UnityEngine`** 的部分（MonoBehaviour、ScriptableObject、Unity API 呼叫）放在另一組 namespace 與 asmdef。
- 例外：若整個功能高度相依 `UnityEngine`（拆分後獨立部分幾乎為空），則不額外拆分，維持單一 namespace 與 asmdef。
- asmdef 命名應對應 namespace，並反映其相依性。

### Coding Style 與其他

- 遵循 StyleCop.Analyzers（JoJorge 版本）規範，細則見 `DesignData/CodingStyle/` 下各 rule area 文件（Spacing、Readability、Ordering、Naming、Maintainability、Layout、Documentation）。
- 避免 magic number：數值 / 字串常量提取為具語義名稱的 `const` 或 `static readonly`；語境明確的 `0`、`1`、`-1` 等可例外。
- MonoBehaviour、ScriptableObject 等 Unity 類別遵循 Unity 生命週期慣例。

---

## 語言慣例（Language Conventions）

詳見 `.kiro/steering/language-conventions.md`：

- **預設一律使用繁體中文** 撰寫內文、說明、表格內容、程式註解、名詞定義、使用者故事與驗收標準描述。
- **保持英文** 的例外：所有 `#`~`####` 章節標題、EARS 關鍵字（`WHEN`/`WHERE`/`WHILE`/`IF`/`THEN`/`THE`/`SHALL`）、類別 / 方法 / 變數 / 檔案 / 函式庫 / 框架名稱等程式識別字、縮寫與專有名詞（DCL、PBT、Singleton、volatile 等）。

---

## 開發心法（Ponytail：精實資深工程師模式）

詳見 `.kiro/steering/ponytail.md`。核心是「最好的程式碼是永遠不用寫的程式碼」，動手前先確認需求與現有程式，然後依序停在第一個成立的階梯：

1. 這功能真的需要做嗎？（YAGNI）
2. 現有 codebase 已經有了嗎？重用既有 helper / util / pattern。
3. 標準函式庫已經能做嗎？用它。
4. 平台原生功能能涵蓋嗎？用它。
5. 已安裝的套件能解決嗎？用它。
6. 能一行解決嗎？寫成一行。
7. 以上都不行，才寫出剛好可運作的最小程式。

其他原則：

- 不加未被要求的抽象、樣板與相依。刪除優於新增，無聊優於花俏，檔案越少越好。
- 修 bug 找**根因**而非症狀：修改共用函式時，grep 所有呼叫端，在共用處修一次。
- 不會偷懶的地方：理解問題、信任邊界的輸入驗證、防止資料遺失的錯誤處理、安全性、無障礙、真實硬體校準，以及明確被要求的事項。
- 非瑣碎邏輯要留一個可執行的最小檢查（一個小測試或 assert 型自我檢查），瑣碎的一行程式則免。

---

## 快速上手檢查清單

接手新功能時：

1. 在 `.kiro/specs/<feature-name>/` 建立或閱讀 `requirements.md`，確認需求與驗收標準（EARS）。
2. 完成 / 審閱 `design.md`，確認架構、Correctness Properties 與需求追溯，格式符合 `spec-design-format`。
3. 依 `tasks.md` 逐項實作並勾選，測試任務與實作任務分開。
4. 程式放對位置（`Scripts/<Feature>/`），依 `UnityEngine` 相依情況拆分 asmdef。
5. 遵循繁體中文語言慣例與 StyleCop coding style。
6. 最終 checkpoint 產出 `test-coverage.md`，並在功能不相依 `UnityEngine` 時執行 Stryker 變異測試產出 `mutation-test-result.md`。
7. 需要安裝任何套件 / 工具時，通知負責人，不要自行安裝。
