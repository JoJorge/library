# Implementation Plan: DevSystem（開發者工具系統）

## Overview

本實作計畫依據 `design.md` 將 DevSystem 拆解為可增量交付的程式任務，涵蓋 10 項需求與 11 條 Correctness Properties。實作以 **C#**（Unity 6000.5.7f1 相容）進行，使用 New Input System、UI Toolkit、NUnit + FsCheck 3.x（`FsCheck.Fluent`，不使用 `FsCheck.NUnit`），並由 Unity Test Runner 執行測試。

依 coding conventions 的 namespace/asmdef 拆分規則，程式碼分為兩組 assembly：

- **`DevSystem`（純邏輯，不相依 `UnityEngine`）**：`DevLogger`、`DevKeyBinder`、`PanelDefinition`/`PanelNodeDefinition`/`PanelNode` 資料模型、所有列舉、`ILogSink`/`ILogToggleConfigLoader` 介面、`DevKeyRegistration`/`DevKeyBindResult`、DEV_MODE gating 邏輯。此 assembly 設定 `noEngineReferences: true`，可於純 .NET 環境進行單元測試、PBT 與 mutation testing。它 reference 既有的 `InputSystem`（取得 `BindingManager`、`ConflictChecker`、`BindingConflict`、`[InputActionEnum]`、`ActionEnumResolver`、`InputContext`、`InputDeviceType`）與 `Utils`（`Singleton<T>`）。
- **`DevSystem.Unity`（相依 `UnityEngine`）**：`DevSystemManager`（`Singleton<DevSystemManager>`）、`DevLogToggleConfig`（ScriptableObject + Odin `[ShowInInspector]`）、`DevLogToggleConfigLoader`、預設 `ILogSink`（包裹 `Debug.Log`）、`DevPanel`（UI Toolkit）、`DevKeyListTab`、`DebugLogSettingsTab`。

所有程式置於 `Assets/Scripts/DevSystem/` 下，依上述兩組拆分子資料夾。測試類別位於獨立的測試 assembly，與功能程式分離。

> 說明：`DevAction` enum 需標註 `[InputActionEnum]`，屬純 C# attribute（定義於 `InputSystem` assembly），因此可置於純邏輯 assembly。`DevPanel` 與內建分頁使用 UI Toolkit（`VisualElement`、`Toggle`），故置於 Unity-dependent assembly。

## Tasks

- [x] 1. Set up DevSystem assemblies, folders and DEV_MODE gating skeleton
  - [x] 1.1 建立純邏輯 assembly 與資料夾骨架
    - 在 `Assets/Scripts/DevSystem/Core/` 建立 `DevSystem.asmdef`（`name: "DevSystem"`、`rootNamespace: "DevSystem"`、`references: ["InputSystem", "Utils"]`、`noEngineReferences: true`、`autoReferenced: true`）
    - 建立列舉檔：`DevSystemState`、`LogCategory`（General/Network/Gameplay/UI）、`DevAction`（`[InputActionEnum(InputContext.Gameplay)]`，成員皆 Dev 前綴）、`PanelNodeKind`、`DevKeyBindFailure`
    - 確認純邏輯 assembly 不 `using UnityEngine`
    - _Requirements: 1.6, 2.3, 2.4, 4.5, 4.6_

  - [x] 1.2 建立 Unity-dependent assembly 資料夾骨架
    - 在 `Assets/Scripts/DevSystem/Unity/` 建立 `DevSystem.Unity.asmdef`（`name: "DevSystem.Unity"`、`rootNamespace: "DevSystem.Unity"`、`references: ["DevSystem", "InputSystem", "Utils", "Unity.InputSystem"]`、`noEngineReferences: false`）
    - 設定 Odin Inspector 可用（既有套件，無需安裝）
    - _Requirements: 6.1_

