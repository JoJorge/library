# Requirements Document

## Introduction

本文件定義集中式事件系統（Event System）的需求。此系統採用發布/訂閱（Pub-Sub）模式，作為遊戲內各系統之間解耦通訊的全域事件匯流排。事件以列舉（Enum）識別，每個事件可攜帶不定數量的參數，參數支援常見的值型別（int、float、bool、string、Vector2、Vector3 等）以及通用的參考型別。任何系統皆可向事件系統註冊或解除註冊事件監聽，亦可發送事件，無需直接依賴其他系統。事件系統以單例（Singleton）模式實作為全域 EventManager，統一管理事件生命週期。

## Glossary

- **Event_System**: 事件系統，負責管理事件註冊、解除註冊與發送的核心模組
- **Event_Manager**: 事件管理器，事件系統的單例入口，提供全域存取點
- **Event_Type**: 事件類型列舉，以 enum 定義所有可用的事件識別碼
- **Event_Args**: 事件參數，攜帶事件附帶資料的容器，支援不定數量的值型別與參考型別參數
- **Event_Listener**: 事件監聽者，訂閱特定 Event_Type 的回呼委派（delegate）
- **Value_Type_Param**: 值型別參數，包含 int、long、float、bool、string、Vector2、Vector3 等常見值型別
- **Ref_Type_Param**: 參考型別參數，以 object 型別傳遞的通用參考物件
- **Dispatch**: 發送事件，將指定 Event_Type 與其參數廣播至所有已註冊的監聯者
- **Subscribe**: 訂閱，將 Event_Listener 註冊至指定 Event_Type
- **Unsubscribe**: 取消訂閱，將 Event_Listener 從指定 Event_Type 移除

## Requirements

### Requirement 1: Event Identification

**User Story:** 作為開發者，我想要以列舉值識別事件類型，以便在編譯期即可驗證事件名稱正確性，避免字串拼寫錯誤。

#### Acceptance Criteria

1. THE EventManager SHALL 以 EventType 列舉值作為事件的唯一識別方式，不提供任何以字串或整數直接識別事件的公開 API
2. WHEN 發送或訂閱事件時，THE EventManager SHALL 於公開方法簽章中以 EventType 列舉型別作為參數，使傳入非 EventType 型別的值在編譯期產生型別錯誤
3. THE EventType SHALL 定義為獨立的 enum，新增事件類型僅需在該 enum 中新增成員並重新編譯，無需修改 EventManager 類別的原始碼
4. IF 傳入的 EventType 值未定義於列舉中（透過強制轉型產生的非法整數值），THEN THE EventManager SHALL 不執行該次發送或訂閱操作，並透過 Unity Debug.LogWarning 輸出包含該非法數值的警告訊息

### Requirement 2: Event Parameter Support

**User Story:** 作為開發者，我想要事件能攜帶不定數量的參數且支援多種型別，以便靈活傳遞事件相關資料。

#### Acceptance Criteria

1. THE Event_Args SHALL 支援攜帶零個至最多 12 個參數
2. THE Event_Args SHALL 支援以下值型別參數：int、long、float、bool、string、Vector2、Vector3
3. THE Event_Args SHALL 支援以 object 型別傳遞通用參考型別參數，包含 null 值
4. THE Event_Args SHALL 提供泛型方法 Get<T>(int index)，以型別安全方式取得指定索引位置的參數值
5. IF 呼叫端以不匹配的型別呼叫 Get<T>，THEN THE Event_Args SHALL 回傳 default(T) 並透過 Debug.LogWarning 記錄包含索引與期望型別的警告訊息
6. IF 呼叫端以負數索引或大於等於參數數量的索引呼叫 Get<T>，THEN THE Event_Args SHALL 回傳 default(T) 並透過 Debug.LogWarning 記錄包含索引與參數數量的警告訊息
7. THE Event_Args SHALL 提供公開的 Count 屬性，回傳目前攜帶的參數數量
8. THE Event_System SHALL 提供不需 Event_Args 的 Dispatch 多載方法，允許以無參數方式發送事件
9. IF 以 object 型別存入 null 值後以參考型別呼叫 Get<T>，THEN THE Event_Args SHALL 回傳 null 且不記錄警告

### Requirement 3: Event Subscription

