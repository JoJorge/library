# Requirements Document

## Introduction

本文件定義以 Unity Input System 為基礎的輸入管理系統需求。此系統負責統一管理玩家輸入，包含即時偵測輸入裝置來源（鍵盤滑鼠 / 搖桿）、動態按鍵綁定、按鍵衝突檢查，以及基於情境（Context）的輸入事件分發架構。系統設計目標為高擴充性、低耦合，使不同遊戲情境（如主選單、遊戲中、暫停選單）的輸入處理邏輯彼此獨立，方便後續維護與新增。

## Glossary

- **Input_System_Manager**: 輸入系統管理器，負責統籌所有輸入相關功能的核心管理類別
- **Input_Device_Type**: 輸入裝置類型，列舉值包含 KeyboardMouse（鍵盤滑鼠）與 Gamepad（搖桿）
- **Device_Detector**: 裝置偵測器，負責即時偵測最後使用的輸入裝置來源
- **Binding_Manager**: 按鍵綁定管理器，負責動態按鍵綁定與綁定資料的儲存管理
- **Binding_Profile**: 綁定設定檔，儲存特定裝置類型的完整按鍵綁定配置
- **Conflict_Checker**: 衝突檢查器，負責檢查按鍵綁定是否存在衝突
- **Binding_Conflict**: 綁定衝突，指同一裝置類型下兩個或以上的功能綁定到相同按鍵的情況
- **Action_Name**: 動作名稱，代表遊戲中某個功能對應的輸入動作識別名稱（如 "Jump"、"Attack"）
- **Input_Context**: 輸入情境，代表遊戲中某個特定狀態或場景的輸入處理範疇（如主選單、遊戲中、暫停選單）
- **Context_Handler**: 情境處理器，負責處理特定 Input_Context 下所有輸入事件的獨立模組
- **Input_Action_Asset**: Unity Input System 的 InputActionAsset，定義所有輸入動作的資產檔案
- **Action_Map**: Unity Input System 的 ActionMap，將相關的輸入動作分組管理

## Requirements

### Requirement 1: Input Device Detection

**User Story:** 作為開發者，我想要即時偵測玩家最後使用的輸入裝置類型，以便根據裝置類型切換 UI 提示或調整遊戲行為。

#### Acceptance Criteria

1. WHEN 玩家透過鍵盤按鍵按下、滑鼠按鍵按下、滑鼠滾輪滾動、或滑鼠移動距離超過閥值產生輸入時，THE Device_Detector SHALL 將目前裝置類型更新為 KeyboardMouse
2. WHEN 玩家透過搖桿按鍵按下、或搖桿類比軸位移絕對值超過閥值產生輸入時，THE Device_Detector SHALL 將目前裝置類型更新為 Gamepad
3. WHEN 輸入裝置類型發生切換時，THE Device_Detector SHALL 依註冊順序觸發裝置切換事件通知所有已註冊的回呼，並傳入切換後的裝置類型
4. THE Device_Detector SHALL 提供公開的回呼註冊方法，允許外部訂閱裝置切換事件
5. THE Device_Detector SHALL 提供公開的回呼取消註冊方法，允許外部取消訂閱裝置切換事件
6. THE Device_Detector SHALL 提供公開的 API 供外部查詢目前的輸入裝置類型
7. WHEN 同一裝置類型連續產生多次輸入時，THE Device_Detector SHALL 不重複觸發裝置切換事件
8. WHEN Device_Detector 初始化完成且尚未偵測到任何輸入時，THE Device_Detector SHALL 將目前裝置類型預設為 KeyboardMouse

### Requirement 2: Dynamic Key Binding

**User Story:** 作為玩家，我想要能夠自訂按鍵配置，以便使用最舒適的操作方式遊玩。

#### Acceptance Criteria

