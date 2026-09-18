# Implementation Plan: Event System

## Overview

本實作計畫將 Event System 設計轉換為漸進式的程式撰寫步驟。實作語言為 C#（Unity 6000.5.7f1），程式碼放置於 `Assets/Scripts/Utils/EventSystem`，namespace 為 `Utils.EventSystem`。系統包含三個核心元件：`EventType` enum、`EventArgs` struct、`EventManager` 單例（繼承既有的 `Utils.Singleton.Singleton<T>`）。

實作順序遵循由內而外的組裝原則：先建立無依賴的 `EventType` 與 `EventArgs`，再實作依賴兩者的 `EventManager`，最後串接生命週期與派發機制。測試依專案規範拆分為「測試撰寫」子任務（標記為 optional `*`）與「測試執行」（併入 checkpoint 任務）。

實作參照：

- 原始碼 namespace：`Utils.EventSystem`
- 原始碼位置：`Assets/Scripts/Utils/EventSystem/`（歸屬既有 `Utils` 組件）
- 測試位置：既有 `Utils.Test` 組件（`Assets/Scripts/Utils/Singleton/Tests/` 同層的測試組件），測試 namespace 為 `Utils.EventSystem.Tests`
- 測試框架：NUnit + FsCheck 3.X（`FsCheck.Fluent`，直接呼叫 FsCheck API，不使用 FsCheck.NUnit）
- FsCheck、NSubstitute 等套件已存在於專案，無需安裝

## Tasks

- [x] 1. 建立 EventType 列舉與 EventArgs 值型別結構
  - [x] 1.1 建立 EventType enum
    - 在 `Assets/Scripts/Utils/EventSystem/EventType.cs` 建立 `EventType` enum，namespace 為 `Utils.EventSystem`
    - 定義 `None = 0` 及範例成員（`GameStart`、`GameEnd`、`PlayerDeath`、`SceneLoaded`）
    - 撰寫 XML 文件註解，說明新增事件僅需新增成員並重新編譯
    - _Requirements: 1.1, 1.2, 1.3_

  - [x] 1.2 實作 EventArgs struct 的欄位與建構子
    - 在 `Assets/Scripts/Utils/EventSystem/EventArgs.cs` 建立 `EventArgs` struct，namespace 為 `Utils.EventSystem`
    - 定義專用欄位（`_int0/_int1`、`_long0/_long1`、`_float0/_float1`、`_bool0/_bool1`）與 `_params` (object[])
    - 實作 `params object[]` 建構子（null 時退回 `Array.Empty<object>()`）
    - 實作 `ParamCount` 屬性（`_params != null` 時回傳 `_params.Length`，否則 0）
    - _Requirements: 2.1, 2.3, 2.7_

  - [x] 1.3 實作 EventArgs 的建構工廠與 Setter 方法
    - 實作靜態工廠 `Create(int/long/float/bool, ... = default)`（專用欄位路徑，零堆積配置）
    - 實作 `SetInt`/`SetLong`/`SetFloat`/`SetBool`（v0, v1 = default）支援混合型別組合
    - 實作 `SetParams(params object[])` 覆寫通用路徑
    - _Requirements: 2.1, 2.2, 2.8_

  - [x] 1.4 實作 EventArgs 的型別安全存取器
    - 實作 `GetInt`/`GetLong`/`GetFloat`/`GetBool`（index 僅支援 0/1，越界回傳 default + `Debug.LogWarning`）
    - 實作泛型 `Get<T>(int index)`：專用欄位模式警告、索引越界回傳 default + 警告、null 回傳 default 不警告、`is T` 相容回傳值、型別不匹配回傳 default + 警告
    - _Requirements: 2.4, 2.5, 2.6, 2.9, 7.1, 7.2_