- [x] 2. Implement DevLogger core (pure logic)
  - [x] 2.1 定義日誌抽象介面與資料
    - 在 `DevSystem/Core/Logging/` 建立 `ILogSink`（`bool Write(string formatted)`）與 `ILogToggleConfigLoader`（`IReadOnlyDictionary<LogCategory, bool> Load()`）
    - _Requirements: 2.8, 3.3_

  - [x] 2.2 實作 DevLogger 格式化與閘控邏輯
    - 建立 `DevLogger`：`Log(LogCategory, string)`、`SetCategoryEnabled`、`IsCategoryEnabled`、`GetCategories`、`LoadToggleConfig`
    - `[Dev][name] MSG` 格式化：未定義分類以 `Enum.IsDefined` 判斷並改用固定預設名（`General`）；null/空訊息 MSG 為空字串；訊息超過 `MaxMessageLength`(4096) 截斷為前綴
    - 分類開關以記憶體 `Dictionary<LogCategory, bool>` 維護，永不寫回；載入前/無設定一律視為開啟；`sink.Write` 失敗或拋例外時抑制不外拋
    - `Log` 以 `[Conditional("DEV_MODE")]` 標註（void API）；有回傳值 API 以 `#if DEV_MODE ... #else return default; #endif` 包裹
    - `SetCategoryEnabled` 切換後於後續第一筆該分類日誌請求即生效
    - _Requirements: 2.1, 2.2, 2.5, 2.6, 2.7, 2.8, 3.1, 3.2, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10, 9.4_

- [x] 3. Implement DevKeyBinder core (pure logic)
  - [x] 3.1 建立按鍵註冊資料模型
    - 在 `DevSystem/Core/KeyBinding/` 建立 `DevKeyRegistration`（Action/BindingPath/Description/Context/DeviceType，唯讀）與 `DevKeyBindResult`（Success/FailureReason/Conflicts，重用 `BindingConflict`）
    - _Requirements: 4.3, 5.3_

  - [x] 3.2 實作 DevKeyBinder 綁定與衝突閘控
    - 建立 `DevKeyBinder`：建構子注入 `BindingManager`、`ConflictChecker`；`RegisterDevKey`、`Unbind`、`GetRegistrations`、`ClearAll`
    - 註冊流程：驗證必要欄位（path 非空、Context/DeviceType 可經 `ActionEnumResolver` 解析）→ 缺欄位回傳 `InvalidRequest` 且**不**做衝突檢查 → 路徑無效回傳 `InvalidPath` → 重複 `DevAction` 回傳 `Duplicate` 保留既有 → 呼叫 `ConflictChecker.CheckConflict` → 衝突非空回傳 `Conflict` + 相同衝突清單且未寫入 → 衝突為空才 `BindingManager.ApplyBinding` 並記錄 `DevKeyRegistration`
    - `Unbind` 解除綁定並移除註冊，查無回傳 false；`ClearAll` 清除全部並解綁
    - 全部方法以 `#if DEV_MODE / #else return default;` gating
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.7, 4.8, 4.9, 5.1, 5.2, 5.3, 5.4, 5.5, 10.3, 10.4_