1. THE Binding_Manager SHALL 支援在執行時期動態修改任意 Action_Name 的按鍵綁定
2. THE Binding_Manager SHALL 將鍵盤滑鼠的綁定設定與搖桿的綁定設定分別儲存為獨立的 Binding_Profile
3. WHEN 動態綁定完成時，THE Binding_Manager SHALL 在同一幀內將新綁定套用至對應的 Input_Action_Asset，使後續輸入事件立即反映新配置
4. THE Binding_Manager SHALL 提供儲存目前綁定設定的方法，將 Binding_Profile 持久化至儲存媒體
5. IF 儲存操作失敗（例如寫入錯誤），THEN THE Binding_Manager SHALL 保留記憶體中的現有綁定設定不變，並回傳操作失敗結果供呼叫端處理
6. THE Binding_Manager SHALL 提供載入已儲存綁定設定的方法，從儲存媒體還原 Binding_Profile
7. IF 載入的綁定設定格式無效或資料損毀（無法反序列化為合法的 Binding_Profile），THEN THE Binding_Manager SHALL 回退至預設綁定設定並記錄警告訊息
8. WHEN 應用程式啟動且儲存媒體中存在已儲存的 Binding_Profile 時，THE Binding_Manager SHALL 自動載入該設定並套用至 Input_Action_Asset
9. THE Binding_Manager SHALL 提供重置特定 Action_Name 綁定至預設值的方法
10. THE Binding_Manager SHALL 提供重置特定裝置類型所有綁定至預設值的方法

### Requirement 3: Binding Conflict Detection

**User Story:** 作為開發者，我想要能夠檢查按鍵綁定是否存在衝突，以便在綁定時及時提醒玩家或進行自動處理。

#### Acceptance Criteria

1. THE Conflict_Checker SHALL 提供公開的 API，接受指定的 Action_Name 與按鍵組合（Binding Path），回傳該綁定是否與同 Input_Context 且同裝置類型下的其他綁定產生 Binding_Conflict；若無衝突，SHALL 回傳空的衝突清單
2. WHEN Binding_Conflict 被偵測到時，THE Conflict_Checker SHALL 回傳所有在相同 Input_Context 與相同裝置類型範圍內，綁定至相同 Binding Path 的衝突 Action_Name 清單
3. WHEN 動態綁定完成後，THE Conflict_Checker SHALL 自動執行衝突檢查，並透過事件（Event）通知訂閱者衝突結果，該事件 SHALL 包含觸發綁定的 Action_Name 與衝突 Action_Name 清單
4. THE Conflict_Checker SHALL 僅在相同 Input_Context 與相同裝置類型的範圍內判定衝突；兩個 Binding Path 完全相同（字串相等）時視為衝突
5. THE Conflict_Checker SHALL 提供全域衝突掃描方法，接受指定裝置類型，回傳該裝置類型下所有存在衝突的 Binding Path 及其對應的衝突 Action_Name 群組清單
6. IF 傳入的 Action_Name 未註冊於任何 Input_Context 中，THEN THE Conflict_Checker SHALL 回傳錯誤指示，表明該 Action_Name 不存在

### Requirement 4: Binding Query

**User Story:** 作為開發者，我想要能夠查詢特定功能目前綁定的按鍵，以便在 UI 上顯示正確的按鍵提示。

#### Acceptance Criteria

1. THE Binding_Manager SHALL 提供公開的查詢 API，接受 Action_Name 與 Input_Device_Type，回傳該功能在指定裝置類型下所有綁定的按鍵顯示名稱清單（即 Unity InputBinding 的 display string）
2. THE Binding_Manager SHALL 提供公開的查詢 API，接受 Action_Name 與 Input_Device_Type，回傳該功能在指定裝置類型下所有綁定的按鍵路徑（binding path）清單
3. IF 查詢的 Action_Name 在已註冊的 Action 中不存在，THEN THE Binding_Manager SHALL 回傳空集合，且不拋出未處理的例外
4. IF 查詢的 Action_Name 存在但該 Input_Device_Type 下無任何綁定，THEN THE Binding_Manager SHALL 回傳空集合
5. IF 傳入的 Input_Device_Type 為未定義的列舉值，THEN THE Binding_Manager SHALL 回傳空集合，且不拋出未處理的例外
6. WHEN 綁定被動態修改完成後，THE Binding_Manager SHALL 確保同一幀內的後續查詢即回傳更新後的綁定資訊