- [x] 2. Checkpoint - EventArgs 測試
  - [x] 2.1 撰寫 EventArgs 的 FsCheck 產生器與 property 測試
    - 在 `Utils.Test` 組件建立 `EventArgsPropertyTests.cs`，namespace `Utils.EventSystem.Tests`
    - 建立產生器：`GenEventArgsDedicated`、`GenEventArgsSetters`、`GenEventArgsGeneric`（型別含 int/long/float/bool/string/Vector2/Vector3/object/null，長度 0～12）
    - **Property 1: EventArgs Round-Trip** — 專用欄位、Setter、通用路徑往返一致且 `ParamCount` 正確
    - **Property 2: Type Mismatch Returns Default** — 不匹配型別呼叫 `Get<T>` 回傳 `default(T)`
    - **Property 3: Out-of-Bounds Returns Default** — 越界索引回傳 `default(T)` 且不拋例外
    - 每個 property 最少 100 次迭代，以註解標記對應設計屬性
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.9, 7.1, 7.2**

  - [x] 2.2 撰寫 EventArgs 的 unit 測試
    - 建立 `EventArgsUnitTests.cs`，namespace `Utils.EventSystem.Tests`
    - 測試 `SetInt/SetLong/SetFloat/SetBool` 單/雙參數設定、混合型別組合存取、`SetParams` 覆寫、null 存入取出無警告、GetInt 系列越界
    - _Requirements: 2.2, 2.9_

  - [x] 2.3 執行 EventArgs 測試並確認通過
    - 於 Unity Test Runner 執行 EventArgs 相關 property 與 unit 測試
    - Ensure all tests pass, ask the user if questions arise.

- [x] 3. 實作 EventManager 生命週期與內部狀態
  - [ ] 3.1 建立 EventManager 骨架與生命週期
    - 在 `Assets/Scripts/Utils/EventSystem/EventManager.cs` 建立 `EventManager : Singleton<EventManager>`，namespace `Utils.EventSystem`
    - 定義 `_listeners`、`_dispatchDepth`、`_pendingOps` 欄位與 `IsReady` 屬性
    - 定義巢狀 `PendingOpType` enum 與 `PendingOperation` struct
    - 實作 `Init()`（重複呼叫時 `Debug.LogWarning` 並忽略）與 `Release()`（清除集合、`IsReady = false`）
    - _Requirements: 6.1, 6.2, 6.3, 6.5_

  - [ ] 3.2 實作 EventType 驗證與 not-ready 防護輔助方法
    - 實作 `IsValidEventType(EventType)`（`Enum.IsDefined`）
    - 建立公開 API 進入點的共用防護：`IsReady == false` 或非法 EventType 時忽略操作並以 `Debug.LogWarning` 記錄操作名稱與傳入值
    - _Requirements: 1.4, 6.4, 7.3_

- [x] 4. 實作訂閱與退訂
  - [ ] 4.1 實作 Subscribe 與 Unsubscribe
    - 實作 `Subscribe`：null listener 忽略 + 警告；重複註冊以 `Contains` 忽略；派發中（`_dispatchDepth > 0`）加入 `PendingOp(Add)`，否則立即註冊
    - 實作 `Unsubscribe`：null listener 忽略 + 警告；未註冊靜默忽略；派發中加入 `PendingOp(Remove)`，否則立即移除
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 4.1, 4.2, 4.3, 4.5, 4.6_

  - [ ] 4.2 實作 UnsubscribeAll 與 ApplyPendingChanges
    - 實作 `UnsubscribeAll`：派發中加入 `PendingOp(RemoveAll)`，否則立即清除該事件監聽者
    - 實作 `ApplyPendingChanges()`：依序套用 `_pendingOps` 中的 Add/Remove/RemoveAll 後清空佇列
    - _Requirements: 4.4, 3.6, 4.6_

- [x] 5. 實作事件派發並串接整體流程
  - [ ] 5.1 實作 Dispatch 多載與快照派發
    - 實作 `Dispatch(EventType)` 無參數多載（轉呼叫帶 `default(EventArgs)` 的多載）
    - 實作 `Dispatch(EventType, EventArgs)`：防護檢查 → `_dispatchDepth++` → 建立監聽者快照（`ToArray`）→ 依註冊順序呼叫 → 每個 listener 以 try/catch 包裹，例外時 `Debug.LogError`（含事件類型與方法名稱）後繼續 → `_dispatchDepth--` → 深度歸零時呼叫 `ApplyPendingChanges()`
    - 確保無監聽者時正常結束、支援巢狀派發、公開 API 不向呼叫端拋出例外
    - _Requirements: 2.8, 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 7.4, 7.5_

