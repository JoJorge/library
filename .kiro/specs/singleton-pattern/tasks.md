# Implementation Plan: Singleton Pattern

## Overview

實作一個可繼承的泛型抽象單例基底類別 `Singleton<T>`，提供延遲初始化、雙重檢查鎖定（DCL）執行緒安全、反射防護、序列化防護，以及執行緒安全的狀態管理。子類別僅需繼承即可獲得完整單例功能。測試使用 NUnit 3 + FsCheck 進行屬性導向測試與範例導向測試。

## Tasks

- [x] 1. 建立專案結構與核心介面
  - [x] 1.1 建立目錄結構與核心 Singleton\<T\> 抽象基底類別
    - 建立 `Scripts/SingletonPattern/` 目錄
    - 實作 `Singleton<T>` 抽象類別，包含：
      - 泛型型別參數 `T where T : Singleton<T>`
      - `private static volatile T _instance` 欄位
      - `private static readonly object Lock` 鎖定物件
      - `protected` 建構子，含反射防護邏輯（偵測 `_instance != null` 時拋出 `InvalidOperationException`）
      - `public static T Instance` 屬性，含雙重檢查鎖定延遲初始化邏輯
      - 使用 `Activator.CreateInstance(typeof(T), true)` 建立實例
    - 加入完整 XML 文件註解
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 3.3, 3.4, 4.3, 7.1, 7.2, 7.5, 7.6, 7.7_

- [x] 4. 實作測試基礎設施
  - [x] 4.1 建立測試專案結構與輔助工具
    - 建立 `Scripts/SingletonPattern/Tests/` 目錄
    - 建立測試輔助類別，包含：
      - 透過反射重置 `_instance` 靜態欄位的方法（用於 `[SetUp]`）
      - 測試專用的子類別（`TestSingleton`、`AnotherTestSingleton` 等）
      - 含 `State` 屬性的測試子類別（`StatefulSingleton`），用於 Property 5 測試：
        - `volatile string _state` 欄位確保讀取可見性
        - `lock(StateLock)` 確保 setter 原子性
    - 設定 `[assembly: InternalsVisibleTo]` 屬性（若需要）
    - 確認 NUnit 3 與 FsCheck.NUnit 套件參考
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 6.1, 6.2, 6.3, 6.4_

- [x] 5. 實作範例導向測試（Unit Tests）
  - [x] 5.1 實作基本單例行為的單元測試
    - 建立 `Scripts/SingletonPattern/Tests/SingletonUnitTests.cs`
    - 測試項目：
      - 首次存取前 `_instance == null`（透過反射讀取私有欄位驗證延遲初始化）
      - `protected` 建構子存取控制（反射確認修飾詞為 `Family`）
      - `abstract` 類別驗證（`typeof(Singleton<>).IsAbstract == true`）
      - 泛型約束驗證（T 受限為 `Singleton<T>` 子類別）
      - 多次存取回傳同一實例（`ReferenceEquals` 驗證）
      - 子類別繼承後即可使用 `Instance`（無需額外單例邏輯）
    - _Requirements: 5.1, 5.4, 5.5_

  - [x] 5.2 實作反射防護與錯誤處理的單元測試
    - 建立或擴充測試檔案
    - 測試項目：
      - 反射觸發第二次建構子時拋出 `InvalidOperationException`，驗證訊息內容
      - 子類別未提供無參建構子時拋出 `MissingMethodException`
    - _Requirements: 5.4, 4.3_

- [x] 6. 實作屬性導向測試（Property-Based Tests）
  - [x] 6.1 Property 1: Identity Preservation — 多次存取回傳相同實例
    - **Property 1: Identity Preservation — 多次存取回傳相同實例**
    - **Validates: Requirements 1.2, 2.3**
    - 建立 `Scripts/SingletonPattern/Tests/SingletonPropertyTests.cs`
    - 使用 FsCheck 生成隨機存取次數（2～1000）
    - 驗證所有回傳值的 `ReferenceEquals` 關係均成立
    - 標籤：`[Category("Feature: singleton-pattern, Property 1: Identity Preservation")]`

  - [ ]* 6.2 Property 2: Reflection Guard — 建構子防衛子句阻止第二個實例
    - **Property 2: Reflection Guard — 建構子防衛子句阻止第二個實例**
    - **Validates: Requirements 1.4, 4.3**
    - 生成多次反射呼叫嘗試（在實例已存在後）
    - 驗證每次均拋出 `InvalidOperationException`
    - 確認不存在兩個不同的同型別實例參考
    - 標籤：`[Category("Feature: singleton-pattern, Property 2: Reflection Guard")]`

  - [ ]* 6.3 Property 3: Thread Safety — 並發存取只建立一個實例
    - **Property 3: Thread Safety — 並發存取只建立一個實例**
    - **Validates: Requirements 2.4, 3.1, 3.2, 7.6**
    - 生成隨機執行緒數量（2～50）
    - 使用 `ManualResetEventSlim` 同時啟動所有執行緒
    - 驗證建構子呼叫次數為 1 且所有回傳參考相同
    - 標籤：`[Category("Feature: singleton-pattern, Property 3: Thread Safety")]`

  - [ ]* 6.4 Property 4: Serialization Round-Trip — 反序列化保持同一實例
    - **Property 4: Serialization Round-Trip — 反序列化保持同一實例**
    - **Validates: Requirements 4.1, 5.3**
    - 驗證未標記 `[Serializable]` 時，序列化嘗試拋出 `SerializationException`
    - 確認不會因反序列化產生第二個實例
    - 標籤：`[Category("Feature: singleton-pattern, Property 4: Serialization Round-Trip")]`

  - [ ]* 6.5 Property 5: Concurrent State Safety — 並發狀態修改的安全性與可見性
    - **Property 5: Concurrent State Safety — 並發狀態修改的安全性與可見性**
    - **Validates: Requirements 6.1, 6.2, 6.4**
    - 使用測試輔助子類別 `StatefulSingleton`（定義於 Task 4.1）
    - 生成隨機字串集合和執行緒數量
    - 多執行緒並發寫入 `StatefulSingleton.Instance.State`
    - 驗證最終 `State` 值屬於合法寫入集合
    - 標籤：`[Category("Feature: singleton-pattern, Property 5: Concurrent State Safety")]`

  - [ ]* 6.6 Property 6: Per-Subclass Instance Isolation — 子類別實例獨立性
    - **Property 6: Per-Subclass Instance Isolation — 子類別實例獨立性**
    - **Validates: Requirements 7.4**
    - 生成隨機存取順序和組合（多個子類別）
    - 驗證不同子類別的 `Instance` 之間 `!ReferenceEquals` 成立
    - 驗證同一子類別的多次存取滿足 `ReferenceEquals`
    - 標籤：`[Category("Feature: singleton-pattern, Property 6: Per-Subclass Instance Isolation")]`

- [x] 7. Final checkpoint - 確認所有測試通過
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- 以 C# 為實作語言，遵循 Unity 相容的 C# 版本
- 程式碼放置於 `Scripts/SingletonPattern/` 目錄下
- 測試放置於 `Scripts/SingletonPattern/Tests/` 目錄下
- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from design document
- Unit tests validate specific examples and edge cases
- 測試中需透過反射重置靜態欄位，建議在 `[SetUp]` 中執行
- 使用 NUnit 3 + FsCheck.NUnit 作為測試框架

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["4.1"] },
    { "id": 2, "tasks": ["5.1", "5.2"] },
    { "id": 3, "tasks": ["6.1", "6.2", "6.3", "6.4", "6.5", "6.6"] }
  ]
}
```
