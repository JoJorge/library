# Implementation Plan: Input System（輸入管理系統）

## Overview

基於 Unity Input System 的輸入管理系統實作計畫。系統包含裝置偵測、動態綁定、衝突檢查與情境切換四大核心模組，由 `InputSystemManager`（繼承 `Singleton<T>`）統籌管理。程式碼放置於 `Assets/Scripts/InputSystem/`，測試放置於 `Assets/Scripts/InputSystem/Tests/`。

## Tasks

- [x] 1. Set up project structure, enums, and core interfaces
  - [x] 1.1 Create assembly definition and enum/interface files
    - 建立 `Assets/Scripts/InputSystem/` 目錄與 `InputSystem.asmdef`（引用 Utils、Unity.InputSystem）
    - 建立 `InputDeviceType` 列舉（KeyboardMouse, Gamepad）
    - 建立 `InputContext` 列舉（MainMenu, Gameplay, PauseMenu）
    - 建立 `SystemState` 列舉（Uninitialized, Initializing, Ready, Disabled）
    - 建立 `IContextHandler` 介面（Context, IsActive, Activate, Deactivate）
    - 建立 `IBindingStorage` 介面（Save, Load, Exists）
    - _Requirements: 5.7, 1.1, 1.2_

  - [x] 1.2 Create InputActionEnumAttribute and ActionEnumResolver
    - 建立 `InputActionEnumAttribute`（AttributeUsage Enum, 含 Context 屬性）
    - 建立 `ActionEnumResolver` 靜態類別（Validate, Resolve, GetContext 方法）
    - 建立範例動作列舉 `GameplayAction`、`MainMenuAction`，標註 `[InputActionEnum]`
    - _Requirements: 4.1, 4.2, 3.6_

  - [x] 1.3 Create BindingProfile and BindingConflict data models
    - 建立 `BindingProfile` 類別（DeviceType, OverridesJson）
    - 建立 `BindingConflict` 類別（BindingPath, Action, ConflictingActions, Context, DeviceType）
    - _Requirements: 2.2, 3.1, 3.2_

- [x] 2. Implement DeviceDetector
  - [x] 2.1 Implement DeviceDetector core logic
    - 實作 `DeviceDetector` 類別，透過 `InputSystem.onEvent` 監聽輸入事件
    - 實作裝置類型判定邏輯（Keyboard/Mouse → KeyboardMouse, Gamepad → Gamepad）
    - 實作滑鼠移動閥值與搖桿軸位移閥值判斷
    - 實作裝置切換事件通知（多播委派），含去重邏輯（同裝置連續輸入不重複觸發）
    - 實作 Init/Release 方法（訂閱/取消 InputSystem.onEvent）
    - 預設裝置類型為 KeyboardMouse
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8_

  - [x] 2.2 Write property test for DeviceDetector — Property 1
    - **Property 1: Device Type Stability**
    - 生成隨機同一裝置類型的輸入事件序列（1～500），驗證切換回呼觸發次數為 0
    - **Validates: Requirements 1.7**

  - [x] 2.3 Write unit tests for DeviceDetector
    - 測試初始裝置類型為 KeyboardMouse（Req 1.8）
    - 測試鍵盤/滑鼠輸入切換（Req 1.1）
    - 測試搖桿輸入切換（Req 1.2）
    - 測試回呼按註冊順序觸發（Req 1.3）
    - 測試取消註冊後不再收到回呼（Req 1.5）
    - _Requirements: 1.1, 1.2, 1.3, 1.5, 1.8_

