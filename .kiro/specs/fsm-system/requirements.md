# Requirements Document

## Introduction

本文件定義通用有限狀態機（FSM）系統的需求。此系統設計為 Unity/C# 環境下的通用框架，支援狀態生命週期管理（Start、Update、FixedUpdate、End），並允許同時運行多個獨立的 Machine。系統旨在提供靈活、可擴展的狀態管理機制，適用於 AI 行為、UI 流程、遊戲邏輯等多種場景。

## Glossary

- **FsmManager**: 有限狀態機系統的頂層管理器，負責建立、管理與更新所有 Machine。
- **Machine**: 一個獨立運行的有限狀態機實例，僅持有當前狀態的參考，並提供轉換方法供 State 呼叫。Machine 不維護狀態集合，不進行狀態發現。
- **State**: FSM 中的一個狀態，具備 Start、Update、FixedUpdate、End 四個生命週期方法。State 實例由呼叫端負責建立並傳入。
- **Transition**: 狀態之間的轉換，由 State 主動呼叫 Transition_Function 並傳入目標 State 實例觸發。
- **Current_State**: Machine 當前正在執行的 State。
- **Transition_Function**: Machine 提供的轉換方法（`TransitionTo(IState<TOwner> nextState)`），接受目標 State 實例作為參數，供 Current_State 呼叫以觸發狀態切換。

## Requirements

### Requirement 1: State Lifecycle

**User Story:** As a developer, I want each state to have Start, Update, FixedUpdate, and End lifecycle methods, so that I can define behavior for entering, per-frame logic, fixed-interval physics logic, and exiting a state.

#### Acceptance Criteria

1. WHEN a Machine 進入一個 State 時，THE State SHALL 執行其 Start 方法恰好一次，且在 Start 方法完成前不執行該 State 的 Update 或 FixedUpdate 方法
2. WHILE a State 為 Current_State 時，THE State SHALL 在每次 Machine 的 Update 被呼叫時執行其 Update 方法恰好一次，且每次 Update 呼叫之間不重複執行
3. WHILE a State 為 Current_State 時，THE State SHALL 在每次 Machine 的 FixedUpdate 被呼叫時執行其 FixedUpdate 方法恰好一次，且每次 FixedUpdate 呼叫之間不重複執行
4. WHEN a Machine 離開一個 State 時，THE State SHALL 執行其 End 方法恰好一次，且在 End 方法完成後該 State 不再接收 Update 或 FixedUpdate 呼叫
5. WHEN a Machine 從一個 State 轉換至另一個 State 時，THE Machine SHALL 先執行前一個 State 的 End 方法至完成，再執行新 State 的 Start 方法，保證執行順序為 End → Start 且無交錯
6. WHEN a Machine 進入其初始 State（無前一個 State 存在）時，THE Machine SHALL 直接執行該 State 的 Start 方法，不呼叫任何 End 方法
7. IF State 的 Start、Update、FixedUpdate 或 End 方法拋出例外，THEN THE Machine SHALL 停止該 State 的後續生命週期方法執行，並將例外傳播給呼叫端，且不進入新的 State
8. WHEN a State 的 Start 方法執行完成後且該 State 仍為 Current_State 時，THE Machine SHALL 保證在下一次 Update 或 FixedUpdate 呼叫前不再重複執行 Start 方法

### Requirement 2: State Transition

**User Story:** As a developer, I want each State to be responsible for deciding when to trigger a transition by calling a Machine-provided transition function and passing in the target State instance, so that transition logic is encapsulated within the State itself and the Machine remains stateless regarding available states.

#### Acceptance Criteria

1. THE Machine SHALL 提供一個 Transition_Function（`TransitionTo(IState<TOwner> nextState)`），接受目標 State 實例作為參數，供 Current_State 呼叫以請求轉換
2. WHEN Current_State 呼叫 Transition_Function 並傳入一個有效的目標 State 實例時，THE Machine SHALL 依序執行 Current_State 的 End 回呼、更新 Current_State 為傳入的目標 State 實例、再執行該目標 State 的 Start 回呼
3. THE 泛型型別約束 SHALL 限制 Transition_Function 的參數僅接受實作 `IState<TOwner>` 介面的 State 實例，使綁定至其他 Machine 型別的 State 無法在編譯期通過型別檢查
4. WHILE Machine 正在執行轉換流程（End 回呼至 Start 回呼完成之間），IF State 再次呼叫 Transition_Function，THEN THE Machine SHALL 忽略該呼叫且不觸發新的轉換
5. THE Machine SHALL 不自動評估或檢查任何 Transition 條件，狀態轉換僅在 State 明確呼叫 Transition_Function 時發生
6. WHEN State 在其 Update、FixedUpdate 或其他自訂邏輯中呼叫 Transition_Function 時，THE Machine SHALL 接受該呼叫並執行轉換流程，不限制呼叫來源的生命週期階段（Start 除外，見下條）
7. IF State 在其 Start 方法中呼叫 Transition_Function，THEN THE Machine SHALL 忽略該呼叫且不執行轉換，確保 Start 方法完整執行後狀態才可被切換