### Requirement 5: Context-Based Input Architecture

**User Story:** 作為開發者，我想要將不同遊戲情境的輸入處理邏輯拆分為獨立的模組，以便在新增情境時不影響現有的輸入處理邏輯。

#### Acceptance Criteria

1. THE Input_System_Manager SHALL 支援註冊 1 至 32 個 Context_Handler，每個 Context_Handler 對應一個獨立的 Input_Context
2. WHEN 遊戲切換至特定 Input_Context 時，THE Input_System_Manager SHALL 先停用當前啟用的 Context_Handler，再啟用目標 Input_Context 對應的 Context_Handler，確保同一時間僅有一個 Context_Handler 處於啟用狀態
3. THE Context_Handler SHALL 透過 C# 事件（event）發布該情境下的輸入事件，供任何外部系統透過標準事件訂閱機制進行訂閱
4. WHEN Context_Handler 被停用時，THE Context_Handler SHALL 立即停止發布所有輸入事件，已停用後收到的輸入動作不得觸發事件發布
5. WHEN Context_Handler 被啟用時，THE Context_Handler SHALL 開始監聽對應 Action_Map 中的輸入動作並發布事件
6. THE Input_System_Manager SHALL 提供新增 Context_Handler 的擴充機制，新增時不需修改現有 Context_Handler 的程式碼
7. THE Context_Handler SHALL 定義為可獨立實作的介面或抽象類別，供各情境分別實作
8. IF 切換目標的 Input_Context 未註冊任何對應的 Context_Handler，THEN THE Input_System_Manager SHALL 維持當前啟用的 Context_Handler 不變，並透過回傳值或錯誤事件通知呼叫端切換失敗
9. IF 註冊 Context_Handler 時該 Input_Context 已有對應的 Context_Handler，THEN THE Input_System_Manager SHALL 拒絕重複註冊，並透過回傳值或錯誤事件通知呼叫端註冊失敗
10. WHEN Input_System_Manager 初始化完成且尚未執行任何情境切換時，THE Input_System_Manager SHALL 不啟用任何 Context_Handler，所有已註冊的 Context_Handler 皆處於停用狀態

### Requirement 6: Input System Initialization and Lifecycle

**User Story:** 作為開發者，我想要輸入系統能正確初始化與清理資源，以便在遊戲生命週期中穩定運作。

#### Acceptance Criteria

1. WHEN 輸入系統初始化時，THE Input_System_Manager SHALL 載入 Input_Action_Asset 並建立所有已註冊的 Context_Handler，初始化完成後將系統狀態標記為就緒
2. WHEN 輸入系統初始化時，THE Input_System_Manager SHALL 載入已儲存的 Binding_Profile 並套用至對應的 Input_Action_Asset
3. IF 初始化時無已儲存的 Binding_Profile，THEN THE Input_System_Manager SHALL 使用 Input_Action_Asset 內建的預設綁定設定
4. WHEN 輸入系統被銷毀或停用時，THE Input_System_Manager SHALL 釋放所有 Input_Action_Asset 資源、停用所有 Context_Handler、並取消所有事件訂閱
5. IF Input_Action_Asset 無法載入，THEN THE Input_System_Manager SHALL 記錄錯誤訊息並進入停用狀態，於該狀態下拒絕所有輸入操作請求且不啟用任何 Context_Handler
6. IF 外部系統在 Input_System_Manager 尚未完成初始化時嘗試進行輸入相關操作，THEN THE Input_System_Manager SHALL 拒絕該操作並回傳表示系統未就緒的錯誤指示
7. IF Input_System_Manager 在初始化過程中被銷毀，THEN THE Input_System_Manager SHALL 釋放已完成配置的部分資源並取消已建立的事件訂閱