- [x] 3. Implement BindingManager
  - [x] 3.1 Implement BindingManager core binding logic
    - 實作 `BindingManager` 類別，依賴 `InputActionAsset` 與 `IBindingStorage`
    - 實作 `Init` 方法（接收 asset 與 storage，載入已儲存設定或使用預設）
    - 實作 `ApplyBinding`（透過 `InputActionRebindingExtensions.ApplyBindingOverride`，同一幀內生效）
    - 實作 `GetDisplayNames` 與 `GetBindingPaths`（使用 `ActionEnumResolver` 驗證並轉換）
    - 實作查詢防禦邏輯（action 不存在、無綁定、未定義 DeviceType → 回傳空集合）
    - _Requirements: 2.1, 2.3, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6_

  - [x] 3.2 Implement BindingManager persistence and reset
    - 實作 `SaveBindings`（透過 `SaveBindingOverridesAsJson` 序列化，呼叫 `IBindingStorage.Save`）
    - 實作 `LoadBindings`（呼叫 `IBindingStorage.Load`，`LoadBindingOverridesFromJson` 還原；格式無效回退預設）
    - 實作 `ResetBinding`（`RemoveBindingOverride` 單一 Action）
    - 實作 `ResetAllBindings`（`RemoveAllBindingOverrides` 指定裝置類型）
    - 實作啟動時自動載入邏輯
    - _Requirements: 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10_

  - [x] 3.3 Write property test for BindingManager — Property 2
    - **Property 2: Binding Override Round-Trip**
    - 生成隨機 action/path 綁定組合，SaveBindings → LoadBindings，驗證 GetBindingPaths 一致
    - **Validates: Requirements 2.4, 2.6**

  - [x] 3.4 Write property test for BindingManager — Property 5
    - **Property 5: Binding Query Consistency**
    - 生成隨機 action/bindingPath/deviceType，ApplyBinding 後立即 GetBindingPaths，驗證包含新路徑
    - **Validates: Requirements 2.3, 4.6**

  - [x] 3.5 Write property test for BindingManager — Property 7
    - **Property 7: Reset Idempotence**
    - 生成隨機綁定修改後執行 ResetBinding 兩次，驗證兩次結果一致
    - **Validates: Requirements 2.9, 2.10**

  - [x] 3.6 Write unit tests for BindingManager
    - 測試儲存失敗保留現有綁定（Req 2.5）
    - 測試載入無效 JSON 回退至預設（Req 2.7）
    - 測試啟動時自動載入已儲存設定（Req 2.8）
    - 測試查詢不存在 Action 回傳空集合（Req 4.3）
    - 測試無綁定 Action 回傳空集合（Req 4.4）
    - 測試未定義列舉值回傳空集合（Req 4.5）
    - _Requirements: 2.5, 2.7, 2.8, 4.3, 4.4, 4.5_

- [x] 4. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Implement ConflictChecker
  - [x] 5.1 Implement ConflictChecker core logic
    - 實作 `ConflictChecker` 類別，依賴 `InputActionAsset` 與 `_contextHandlers` 字典
    - 實作 `Init` 方法
    - 實作 `CheckConflict`（在同 InputContext 同 DeviceType 下比對 Binding Path 字串完全相等）
    - 實作 `ScanAllConflicts`（指定裝置類型下全域掃描所有衝突群組）
    - 實作 `OnConflictDetected` 事件（動態綁定完成後自動觸發）
    - 實作未註冊 ActionName 的錯誤回傳
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6_

  - [x] 5.2 Write property test for ConflictChecker — Property 3
    - **Property 3: Conflict Symmetry**
    - 生成隨機同 Context action 配對與共用 Binding Path，驗證 CheckConflict 互相對稱
    - **Validates: Requirements 3.1, 3.2**

  - [x] 5.3 Write property test for ConflictChecker — Property 6
    - **Property 6: Conflict Scope Isolation**
    - 生成隨機兩個不同 Context，各自設定相同 Binding Path，驗證跨 Context 不報衝突
    - **Validates: Requirements 3.4**

  - [x] 5.4 Write unit tests for ConflictChecker
    - 測試未註冊 Action 衝突檢查回傳錯誤（Req 3.6）
    - 測試動態綁定後自動觸發衝突事件（Req 3.3）
    - 測試全域掃描涵蓋所有衝突（Req 3.5）
    - _Requirements: 3.3, 3.5, 3.6_