### Requirement 3: Multiple FSM Instances

**User Story:** As a developer, I want to run multiple Machine instances simultaneously, so that different game objects or systems can each have their own independent state machines.

#### Acceptance Criteria

1. THE FsmManager SHALL 支援同時存在多個獨立運行的 Machine，不設上限
2. THE 每個 Machine SHALL 獨立維護自身的 Current_State，不受其他 Machine 的狀態變更影響
3. WHEN FsmManager 執行更新時，THE FsmManager SHALL 依照 Machine 的註冊順序，逐一更新所有處於運行狀態的 Machine
4. THE 每個 Machine SHALL 僅持有當前 State 的參考，不維護狀態集合，各 Machine 的狀態互不干擾
5. IF 在 FsmManager 更新迴圈執行期間有新的 Machine 被註冊或移除，THEN THE FsmManager SHALL 在當次更新迴圈完成後才將變更生效，不影響當次迭代
6. WHEN 一個 Machine 的 State 觸發狀態切換時，THE 其他 Machine 的 Current_State 與轉換流程 SHALL 不受影響

### Requirement 4: Machine Lifecycle Management

**User Story:** As a developer, I want to create, start, stop, and remove Machine instances at runtime, so that I can dynamically control state machine lifecycles.

#### Acceptance Criteria

1. WHEN 開發者請求建立新的 Machine 時，THE FsmManager SHALL 透過 `CreateMachine<TMachine>(IState<TOwner> initialState)` 建立一個新的 Machine，將傳入的 initialState 作為其初始 State，將 Machine 狀態設為「已建立（Created）」，註冊至管理清單，並回傳該實例的參考
2. WHEN 開發者啟動一個狀態為「已建立」或「已停止」的 Machine 時，THE Machine SHALL 將狀態設為「運行中（Running）」，進入建立時傳入的初始 State，並執行該 State 的 Start 方法
3. IF 開發者嘗試啟動一個狀態為「運行中」的 Machine，THEN THE FsmManager SHALL 忽略該操作並維持實例目前狀態不變
4. WHEN 開發者停止一個狀態為「運行中」的 Machine 時，THE Machine SHALL 執行 Current_State 的 End 方法、停止接收更新，並將狀態設為「已停止（Stopped）」
5. IF 開發者嘗試停止一個狀態非「運行中」的 Machine，THEN THE FsmManager SHALL 忽略該操作並維持實例目前狀態不變
6. WHEN 開發者移除一個 Machine 時，IF 該實例狀態為「運行中」，THEN THE FsmManager SHALL 先執行停止流程（觸發 Current_State 的 End 方法並停止更新），再將該實例從管理清單中移除並將實例標記為已銷毀（Destroyed）
7. WHEN 開發者移除一個狀態為「已建立」或「已停止」的 Machine 時，THE FsmManager SHALL 將該實例從管理清單中移除並將實例標記為已銷毀

### Requirement 5: Generic Type Safety

**User Story:** As a developer, I want the generic type constraint `IState<TMachine>` to prevent passing a State bound to one Machine type into a different Machine type, so that type mismatches are caught at compile time.

#### Acceptance Criteria

1. THE 每個 State SHALL 在編譯期透過實作 `IState<TMachine>` 介面宣告其所屬的 Machine 型別
2. THE 泛型型別約束 SHALL 確保 `TransitionTo(IState<TOwner> nextState)` 僅接受實作 `IState<TOwner>` 的 State 實例，使開發者無法傳入綁定至其他 Machine 型別的 State
3. THE `CreateMachine<TMachine>(IState<TOwner> initialState)` 的參數型別約束 SHALL 確保傳入的初始 State 實作 `IState<TOwner>`，使無效的初始 State 在編譯期被攔截
4. THE 型別系統 SHALL 確保開發者無法將 `IState<MachineA>` 的實例用於 `MachineB` 的 TransitionTo 呼叫或作為 `MachineB` 的初始 State

### Requirement 6: Initial State Configuration

**User Story:** As a developer, I want the initial state to be mandatory at Machine creation time, so that the Machine always has a valid initial state and eliminates the possibility of starting without one.

#### Acceptance Criteria

1. THE `CreateMachine<TMachine>(IState<TOwner> initialState)` SHALL 要求傳入一個非 null 的初始 State 實例作為必要參數
2. WHEN Machine 啟動時，THE Machine SHALL 進入建立時傳入的初始 State 並執行其 Start 方法
3. IF 傳入 `CreateMachine` 的 initialState 參數為 null，THEN THE FsmManager SHALL 拋出 `ArgumentNullException` 且不建立 Machine
4. THE 泛型型別約束 SHALL 限制 initialState 參數僅接受實作 `IState<TOwner>` 的 State 實例，確保無效的初始 State 在編譯期被攔截