- [x] 4. Implement Panel data models and tree builder (pure logic)
  - [x] 4.1 建立面板宣告式定義與節點樹模型
    - 在 `DevSystem/Core/Panel/` 建立 `IDevPanelTab`（Title/BuildContent/OnShown/OnUpdate —— `BuildContent` 回傳 UI Toolkit 型別，故介面置於 Unity assembly，見任務 6.1）
    - 於純邏輯建立 `PanelDefinition`（`Nodes`）、`PanelNodeDefinition`（Title/Tab/Children，巢狀）、`PanelNode`（Depth/Kind/Title/Tab/Children）
    - 建立純邏輯的樹建構器 `PanelTreeBuilder`（供 `DevPanel` 使用）：依巢狀定義建立 `PanelNode` 樹、頂層 Depth=1、`Kind` 由 `Children` 是否為空推導、順序即 `Children` 清單順序；任一節點深度 > `DevPanel.MaxDepth`(10) 或頂層數量 > `MaxTabCount`(100) 或 < `MinDepth`(1) 時拒絕並保留既有結構
    - _Requirements: 6.1, 6.2, 6.3, 6.5, 6.8, 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 5. Checkpoint - 純邏輯層測試
  - [x] 5.1 撰寫 DevLogger 屬性測試與範例測試
    - **Property 1: Log Format Structure and Robustness** — 隨機 `LogCategory`（含越界強制轉型）與訊息（含 null/空/>4096），攔截 mock `ILogSink`，驗證 `[Dev][name] msg` 結構、預設分類名、截斷為前綴；注入拋例外/回 false 的 sink 驗證不外拋
    - **Validates: Requirements 2.2, 2.5, 2.6, 2.7, 2.8**
    - **Property 2: Category Toggle Gate Correctness** — 隨機分類+開關序列+訊息，驗證 sink 呼叫次數 == (開啟?1:0)，切換後首筆即生效
    - **Validates: Requirements 3.1, 3.2, 3.10**
    - **Property 3: Toggle Config Application and Non-Persistence** — 隨機啟用清單，`LoadToggleConfig` 後清單內=啟用/外=停用；無設定/載入前全開；重載決定性；對 mock 設定來源斷言零寫入
    - **Validates: Requirements 3.3, 3.4, 3.5, 3.7, 3.8**
    - 範例測試：`Log` 簽章存在（2.1）、`LogCategory` 為 enum 可擴充（2.3, 2.4）、載入前全開（3.6）、`SetCategoryEnabled` 存在回 bool（3.9）

  - [x] 5.2 撰寫 DevKeyBinder 屬性測試與範例測試
    - **Property 4: Dev Key Registration Query Consistency** — 唯一 action/有效路徑/互不衝突集合（mock `ConflictChecker` 回空），驗證 `GetRegistrations` 一一對應、重複被拒既有不變、`Unbind` 後不含該項
    - **Validates: Requirements 4.3, 4.4, 4.7, 4.9**
    - **Property 5: Dev Key Conflict Gate and Scope Reuse** — mock `CheckConflict` 回受控清單：非空→拒絕且回相同清單且未寫入；空→完成且呼叫 `ApplyBinding`；驗證寫入前先呼叫 `CheckConflict` 且無自訂範圍邏輯
    - **Validates: Requirements 5.1, 5.2, 5.3, 5.4**
    - 範例測試：成功路徑呼叫 `ApplyBinding`（4.1）、空/無效路徑回 `InvalidPath`（4.2）、`DevAction` 皆 Dev 前綴反射驗證（4.5）、`Unbind` 不存在回 false（4.8）、缺欄位不做衝突檢查回 `InvalidRequest`（5.5）

  - [x] 5.3 撰寫 Panel 樹模型屬性測試與範例測試
    - **Property 6: Panel Depth Validity Invariant** — 生成含各種深度定義（含 > `MaxDepth`），驗證成功建構者最大深度 ≤ `MaxDepth`、越界被拒且既有結構不變
    - **Validates: Requirements 6.5, 7.3**
    - **Property 7: Panel Navigation and Ordering** — 隨機巢狀樹，驗證父子關係與巢狀一致、同層順序等於 `Children` 清單順序、`Kind == (Children 非空 ? SubTabContainer : Leaf)`
    - **Validates: Requirements 6.2, 6.3, 6.8, 7.2**
    - 範例測試：巢狀順序保留（7.2, 7.4）

  - [x] 5.4 執行純邏輯層所有測試並使其通過
    - 建立 `DevSystem/Core/Tests/DevSystem.Core.Tests.asmdef`（Editor-only、`references: ["DevSystem", "InputSystem", "Utils"]`、`precompiledReferences: ["nunit.framework.dll", "FsCheck.dll", "NSubstitute.dll"]`、`defineConstraints: ["UNITY_INCLUDE_TESTS"]`）
    - 於 Unity Test Runner (Edit Mode) 執行任務 5.1–5.3 的測試，修正實作直到全數通過
    - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Implement Unity-dependent panel UI (UI Toolkit)
  - [x] 6.1 定義 IDevPanelTab 介面（Unity assembly）
    - 在 `DevSystem/Unity/Panel/` 建立 `IDevPanelTab`（`string Title`、`VisualElement BuildContent()`、`void OnShown()`、`void OnUpdate()`）
    - 調整 `PanelNodeDefinition`/`PanelNode` 對 `Tab` 的型別引用策略：`IDevPanelTab` 因回傳 `VisualElement` 需置於 Unity assembly；純邏輯 `PanelNodeDefinition`/`PanelNode`/`PanelTreeBuilder` 改以泛型或 `object` tab 承載，`DevPanel` 於 Unity 層轉型使用（保持純邏輯不相依 UI Toolkit）
    - _Requirements: 6.1_

  - [x] 6.2 實作 DevPanel（UI Toolkit）
    - 建立 `DevPanel`：常數 `MinDepth=1`、`MaxDepth=10`、`MaxTabCount=100`；`Build(PanelDefinition)` 委派純邏輯 `PanelTreeBuilder` 建樹，成功才建立 UI Toolkit 呈現；`Release`、`GetTabs`
    - 導覽顯示：選取節點 `Children` 非空顯示子分頁選擇器、為空顯示 `Tab.BuildContent()`
    - 不對外公開任何執行期新增/排序/升降/深度變更 API
    - 全部方法 `#if DEV_MODE / #else return default;` gating
    - _Requirements: 6.1, 6.2, 6.3, 6.8, 7.1, 7.2, 7.3, 7.5, 10.3, 10.4_

