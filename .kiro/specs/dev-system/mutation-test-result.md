# Mutation Test Result Report

## Overall Score

- Mutation Score: N/A（已跳過，未執行 Stryker）
- Killed: N/A
- Survived: N/A
- Timeout: N/A
- No Coverage: N/A

> 本次 mutation testing 依 steering 前置條件判定為**跳過**，因此無分數資料。跳過原因見下節。

## Skip Reason

依 steering「Mutation Testing (Final Checkpoint)」前置條件，mutation testing 僅在受測功能**不相依 `UnityEngine`** 時執行（Stryker 建置為純 .NET 專案，無法取得 `UnityEngine` 型別）。DevSystem 的兩組 assembly 判定如下：

- **`DevSystem.Unity`（直接相依）** — ScriptableObject、UI Toolkit、MonoBehaviour glue 直接 `using UnityEngine`，依規則直接跳過。

- **`DevSystem`（純邏輯，`noEngineReferences: true`，但間接相依）** — 雖然此 assembly 本身無任何 `using UnityEngine`，但它 reference 了 `InputSystem` assembly，並直接使用其中的 `BindingManager`、`ConflictChecker`、`ActionEnumResolver`、`InputContext`、`InputDeviceType`、`BindingConflict`、`[InputActionEnum]` 等型別（見 `DevAction.cs`、`DevKeyBinder.cs`、`DevKeyBindResult.cs`、`DevKeyRegistration.cs`）。而 `InputSystem` assembly 本身**直接相依 `UnityEngine` 與 `UnityEngine.InputSystem`**（例如 `BindingManager.cs`、`ConflictChecker.cs` 皆 `using UnityEngine;`）。

  因此 `DevSystem` 對 `UnityEngine` 存在**間接（transitive）相依**。實際以 `dotnet build` 驗證 `TestProj/DevSystem/TestProj.csproj` 時，編譯失敗於下列符號無法解析（節錄）：

  ```
  CS0246: 找不到型別或命名空間名稱 'InputSystem'
  CS0246: 找不到型別或命名空間名稱 'BindingManager'
  CS0246: 找不到型別或命名空間名稱 'ConflictChecker'
  CS0246: 找不到型別或命名空間名稱 'BindingConflict'
  CS0246: 找不到型別或命名空間名稱 'InputDeviceType'
  CS0246: 找不到型別或命名空間名稱 'InputActionEnum' / 'InputActionEnumAttribute'
  CS0103: 名稱 'InputContext' 不存在於目前的內容中
  ```

  要讓 `DevSystem` 於純 .NET 下編譯，勢必得一併納入 `InputSystem` 的原始碼，但那會直接引入 `UnityEngine`／`UnityEngine.InputSystem`，違反 Stryker 純 .NET 建置的前置條件。

  > 註：`DevSystem/Core` 中僅 `DevLogger`（Logging）與 Panel 樹模型（`PanelDefinition`／`PanelNodeDefinition`／`PanelNode`／`PanelTreeBuilder`）以及純列舉不觸及 `InputSystem`；但按鍵綁定相關型別與 `DevAction` 皆相依 `InputSystem`。由於 asmdef 為單一編譯單元，無法在不切分 assembly 的情況下只對不相依部分執行 Stryker。

**結論：本 spec 的功能程式（`DevSystem` 與 `DevSystem.Unity`）皆（直接或間接）相依 `UnityEngine`，mutation testing 全數跳過，未執行 Stryker。**

## Risky Undetected Mutations

無具風險的 Undetected mutation（未執行 mutation testing，無 mutation 資料）。

## Environment Notes

- Stryker CLI 已安裝（`dotnet-stryker` 4.16.0，見 `dotnet-tools.json`）、`dotnet` SDK 10.0.401，工具鏈本身可用；本次跳過純粹因功能相依 `UnityEngine`，非工具缺失。
- 已依 steering 程序將 `TestProj/TemplateTestProj.csproj`、`TestProj/template-stryker-config.json` 複製至 `TestProj/DevSystem/` 並重新命名為 `TestProj.csproj`、`stryker-config.json`（`mutate` 鎖定 `DevSystem/Core` 純邏輯、排除 `**/Tests/**/*.cs`），保留供後續參考；因編譯無法通過而未執行 `dotnet stryker`。

## Follow-up Options（交由使用者決定）

以下為若日後仍希望對 `DevSystem` 純邏輯進行 mutation testing 的可能方向，本任務不逕行實作：

1. **切分 assembly**：將完全不相依 `InputSystem` 的部分（`DevLogger`、Panel 樹模型與相關列舉）獨立為更細的純邏輯 assembly，僅對該部分執行 Stryker。
2. **抽象化 `InputSystem` 相依**：於 `DevSystem` 以自有介面／DTO 隔離 `BindingManager`／`ConflictChecker` 等型別，使 `DevKeyBinder` 等不再直接相依 `InputSystem` 具體型別，即可在純 .NET 下編譯與變異。
3. **維持現狀**：接受此功能因相依 `UnityEngine`（間接經 `InputSystem`）而不適用 Stryker，改以既有的 NUnit + FsCheck 單元測試／PBT 與 Unity Test Runner 覆蓋（詳見 `test-coverage.md`）。
