# Implementation Plan: FSM System

## Overview

實作一個通用有限狀態機（FSM）系統，適用於 Unity/C# 環境。系統由三層組成：`MachineStatus` 列舉、`IState<TMachine>` 介面、`Machine<TOwner>` 抽象類別、`FsmManager` 管理器。實作順序為由底層介面與列舉開始，逐步往上建構 Machine 與 FsmManager，最後整合測試。

## Tasks

- [x] 1. Set up project structure and core types
  - [x] 1.1 Create MachineStatus enum and IState interface
    - 在程式根目錄下建立 `FsmSystem/` 子資料夾
    - 建立 `MachineStatus.cs`，定義 `Created`、`Running`、`Stopped`、`Destroyed` 四個值，加上 XML 文件註解
    - 建立 `IState.cs`，定義 `IState<TMachine> where TMachine : Machine<TMachine>` 介面，包含 `Start()`、`Update()`、`FixedUpdate()`、`End()` 四個方法
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 4.1, 5.1_

- [x] 2. Implement Machine class
  - [x] 2.1 Create Machine abstract class with internal state fields
    - 建立 `Machine.cs`，定義 `Machine<TOwner> where TOwner : Machine<TOwner>` 抽象類別
    - 實作內部欄位：`_currentState`、`_initialState`、`_isTransitioning`、`_isInStart`、`Status`
    - 實作唯讀屬性 `CurrentState` 和 `Status`
    - 實作內部建構子，接收 `IState<TOwner> initialState` 參數並驗證非 null
    - _Requirements: 5.1, 5.2, 6.1, 6.4_

  - [x] 2.2 Implement Machine lifecycle methods (StartMachine, StopMachine)
    - 實作 `StartMachine()`：檢查 Status 為 Created 或 Stopped 時才執行，將 Status 設為 Running，設定 `_currentState = _initialState`，執行 Start 方法
    - 實作 `StopMachine()`：檢查 Status 為 Running 時才執行，執行 Current_State 的 End 方法，將 Status 設為 Stopped
    - 對於無效操作（如 Running 時再 Start、非 Running 時 Stop）靜默忽略
    - 對於已 Destroyed 的 Machine 操作拋出 `ObjectDisposedException`
    - _Requirements: 4.2, 4.3, 4.4, 4.5, 1.6_

  - [x] 2.3 Implement TransitionTo method with guards
    - 實作 `TransitionTo(IState<TOwner> nextState)`
    - 檢查 `_isTransitioning` 旗標，若為 true 則忽略呼叫
    - 檢查 `_isInStart` 旗標，若為 true 則忽略呼叫
    - 設定 `_isTransitioning = true`，依序執行：Current_State.End() → 更新 `_currentState = nextState` → nextState.Start()
    - 在 Start 執行期間設定 `_isInStart = true`，完成後重設
    - 完成後重設 `_isTransitioning = false`
    - _Requirements: 2.1, 2.2, 2.4, 2.6, 2.7, 1.5_

  - [x] 2.4 Implement Machine Update and FixedUpdate methods
    - 實作 `internal void Update()`：若 Status 為 Running 且 `_currentState` 不為 null，呼叫 `_currentState.Update()`
    - 實作 `internal void FixedUpdate()`：若 Status 為 Running 且 `_currentState` 不為 null，呼叫 `_currentState.FixedUpdate()`
    - 例外處理：若生命週期方法拋出例外，傳播給呼叫端
    - _Requirements: 1.2, 1.3, 1.7_

  - [x] 2.5 Write property tests for Machine state lifecycle
    - **Property 1: Start Invoked Exactly Once**
    - **Property 2: Update/FixedUpdate 1-to-1 Call Mapping**
    - **Validates: Requirements 1.1, 1.2, 1.3, 1.8**

  - [x] 2.6 Write property tests for Machine transitions
    - **Property 3: Transition Sequence Ordering**
    - **Property 4: End Terminates Lifecycle**
    - **Property 6: Guarded Transition Rejection**
    - **Property 7: No Automatic Transitions**
    - **Validates: Requirements 1.4, 1.5, 2.2, 2.4, 2.5, 2.7**

  - [x] 2.7 Write property test for exception propagation
    - **Property 5: Exception Propagation Halts Lifecycle**
    - **Validates: Requirements 1.7**

  - [x] 2.8 Write unit tests for Machine edge cases
    - 測試初始 State 進入時不呼叫 End（Requirements 1.6）
    - 測試 TransitionTo 在 Update 和 FixedUpdate 中均可成功呼叫（Requirements 2.6）
    - 測試對已 Destroyed 的 Machine 操作拋出 ObjectDisposedException
    - 測試 StartMachine 在 Running 時被忽略（Requirements 4.3）
    - 測試 StopMachine 在非 Running 時被忽略（Requirements 4.5）
    - _Requirements: 1.6, 2.6, 4.3, 4.5_