- [x] 7. Implement config, sink, and built-in tabs (Unity assembly)
  - [x] 7.1 實作日誌設定 ScriptableObject、Loader 與預設 Sink
    - 在 `DevSystem/Unity/Logging/` 建立 `DevLogToggleConfig`（ScriptableObject，`[CreateAssetMenu]`，`[ShowInInspector] List<LogCategory> EnabledCategories { get; set; }` 純 property 非 `[SerializeField]`，不落地）
    - 建立 `DevLogToggleConfigLoader : ILogToggleConfigLoader`（將啟用清單映射為涵蓋所有 `LogCategory` 的唯讀對映，清單為權威來源）
    - 建立預設 `ILogSink` 實作 `UnityDebugLogSink`（包裹 `Debug.Log`，try-catch 回傳成敗）
    - _Requirements: 3.3, 3.4, 3.7_

  - [x] 7.2 實作 DevKeyListTab
    - 在 `DevSystem/Unity/Panel/Tabs/` 建立 `DevKeyListTab : IDevPanelTab`（注入 `DevKeyBinder`）
    - `OnShown` 讀取 `GetRegistrations()` 最新清單，依 `BindingPath` 以 `StringComparer.Ordinal` 升冪排序，逐筆顯示綁定按鍵與敘述；空清單顯示提示並顯示零筆
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

  - [x] 7.3 實作 DebugLogSettingsTab
    - 在 `DevSystem/Unity/Panel/Tabs/` 建立 `DebugLogSettingsTab : IDevPanelTab`（注入 `DevLogger`）
    - `BuildContent` 為 `GetCategories()` 每分類建立 `Toggle` 顯示 `IsCategoryEnabled`；集合為空顯示空狀態訊息
    - 使用者切換：呼叫 `SetCategoryEnabled(category, target)`，成功則同影格將該列同步為 `IsCategoryEnabled` 回報值；失敗則還原該列並顯示錯誤
    - `OnUpdate` 將各列同步為 `DevLogger` 最新狀態（反映外部來源變更）
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_