- [x] 6. Implement Context-Based Input Architecture
  - [x] 6.1 Implement InputSystemManager context registration and switching
    - 實作 `RegisterContextHandler`（上限 8 個，重複註冊拒絕，回傳 bool）
    - 實作 `SwitchContext`（先 Deactivate 當前→再 Activate 目標，互斥啟用）
    - 實作 `GetActiveContext`
    - 實作未註冊目標 Context 時維持現狀並回傳 false
    - 初始化完成後所有 handler 皆停用
    - _Requirements: 5.1, 5.2, 5.4, 5.5, 5.6, 5.8, 5.9, 5.10_

  - [x] 6.2 Write property test for context switching — Property 4
    - **Property 4: Context Exclusivity**
    - 生成隨機 Context 切換序列（2～100），每次切換後驗證 IsActive 數量 ≤ 1
    - **Validates: Requirements 5.2, 5.4, 5.5**

  - [x] 6.3 Write unit tests for context architecture
    - 測試切換未註冊 Context 失敗（Req 5.8）
    - 測試重複註冊同 Context 失敗（Req 5.9）
    - 測試超過 handler 上限拒絕註冊（Req 5.1）
    - 測試初始化完成後所有 handler 皆停用（Req 5.10）
    - _Requirements: 5.1, 5.8, 5.9, 5.10_

- [x] 7. Implement InputSystemManager lifecycle and integration
  - [x] 7.1 Implement InputSystemManager Init and Release
    - 實作 `Init`（載入 InputActionAsset、初始化 DeviceDetector/BindingManager/ConflictChecker、載入 BindingProfile、設定 SystemState）
    - 實作 `Release`（釋放資源、停用所有 handler、取消所有事件訂閱、設定 Disabled）
    - 實作系統未就緒操作守衛（非 Ready 狀態拒絕操作）
    - 實作 Asset 載入失敗進入 Disabled 狀態
    - 實作初始化中被銷毀的部分資源釋放
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7_

  - [x] 7.2 Implement IBindingStorage default implementation (PlayerPrefsBindingStorage)
    - 實作基於 `PlayerPrefs` 的 `IBindingStorage`，作為預設儲存實作
    - _Requirements: 2.4, 2.6_

  - [x] 7.3 Wire ConflictChecker with BindingManager
    - 在 `BindingManager.ApplyBinding` 完成後呼叫 `ConflictChecker.CheckConflict` 並觸發事件
    - 確保 `OnConflictDetected` 事件在動態綁定後自動觸發
    - _Requirements: 3.3_

  - [x] 7.4 Write unit tests for InputSystemManager lifecycle
    - 測試 Asset 載入失敗進入 Disabled（Req 6.5）
    - 測試 Disabled 狀態拒絕操作（Req 6.6）
    - 測試 Release 釋放資源（Req 6.4）
    - _Requirements: 6.4, 6.5, 6.6_

- [x] 8. Set up test assembly and integration tests
  - [x] 8.1 Create test assembly definition
    - 建立 `Assets/Scripts/InputSystem/Tests/` 目錄與 `InputSystem.Test.asmdef`（引用 InputSystem、Utils、NUnit、FsCheck）
    - 設定 `includePlatforms: Editor`
    - _Requirements: 全部_

  - [x] 8.2 Write integration tests
    - 測試完整初始化流程（系統到達 Ready 狀態）
    - 測試情境切換切實啟用 ActionMap
    - 測試綁定修改後影響實際輸入行為
    - _Requirements: 6.1, 6.2, 5.2, 2.3_

- [x] 9. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- 測試使用 FsCheck API 直接呼叫（不使用 FsCheck.NUnit），在 NUnit test 中執行
- `IBindingStorage` 與 `IContextHandler` 使用 NSubstitute mock
- `DeviceDetector` 測試透過 Unity `InputTestFixture` 模擬裝置輸入
- 每個測試的 `[SetUp]` 需透過反射重置 `Singleton<T>._instance`

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2", "1.3"] },
    { "id": 2, "tasks": ["2.1", "3.1", "8.1"] },
    { "id": 3, "tasks": ["2.2", "2.3", "3.2"] },
    { "id": 4, "tasks": ["3.3", "3.4", "3.5", "3.6"] },
    { "id": 5, "tasks": ["5.1", "6.1"] },
    { "id": 6, "tasks": ["5.2", "5.3", "5.4", "6.2", "6.3"] },
    { "id": 7, "tasks": ["7.1", "7.2"] },
    { "id": 8, "tasks": ["7.3"] },
    { "id": 9, "tasks": ["7.4", "8.2"] }
  ]
}
```