**User Story:** 作為開發者，我想要任意系統皆可註冊事件監聽，以便在感興趣的事件發生時收到通知。

#### Acceptance Criteria

1. THE Event_Manager SHALL 提供公開的 Subscribe 方法，接受 Event_Type 與 Event_Listener 委派，將該監聽者註冊至指定事件
2. THE Event_Manager SHALL 允許同一 Event_Type 註冊多個不同的 Event_Listener，且各監聽者於事件發送時皆會被通知
3. IF 同一個 Event_Listener 實例對同一 Event_Type 重複註冊，THEN THE Event_Manager SHALL 忽略重複註冊，確保同一監聽者對同一事件僅被呼叫一次
4. WHEN Subscribe 被呼叫時，THE Event_Manager SHALL 以同步方式立即完成註冊，使得該次呼叫返回後，後續對相同 Event_Type 的 Dispatch 即會通知該監聽者
5. IF Subscribe 時傳入 null 的 Event_Listener，THEN THE Event_Manager SHALL 忽略該操作並透過 Debug.LogWarning 記錄指出哪個參數為 null 的警告訊息
6. IF 在事件派發過程中對同一 Event_Type 呼叫 Subscribe 註冊新監聽者，THEN THE Event_Manager SHALL 完成註冊，但該新監聽者不參與當次正在進行的派發，於下次該事件派發時才會被通知

### Requirement 4: Event Unsubscription

**User Story:** 作為開發者，我想要能夠取消事件監聽，以便在系統停用或銷毀時避免記憶體洩漏或無效回呼。

#### Acceptance Criteria

1. THE Event_Manager SHALL 提供公開的 Unsubscribe 方法，接受 Event_Type 與 Event_Listener 委派，將該監聽者從指定事件移除
2. WHEN Unsubscribe 被呼叫後，THE Event_Manager SHALL 確保該監聽者不再收到對應 Event_Type 的後續事件通知
3. IF Unsubscribe 時指定的 Event_Listener 並未註冊於該 Event_Type，THEN THE Event_Manager SHALL 忽略該操作，不拋出例外
4. THE Event_Manager SHALL 提供 UnsubscribeAll 方法，接受 Event_Type，移除該事件下所有已註冊的監聽者
5. IF Unsubscribe 時傳入 null 的 Event_Listener，THEN THE Event_Manager SHALL 忽略該操作並透過 Debug.LogWarning 記錄警告訊息
6. IF 在事件派發過程中對同一 Event_Type 呼叫 Unsubscribe，THEN THE Event_Manager SHALL 標記該監聽者為待移除，當次派發仍會呼叫該監聽者，於派發結束後完成實際移除

### Requirement 5: Event Dispatching

**User Story:** 作為開發者，我想要能夠從任何系統發送事件，以便通知所有已訂閱該事件的監聽者。

#### Acceptance Criteria

1. THE Event_Manager SHALL 提供公開的 Dispatch 方法，接受 Event_Type 與可選的 Event_Args，將事件同步廣播至所有已註冊該 Event_Type 的 Event_Listener
2. WHEN Dispatch 被呼叫時，THE Event_Manager SHALL 依照監聽者的註冊順序依序呼叫所有已註冊的 Event_Listener
3. WHEN Dispatch 被呼叫且該 Event_Type 無任何已註冊的監聽者時，THE Event_Manager SHALL 正常結束操作，不拋出例外
4. IF 某個 Event_Listener 在執行過程中拋出例外，THEN THE Event_Manager SHALL 捕獲該例外並透過 Debug.LogError 記錄包含事件類型與監聽者方法名稱的錯誤訊息，然後繼續呼叫剩餘的監聽者
5. WHEN 事件正在 Dispatch 過程中，IF 監聽者回呼內觸發另一次 Dispatch（巢狀派發），THEN THE Event_Manager SHALL 正確處理巢狀派發且不產生無限遞迴以外的錯誤
6. THE Event_Manager SHALL 在派發開始時建立當次派發的監聽者快照，確保派發過程中的 Subscribe 或 Unsubscribe 不影響當次遍歷的一致性

### Requirement 6: Event System Lifecycle

**User Story:** 作為開發者，我想要事件系統具備明確的初始化與釋放流程，以便在遊戲生命週期中正確管理資源。

#### Acceptance Criteria

