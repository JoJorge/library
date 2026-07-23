# Requirements Document

## Introduction

本文件定義通用有限狀態機（FSM）系統的需求。此系統設計為 Unity/C# 環境下的通用框架，支援狀態生命週期管理（Start、Update、FixedUpdate、End），並允許同時運行多個獨立的 Machine。系統旨在提供靈活、可擴展的狀態管理機制，適用於 AI 行為、UI 流程、遊戲邏輯等多種場景。

## Glossary

- **FsmManager**: 有限狀態機系統的頂層管理器，負責建立、管理與更新所有 Machine。
- **Machine**: 一個獨立運行的有限狀態機實例，包含自身的狀態集合、當前狀態，並提供轉換方法供 State 呼叫。
- **State**: FSM 中的一個狀態，具備 Start、Update、FixedUpdate、End 四個生命週期方法。
- **Transition**: 狀態之間的轉換規則，定義從一個 State 到另一個 State 的目標。
- **Current_State**: Machine 當前正在執行的 State。
- **Transition_Function**: Machine 提供的轉換方法，供 State 在適當時機呼叫以觸發狀態切換。
- **Compile-Time State Binding**: State 在編譯期透過泛型型別參數或屬性宣告其所屬的 Machine 型別，Machine 在執行期自動識別所有綁定至自身的 State，無需動態註冊。

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

**User Story:** As a developer, I want each State to be responsible for deciding when to trigger a transition by calling a Machine-provided transition function, so that transition logic is encapsulated within the State itself.

#### Acceptance Criteria

1. THE Machine SHALL 提供一個 Transition_Function，供 Current_State 呼叫以請求轉換至指定的目標 State
2. WHEN Current_State 呼叫 Transition_Function 並指定一個有效的目標 State 時，THE Machine SHALL 依序執行 Current_State 的 End 回呼、更新 Current_State 為指定的目標 State、再執行該目標 State 的 Start 回呼
3. THE Machine SHALL 透過型別系統限制 Transition_Function 的目標 State 參數僅接受編譯期綁定至該 Machine 的 State 型別，使無效的目標 State 在編譯期被攔截而非執行期
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
4. THE 每個 Machine SHALL 擁有獨立的 State 集合，該集合由編譯期綁定至該 Machine 型別的所有 State 決定，不與其他 Machine 共享
5. IF 在 FsmManager 更新迴圈執行期間有新的 Machine 被註冊或移除，THEN THE FsmManager SHALL 在當次更新迴圈完成後才將變更生效，不影響當次迭代
6. WHEN 一個 Machine 的 State 觸發狀態切換時，THE 其他 Machine 的 Current_State 與轉換流程 SHALL 不受影響

### Requirement 4: Machine Lifecycle Management

**User Story:** As a developer, I want to create, start, stop, and remove Machine instances at runtime, so that I can dynamically control state machine lifecycles.

#### Acceptance Criteria

1. WHEN 開發者請求建立新的 Machine 時，THE FsmManager SHALL 建立一個新的 Machine，將其狀態設為「已建立（Created）」，註冊至管理清單，並回傳該實例的參考
2. WHEN 開發者啟動一個狀態為「已建立」或「已停止」的 Machine 時，THE Machine SHALL 將狀態設為「運行中（Running）」，進入其指定的初始 State，並執行該節點的 Start 方法
3. IF 開發者嘗試啟動一個狀態為「運行中」的 Machine，THEN THE FsmManager SHALL 忽略該操作並維持實例目前狀態不變
4. WHEN 開發者停止一個狀態為「運行中」的 Machine 時，THE Machine SHALL 執行 Current_State 的 End 方法、停止接收更新，並將狀態設為「已停止（Stopped）」
5. IF 開發者嘗試停止一個狀態非「運行中」的 Machine，THEN THE FsmManager SHALL 忽略該操作並維持實例目前狀態不變
6. WHEN 開發者移除一個 Machine 時，IF 該實例狀態為「運行中」，THEN THE FsmManager SHALL 先執行停止流程（觸發 Current_State 的 End 方法並停止更新），再將該實例從管理清單中移除並將實例標記為已銷毀（Destroyed）
7. WHEN 開發者移除一個狀態為「已建立」或「已停止」的 Machine 時，THE FsmManager SHALL 將該實例從管理清單中移除並將實例標記為已銷毀

### Requirement 5: Compile-Time State Binding

**User Story:** As a developer, I want States to declare at compile time which Machine they belong to, so that the state set is type-safe, immutable, and fully determined without runtime registration.

#### Acceptance Criteria

1. THE 每個 State SHALL 在編譯期透過泛型型別參數或型別約束宣告其所屬的 Machine 型別
2. WHEN Machine 被實例化時，THE Machine SHALL 自動識別所有在編譯期綁定至該 Machine 型別的 State，無需手動新增
3. THE Machine SHALL 不提供任何在執行期動態新增或移除 State 的方法
4. THE Machine 的狀態集合 SHALL 在編譯期完全確定且在執行期不可變更
5. THE 型別系統 SHALL 確保開發者無法將綁定至其他 Machine 型別的 State 用於非其所屬的 Machine

### Requirement 6: Initial State Configuration

**User Story:** As a developer, I want to set an initial state for each Machine, so that the Machine knows which state to enter when it starts.

#### Acceptance Criteria

1. THE Machine SHALL 提供方法讓開發者設定初始 State
2. WHEN Machine 啟動且已設定初始 State 時，THE Machine SHALL 進入該初始 State 並執行其 Start 方法
3. IF Machine 啟動時未設定初始 State，THEN THE Machine SHALL 回傳錯誤且不開始運行
4. THE 型別系統 SHALL 限制初始 State 的設定僅接受編譯期綁定至該 Machine 的 State 型別，確保無效的初始 State 在編譯期被攔截
5. IF Machine 已處於運行狀態時開發者嘗試變更初始 State，THEN THE Machine SHALL 回傳錯誤且維持原有初始 State 設定不變