- [x] 6. Checkpoint - EventManager 測試
  - [x] 6.1 撰寫 EventManager 訂閱與派發的 property 測試
    - 建立 `EventManagerPropertyTests.cs`，namespace `Utils.EventSystem.Tests`
    - 於 `[SetUp]` 重置 `EventManager` 單例（參照既有 `SingletonTestHelper.ResetInstance`）並 `Init()`
    - 建立產生器：`GenEventType`、`GenInvalidEventType`、`GenListenerCount`、`GenExceptionIndex`
    - **Property 4: Subscribe Implies Notification** — _Validates: Requirements 3.1, 3.2, 3.4, 5.1_
    - **Property 5: Unsubscribe Removes Notification** — _Validates: Requirements 4.1, 4.2_
    - **Property 6: Idempotent Subscription** — _Validates: Requirements 3.3_
    - **Property 7: Registration-Order Dispatch** — _Validates: Requirements 3.2, 5.2_
    - **Property 8: UnsubscribeAll Clears Event** — _Validates: Requirements 4.4_
    - 每個 property 最少 100 次迭代，以註解標記對應設計屬性

  - [x] 6.2 撰寫 EventManager 派發中變更與防護的 property 測試
    - **Property 9: Deferred Subscribe During Dispatch** — _Validates: Requirements 3.6_
    - **Property 10: Deferred Unsubscribe During Dispatch** — _Validates: Requirements 4.6_
    - **Property 11: Exception Isolation** — _Validates: Requirements 5.4, 7.4, 7.5_
    - **Property 12: Invalid Enum Rejection** — _Validates: Requirements 1.4, 7.3_
    - **Property 13: Not-Ready Guard** — _Validates: Requirements 6.3, 6.4_
    - 每個 property 最少 100 次迭代，以註解標記對應設計屬性

  - [x] 6.3 撰寫 EventManager 的 unit 測試
    - 建立 `EventManagerUnitTests.cs`，namespace `Utils.EventSystem.Tests`
    - 測試無參數 Dispatch、null listener 忽略（Subscribe/Unsubscribe）、Unsubscribe 未註冊靜默、無監聽者 Dispatch、巢狀 Dispatch、Init 後 IsReady、Release 後 IsReady、重複 Init 被忽略
    - _Requirements: 2.8, 3.5, 4.3, 4.5, 5.3, 5.5, 6.2, 6.3, 6.5_

  - [x] 6.4 執行 EventManager 測試並確認通過
    - 於 Unity Test Runner 執行 EventManager 相關 property 與 unit 測試
    - Ensure all tests pass, ask the user if questions arise.

- [ ] 7. Final checkpoint - 整體驗證
  - 於 Unity Test Runner 執行全部 Event System 測試（EventArgs + EventManager），確認全數通過且無編譯錯誤
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- 標記 `*` 的子任務為 optional（測試撰寫），可為快速 MVP 而略過；測試執行併入 checkpoint 任務（非 optional）以符合專案測試處理規範。
- 測試撰寫（`*`）與測試執行（checkpoint）刻意分離，避免測試失敗阻擋測試實作。
- 每個任務標註對應的 requirements 子項以利追溯。
- Property tests 驗證設計文件中的 13 個通用正確性屬性，unit tests 驗證具體場景與邊界條件。
- FsCheck 3.X、NSubstitute 等測試套件已存在於專案，若需其他套件請通知使用者手動安裝（勿自行安裝）。
- 測試方法不使用 `Category` attribute。
- 所有原始碼與測試遵循專案 StyleCop 規範與 Traditional Chinese 註解慣例。

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["1.3"] },
    { "id": 2, "tasks": ["1.4", "3.1"] },
    { "id": 3, "tasks": ["2.1", "2.2", "3.2"] },
    { "id": 4, "tasks": ["4.1"] },
    { "id": 5, "tasks": ["4.2"] },
    { "id": 6, "tasks": ["5.1"] },
    { "id": 7, "tasks": ["6.1", "6.2", "6.3"] }
  ]
}
```