- [x] 3. Checkpoint - Machine implementation verification
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Implement FsmManager class
  - [x] 4.1 Create FsmManager with machine registration and removal
    - 建立 `FsmManager.cs`，定義 `FsmManager` 類別
    - 實作內部欄位：`_machines`（List）、`_pendingAdd`（List）、`_pendingRemove`（List）、`_isUpdating`（bool）
    - 實作 `CreateMachine<TMachine>(IState<TMachine> initialState) where TMachine : Machine<TMachine>, new()`
    - 驗證 `initialState` 非 null，否則拋出 `ArgumentNullException`
    - 建立 TMachine 實例，設定初始 State，註冊至管理清單（或 pending 佇列），回傳實例
    - 實作 `RemoveMachine<TMachine>(TMachine machine)`：若在更新中則加入 `_pendingRemove`，否則立即處理（Running 先停止再移除，設為 Destroyed）
    - _Requirements: 3.1, 4.1, 4.6, 4.7, 6.1, 6.3_

  - [x] 4.2 Implement FsmManager Update and FixedUpdate with deferred mutation
    - 實作 `Update()`：設定 `_isUpdating = true`，按註冊順序逐一呼叫 Running Machine 的 Update()
    - 對每個 Machine 的 Update 使用 try-catch 隔離例外，記錄錯誤後繼續更新其餘 Machine
    - 實作 `FixedUpdate()`：同上邏輯但呼叫 FixedUpdate()
    - 實作 `ApplyPendingChanges()`：在 Update/FixedUpdate 迴圈結束後呼叫，處理 `_pendingAdd` 和 `_pendingRemove` 佇列
    - 設定 `_isUpdating = false`
    - _Requirements: 3.3, 3.5_

  - [ ]* 4.3 Write property tests for FsmManager
    - **Property 8: Machine Isolation**
    - **Property 9: Registration-Order Update**
    - **Property 10: Deferred Mutation During Update**
    - **Validates: Requirements 3.2, 3.3, 3.5, 3.6**

  - [ ]* 4.4 Write property tests for Machine lifecycle management
    - **Property 11: Machine Lifecycle State Transitions**
    - **Property 12: Remove Triggers Proper Cleanup**
    - **Validates: Requirements 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7**

  - [x] 4.5 Write unit tests for FsmManager edge cases
    - 測試 CreateMachine 傳入 null initialState 拋出 ArgumentNullException（Requirements 6.3）
    - 測試多個 Machine 獨立運行互不干擾（Requirements 3.2）
    - 測試 Machine 故障隔離：一個 Machine 例外不影響其他 Machine 更新
    - 測試同時存在多個 Machine 無上限（Requirements 3.1）
    - _Requirements: 3.1, 3.2, 6.3_

- [~] 5. Checkpoint - FsmManager implementation verification
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Integration and wiring
  - [x] 6.1 Create test helpers and mock states for integration testing
    - 建立測試用的具體 Machine 子類別（如 `TestMachine : Machine<TestMachine>`）
    - 建立測試用的 Mock State 實作，支援呼叫計數、呼叫順序記錄、可配置例外拋出
    - 建立 FsCheck 自訂產生器（`ArbitraryMachineOps`、`ArbitraryStateSequence`、`ArbitraryUpdateCount` 等）
    - _Requirements: 全部（測試基礎設施）_

  - [ ]* 6.2 Write integration tests for full FSM workflow
    - 測試完整流程：CreateMachine → StartMachine → 多次 Update/FixedUpdate → TransitionTo → StopMachine → RemoveMachine
    - 測試多個 Machine 同時運行的完整場景
    - 測試在 Update 迴圈中註冊/移除 Machine 的延遲生效
    - _Requirements: 1.1–1.8, 2.1–2.7, 3.1–3.6, 4.1–4.7_

- [~] 7. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- 測試框架使用 NUnit（Unity Test Framework）搭配 FsCheck（Property-Based Testing）
- 所有程式碼遵循 StyleCop.Analyzers 規範（JoJorge 版本）
- Machine 使用 CRTP 模式確保編譯期型別安全

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["2.1"] },
    { "id": 2, "tasks": ["2.2", "2.3", "2.4"] },
    { "id": 3, "tasks": ["2.5", "2.6", "2.7", "2.8", "6.1"] },
    { "id": 4, "tasks": ["4.1"] },
    { "id": 5, "tasks": ["4.2"] },
    { "id": 6, "tasks": ["4.3", "4.4", "4.5"] },
    { "id": 7, "tasks": ["6.2"] }
  ]
}
```