- [x] 8. Integrate DevSystemManager and wire subsystems
  - [x] 8.1 實作 DevSystemManager 生命週期
    - 在 `DevSystem/Unity/` 建立 `DevSystemManager : Singleton<DevSystemManager>`：`DevSystemState _state`、子元件 `Logger`/`KeyBinder`/`Panel` property
    - `Init(PanelDefinition)`：未就緒時讀取 `Log_Toggle_Config`（`LoadToggleConfig`）、建立 `DevPanel`（掛入 `DevKeyListTab`、`DebugLogSettingsTab`）、`KeyBinder` 取用 `InputSystemManager.Instance` 的 `BindingManager`/`ConflictChecker`、標記 `Ready`；已就緒為冪等 no-op
    - `Release()`：就緒時釋放面板、`KeyBinder.ClearAll()`，任一失敗回傳 false 但狀態仍轉 `Uninitialized`；未就緒為 no-op
    - 就緒守衛：功能 API 於非 `Ready` 拒絕並回傳失敗指示（NotReady/false/空集合）
    - 全部方法 `#if DEV_MODE / #else return default;` gating；`State` getter 亦 gating
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7_

  - [x] 8.2 建立測試 assembly 並驗證 DEV_MODE 未定義組態可編譯
    - 建立 `DevSystem/Unity/Tests/DevSystem.Unity.Tests.asmdef`（Editor-only、`references: ["DevSystem", "DevSystem.Unity", "InputSystem", "Utils", "UnityEngine.TestRunner", "UnityEditor.TestRunner", "Unity.InputSystem"]`、`precompiledReferences: ["nunit.framework.dll", "FsCheck.dll", "NSubstitute.dll"]`、`defineConstraints: ["UNITY_INCLUDE_TESTS"]`）
    - 建立呼叫端範例，確認未定義 `DEV_MODE` 時呼叫各公開 API 零錯誤/零警告
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 9. Checkpoint - Unity 層與整合測試
  - [x] 9.1 撰寫內建分頁屬性測試與範例測試
    - **Property 8: Dev Key List Ordering and Currency** — 隨機註冊集合，`DevKeyListTab` 顯示項目 == 當下 `GetRegistrations` 且依 ordinal 升冪；顯示前後變更清單驗證反映最新
    - **Validates: Requirements 8.2, 8.3, 8.5**
    - **Property 9: Log Settings UI-Model Sync** — 隨機分類與切換目標，切換後該列 == `IsCategoryEnabled`；外部改 `DevLogger` 後 `OnUpdate` 各列同步
    - **Validates: Requirements 9.2, 9.4, 9.5, 9.6**
    - 範例測試：按鍵列表空清單提示（8.4）、日誌設定空狀態（9.3）、切換更新失敗還原並顯示錯誤（9.7）、內建分頁可作為 `PanelNodeDefinition.Tab` 掛入（8.1, 9.1）

  - [x] 9.2 撰寫生命週期屬性測試與範例測試
    - **Property 10: Lifecycle Correctness** — `Init` 次數 n∈[1,10] 面板/設定各一次且狀態 Ready；未就緒對任意功能 API 拒絕且狀態不變；就緒 `Release` 成功後 `GetRegistrations` 空、面板釋放、狀態未就緒
    - **Validates: Requirements 10.1, 10.4, 10.5, 10.6**
    - 範例測試：`Singleton` 全域存取（10.2）、`Release` 未就緒為 no-op（10.7）

  - [x] 9.3 撰寫 DEV_MODE 空實作等價性屬性測試
    - **Property 11: DEV_MODE No-Op Equivalence** — 於**未定義 `DEV_MODE`** 的測試組態，對任意引數呼叫各具回傳值 API 斷言回傳 `default(T)` 且前後查詢無可觀察狀態變更
    - **Validates: Requirements 1.2, 1.3, 1.5**

  - [x] 9.4 撰寫整合測試
    - 完整 `Init` 流程（讀設定、建面板、狀態 Ready、兩內建分頁掛載）
    - 與真實 `InputSystemManager.Instance` 的 `BindingManager`/`ConflictChecker` 整合註冊 Dev 按鍵驗證衝突檢查與綁定套用
    - 面板導覽（多層樹選取容器/葉節點）、`Release` 清理
    - _Requirements: 5.1, 6.2, 6.3, 10.1, 10.3_

  - [x] 9.5 執行 Unity 層與整合所有測試並使其通過
    - 於 Unity Test Runner (Edit Mode) 執行任務 9.1–9.4 測試（`[SetUp]` 以反射重置 `Singleton` 基底 `_instance`），修正實作直到全數通過
    - Ensure all tests pass, ask the user if questions arise.