1. THE Event_Manager SHALL 以單例模式實作，繼承專案的抽象單例基底類別，提供全域唯一的存取點
2. WHEN Event_Manager 的 Init 方法被呼叫時，THE Event_Manager SHALL 建立內部的事件註冊表（Dictionary 結構）並將系統狀態標記為就緒（IsReady = true）
3. WHEN Event_Manager 的 Release 方法被呼叫時，THE Event_Manager SHALL 清除所有已註冊的事件監聽者、釋放內部集合資源，並將系統狀態標記為未就緒（IsReady = false）
4. IF 外部系統在 Event_Manager 狀態為未就緒時（IsReady = false）嘗試 Subscribe、Unsubscribe 或 Dispatch，THEN THE Event_Manager SHALL 不執行該操作並透過 Debug.LogWarning 輸出包含操作名稱的警告訊息
5. IF Init 被重複呼叫（系統已處於就緒狀態），THEN THE Event_Manager SHALL 忽略重複初始化並透過 Debug.LogWarning 輸出警告訊息

### Requirement 7: Type Safety and Error Handling

**User Story:** 作為開發者，我想要事件系統具備型別安全機制與完善的錯誤處理，以便在開發期間快速定位問題。

#### Acceptance Criteria

1. THE Event_Args SHALL 在存取參數時進行型別檢查，僅當請求型別與儲存值的實際型別完全一致或具繼承關係時視為相容，否則視為型別轉換失敗
2. IF 型別轉換失敗，THEN THE Event_Args SHALL 回傳該型別的預設值（default(T)）並透過 Debug.LogWarning 記錄警告訊息，內容包含事件類型名稱、參數索引與期望型別名稱
3. IF 公開 API 接收到無效參數（null 參照於不可為 null 的參數、超出定義範圍的列舉值），THEN THE Event_System SHALL 不執行該操作並透過 Debug.LogWarning 記錄錯誤訊息，內容包含 API 方法名稱、參數名稱與傳入值
4. WHEN 事件派送過程中任一訂閱者的回呼擲出例外，THEN THE Event_System SHALL 捕捉該例外、透過 Debug.LogError 記錄包含事件類型名稱與訂閱者方法名稱的錯誤訊息，並繼續派送至剩餘的訂閱者
5. THE Event_System SHALL 確保所有公開 API 呼叫皆不會因內部錯誤而向呼叫端擲出未處理的例外，所有內部例外須於系統邊界內被捕捉並以 Debug.LogError 記錄

### Requirement 8: Event System Testing

**User Story:** 作為開發者，我想要能夠測試事件系統的正確性，以便確保事件的註冊、發送與參數傳遞皆符合預期。

#### Acceptance Criteria

1. THE 測試套件 SHALL 驗證對同一 Event_Type 註冊至少 3 個不同的 Event_Listener 後發送事件，所有監聽者均依照註冊順序被呼叫且各自僅被呼叫一次
2. THE 測試套件 SHALL 驗證對已 Unsubscribe 的 Event_Listener 發送對應 Event_Type 事件時，該監聽者的呼叫次數為零
3. THE 測試套件 SHALL 以 FsCheck 屬性基礎測試驗證 Event_Args 攜帶由 int、float、bool、string、Vector2、Vector3 任意組合產生的參數序列時，以對應型別與索引取出的值等於放入的值（往返屬性）
4. THE 測試套件 SHALL 驗證以負數索引或大於等於參數數量的索引存取 Event_Args 時，回傳該請求型別的 default 值且不拋出例外；以不匹配的型別存取有效索引時，同樣回傳該請求型別的 default 值且不拋出例外
5. THE 測試套件 SHALL 驗證同一個 Event_Listener 實例對同一 Event_Type 呼叫 Subscribe 兩次後發送事件，該監聽者僅被呼叫一次
6. THE 測試套件 SHALL 驗證在 Dispatch 遍歷監聽者過程中，於回呼內執行 Subscribe 或 Unsubscribe 操作時，Dispatch 不拋出集合修改例外且所有原已註冊的剩餘監聽者仍完成呼叫
7. THE 測試套件 SHALL 驗證 Event_Manager 未執行 Init 或已執行 Release 時，呼叫 Subscribe、Unsubscribe 與 Dispatch 均無操作效果、不拋出例外且產生 Debug.LogWarning 輸出