- [x] 10. Final checkpoint - 全測試、覆蓋率文件與 mutation testing
  - [x] 10.1 執行全部測試並確認通過
    - 於 Unity Test Runner 執行 DevSystem 所有 Edit Mode 測試，確認全數通過
    - Ensure all tests pass, ask the user if questions arise.

  - [x] 10.2 產生需求測試覆蓋率文件
    - 於 `design.md` 同資料夾產生 `test-coverage.md`，依 steering 固定格式列出全部需求（含各子項）對應的測試方法（`Property N` 或 unit test 方法名）與涵蓋狀態
    - 此任務僅列印覆蓋率供使用者參考，**不得**調整或新增任何測試

  - [x] 10.3 執行 mutation testing（Stryker）
    - 前置檢查：確認受測功能是否相依 `UnityEngine`。純邏輯 assembly `DevSystem`（`DevLogger`、`DevKeyBinder`、Panel 樹模型與列舉，`noEngineReferences: true`）不相依 `UnityEngine`，對此部分執行 Stryker；`DevSystem.Unity`（ScriptableObject/UI Toolkit/MonoBehaviour glue）相依 `UnityEngine`，**跳過並於 `mutation-test-result.md` 註記原因**
    - 依 steering 程序：複製 `TestProj/TemplateTestProj.csproj` 與 `TestProj/template-stryker-config.json` 至 `TestProj/DevSystem/`，重新命名為 `TestProj.csproj` 與 `stryker-config.json`，調整 `ProjectReference`/`Compile` include（因多一層須多一個 `../`）與 `mutate` globs（鎖定 `DevSystem` 純邏輯、排除 `**/Tests/**/*.cs`）
    - 於 `TestProj/DevSystem/` 執行 `dotnet stryker`（若缺 Stryker CLI 請使用者安裝，勿自行安裝）
    - 分析報告並依固定格式輸出 `mutation-test-result.md` 至 `design.md` 同資料夾（含整體 mutation score 與具風險的 Undetected mutations 清單）
    - 此任務**不得**調整或新增任何測試或功能程式，結果交由使用者決定後續處理

## Notes

- 標示 `*` 的子任務為選用（測試撰寫），可為求快速 MVP 略過；核心實作任務不得標為選用。
- 每個任務標註對應需求以利追溯。
- Checkpoint 任務將測試執行與驗證集中，與測試撰寫分離，避免測試失敗阻塞測試實作。
- 屬性測試以 `FsCheck.Fluent` 於標準 NUnit `[Test]` 方法內呼叫（不使用 `FsCheck.NUnit`／`[Property]`），每屬性最少 100 次隨機迭代，且不使用 `[Category]` 屬性。
- 套件安裝（FsCheck 3.x、Stryker CLI 等）由使用者負責，任務不含自動安裝。
- 純邏輯與 `UnityEngine` 相依部分拆為 `DevSystem` 與 `DevSystem.Unity` 兩組 assembly，使純邏輯可進行單元測試、PBT 與 mutation testing。

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["2.1", "3.1", "4.1"] },
    { "id": 2, "tasks": ["2.2", "3.2"] },
    { "id": 3, "tasks": ["5.1", "5.2", "5.3"] },
    { "id": 4, "tasks": ["5.4"] },
    { "id": 5, "tasks": ["6.1"] },
    { "id": 6, "tasks": ["6.2", "7.1"] },
    { "id": 7, "tasks": ["7.2", "7.3"] },
    { "id": 8, "tasks": ["8.1"] },
    { "id": 9, "tasks": ["8.2"] },
    { "id": 10, "tasks": ["9.1", "9.2", "9.3", "9.4"] },
    { "id": 11, "tasks": ["9.5"] },
    { "id": 12, "tasks": ["10.1"] },
    { "id": 13, "tasks": ["10.2"] },
    { "id": 14, "tasks": ["10.3"] }
  ]
}
```