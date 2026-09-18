# Requirements Document

## Introduction

本文件定義 Unity 開發者工具系統「DevSystem」的需求。DevSystem 提供一套統一的開發者除錯與工具框架，讓各開發者能夠在開發期間快速掛載除錯功能，包含：由單一 compilation symbol（DEV_MODE）統一控制的功能開關、分類化的除錯日誌（debug log）輸出與個別開關、開發者專用按鍵綁定（與既有輸入系統整合並進行衝突檢查），以及可程式化擴充的開發者面板 UI 架構（支援分頁與子分頁、最大階層數）。DevSystem 內建兩個分頁：開發者按鍵列表與除錯日誌狀態設定。此系統設計目標為高擴充性、低耦合，且在未定義 DEV_MODE 時完全不編譯、不影響正式版本。

DevSystem 的頂層管理者命名為 DevSystemManager，遵循既有 singleton-pattern spec 的 Singleton<T> 慣例，初始化與釋放 API 分別命名為 Init 與 Release。按鍵綁定功能需與既有 input-system spec 整合，透過該系統的 Binding_Manager 與 Conflict_Checker 進行動態綁定與衝突檢查。

## Glossary

- **DevSystem**: 開發者工具系統整體，涵蓋日誌、按鍵綁定、面板 UI 等所有開發者功能
- **DevSystem_Manager**: DevSystem 的頂層管理器（類別名 DevSystemManager），採 Singleton<T> 慣例，提供 Init 與 Release 生命週期 API
- **DEV_MODE**: 控制 DevSystem 全部程式碼與 API 是否編譯與作用的 compilation symbol（條件編譯符號）；未定義時 DevSystem 相關功能不編譯亦不作用
- **Dev_Logger**: 開發者日誌元件，提供帶分類名稱的 debug log 輸出 API
- **Log_Category**: 日誌分類，以 enum 管理的日誌類別識別值（如 Network、Gameplay、UI）
- **Log_Message_Format**: 日誌訊息格式，固定為 `[Dev][分類] MSG`，其中「分類」為 Log_Category 名稱、MSG 為呼叫端提供的訊息內容
- **Log_Toggle_Config**: 日誌開關設定檔，於執行前提供各 Log_Category 的初始開關狀態；其內容僅於載入時讀取為暫存狀態，DevSystem 在執行前或執行期的任何開關調整皆不會寫回此設定檔，以避免開發者誤將個人設定提交至版本控制
- **Dev_Key_Binder**: 開發者按鍵綁定元件，負責向輸入系統註冊開發者專用按鍵並記錄功能與按鍵的對應關係
- **Dev_Action_Enum**: 開發者動作列舉，識別開發者按鍵功能的 enum，其列舉值名稱皆帶有 `Dev` 前綴以便辨識
- **Dev_Key_Registration**: 開發者按鍵註冊資訊，記錄某個開發者功能（Dev_Action_Enum 值）、其綁定按鍵路徑、以及功能敘述文字的對應紀錄
- **Binding_Manager**: 既有 input-system spec 的按鍵綁定管理器，DevSystem 透過其執行動態按鍵綁定
- **Conflict_Checker**: 既有 input-system spec 的衝突檢查器，DevSystem 透過其對開發者按鍵與一般按鍵進行衝突檢查
- **Normal_Binding**: 一般（非開發者）按鍵綁定，即既有輸入系統中非 DevSystem 註冊的按鍵綁定
- **Dev_Panel**: 開發者面板，DevSystem 提供的通用 UI 容器，以 UI Toolkit 建置
- **Panel_Tab**: 面板分頁，Dev_Panel 中的頂層分類頁籤
- **Panel_Sub_Tab**: 面板子分頁，隸屬於某個 Panel_Tab 或上層 Panel_Sub_Tab 之下的次級頁籤
- **Panel_Max_Depth**: 面板最大階層數，為固定的編譯期常數上限，限制 Panel_Tab 與其下 Panel_Sub_Tab 的巢狀階層深度，不可於執行期變更
- **Dev_Key_List_Tab**: 內建分頁之一，顯示所有開發者按鍵及其對應功能敘述的列表
- **Debug_Log_Settings_Tab**: 內建分頁之一，顯示各 Log_Category 的日誌開關狀態並提供執行期開關設定

## Requirements

### Requirement 1: DEV_MODE Compilation Gating

**User Story:** 作為開發者，我想要透過單一 compilation symbol（DEV_MODE）統一開關整個 DevSystem，以便正式版本不包含任何開發者程式碼與效能負擔。

#### Acceptance Criteria

1. WHERE DEV_MODE 已定義，THE DevSystem SHALL 編譯並提供其所有公開 API
2. WHERE DEV_MODE 未定義，THE DevSystem SHALL 仍可編譯，但其公開 API 方法主體為空實作（no-op），不包含任何開發者功能邏輯
3. WHERE DEV_MODE 未定義且外部程式碼呼叫 DevSystem 的公開 API，THE DevSystem SHALL 使該呼叫成為 no-op（不執行任何方法主體、不修改任何狀態、不產生任何可觀察的輸出或副作用）
4. WHERE DEV_MODE 未定義且外部程式碼呼叫 DevSystem 的公開 API，THE DevSystem SHALL 保持該呼叫端可成功編譯且不產生任何編譯錯誤或警告
5. IF DevSystem 的公開 API 具有回傳值且於 DEV_MODE 未定義時被呼叫，THEN THE DevSystem SHALL 回傳該型別的預設值（default）而不執行任何運算邏輯
6. THE DevSystem SHALL 以 DEV_MODE 作為唯一控制開發者功能編譯與作用的 compilation symbol

### Requirement 2: Categorized Debug Logging

**User Story:** 作為開發者，我想要輸出帶有分類名稱的除錯日誌，以便在日誌中快速辨識訊息來源分類。

#### Acceptance Criteria

1. THE Dev_Logger SHALL 提供公開的 API，接受一個 Log_Category 值與一段訊息字串，輸出一筆除錯日誌
2. WHEN Dev_Logger 輸出日誌時，THE Dev_Logger SHALL 依 Log_Message_Format 格式化訊息為 `[Dev][分類] MSG`，其中「分類」為傳入 Log_Category 的名稱、MSG 為傳入的訊息字串
3. THE Dev_Logger SHALL 以 enum 型別（Log_Category）作為分類參數，使呼叫端僅能指定已定義的分類
4. THE Dev_Logger SHALL 提供擴充機制，使開發者能新增 Log_Category 列舉值而不需修改既有日誌輸出邏輯
5. IF 傳入的訊息字串為 null 或空字串，THEN THE Dev_Logger SHALL 仍輸出一筆日誌，其中 MSG 部分為空字串，並維持 `[Dev][分類] ` 的格式前綴不變
6. IF 傳入的 Log_Category 值不對應任何已定義的列舉成員（例如透過強制轉型傳入超出範圍的整數值），THEN THE Dev_Logger SHALL 輸出一筆日誌，並以固定的預設分類名稱作為「分類」欄位內容，且不拋出例外
7. THE Dev_Logger SHALL 接受訊息字串長度上限為 4096 個字元，WHEN 訊息字串超過該上限時，THE Dev_Logger SHALL 將訊息截斷至 4096 個字元後輸出
8. IF 底層日誌輸出系統本身發生失敗或不可用，THEN THE Dev_Logger SHALL 抑制該筆日誌輸出且不拋出例外（優雅處理，不影響呼叫端）

### Requirement 3: Per-Category Log Toggle

**User Story:** 作為開發者，我想要個別開關各分類的除錯日誌，且我的開關調整僅存於暫存狀態，以便只顯示當前關注的分類訊息，並避免個人設定被誤提交至版本控制。

#### Acceptance Criteria

1. WHILE 某個 Log_Category 的開關為開啟狀態，THE Dev_Logger SHALL 輸出該 Log_Category 的日誌
2. WHILE 某個 Log_Category 的開關為關閉狀態，THE Dev_Logger SHALL 不輸出該 Log_Category 的日誌
3. THE Dev_Logger SHALL 於載入時讀取 Log_Toggle_Config 設定檔，將各 Log_Category 的開關狀態載入為暫存狀態
4. WHEN DevSystem 讀取 Log_Toggle_Config 完成後，THE Dev_Logger SHALL 依該設定檔內容套用各 Log_Category 的暫存開關狀態，將出現於設定中的 Log_Category 設為開啟、未出現於設定中的 Log_Category 設為關閉
5. IF 不存在可用的 Log_Toggle_Config（未提供設定檔），THEN THE Dev_Logger SHALL 將所有 Log_Category 的暫存開關預設為開啟狀態
6. WHILE Log_Toggle_Config 尚未讀取完成，THE Dev_Logger SHALL 將所有 Log_Category 的開關視為開啟狀態並輸出其日誌
7. THE Dev_Logger SHALL 僅將各 Log_Category 的開關狀態維護於暫存狀態，且不論調整發生於執行前或執行期，THE Dev_Logger SHALL 不將任何開關狀態寫回 Log_Toggle_Config 設定檔或任何持久化儲存
8. WHEN 系統啟動時，THE Dev_Logger SHALL 重新依 Log_Toggle_Config 內容套用暫存開關狀態，使前次執行期間的任何開關調整不影響本次啟動的初始狀態
9. THE Dev_Logger SHALL 提供公開的 API，供 Debug_Log_Settings_Tab 於執行期切換任一 Log_Category 的暫存開關狀態
10. WHEN 開發者於執行期透過 Debug_Log_Settings_Tab 切換某個 Log_Category 的開關時，THE Dev_Logger SHALL 於切換後更新該 Log_Category 的暫存開關狀態，並使該切換於後續第一筆該 Log_Category 日誌請求時即生效，決定其是否輸出

### Requirement 4: Dev Key Binding Registration

**User Story:** 作為開發者，我想要透過 API 將開發者功能綁定到按鍵，以便在遊戲執行時以按鍵觸發除錯功能。

#### Acceptance Criteria

1. THE Dev_Key_Binder SHALL 提供公開的 API，接受一個 Dev_Action_Enum 值、目標按鍵路徑（Binding Path）與功能敘述文字，向輸入系統的 Binding_Manager 註冊該開發者按鍵綁定
2. IF 傳入的目標按鍵路徑為空字串或無法被 Binding_Manager 解析為有效的按鍵路徑，THEN THE Dev_Key_Binder SHALL 拒絕該次註冊、不建立任何 Dev_Key_Registration，並回傳表示按鍵路徑無效的錯誤指示
3. WHEN 開發者按鍵綁定完成時，THE Dev_Key_Binder SHALL 建立並儲存一筆 Dev_Key_Registration，記錄該 Dev_Action_Enum 值、綁定按鍵路徑與功能敘述文字的對應關係
4. IF 傳入的 Dev_Action_Enum 值已存在於既有的 Dev_Key_Registration 清單中，THEN THE Dev_Key_Binder SHALL 拒絕該次重複註冊、保留既有的 Dev_Key_Registration 不變，並回傳表示該 Dev_Action_Enum 已註冊的錯誤指示
5. THE Dev_Action_Enum SHALL 以 enum 管理開發者按鍵功能，且每個列舉值名稱皆以 `Dev` 前綴命名以便與一般動作辨識
6. THE Dev_Key_Binder SHALL 提供擴充機制，使開發者能新增 Dev_Action_Enum 列舉值而不需修改既有綁定邏輯
7. WHEN 開發者以某個 Dev_Action_Enum 值呼叫解除綁定 API 時，THE Dev_Key_Binder SHALL 向 Binding_Manager 解除該按鍵綁定並從清單中移除對應的 Dev_Key_Registration
8. IF 解除綁定 API 收到的 Dev_Action_Enum 值不存在於既有的 Dev_Key_Registration 清單中，THEN THE Dev_Key_Binder SHALL 不變更任何既有綁定，並回傳表示查無對應註冊的錯誤指示
9. WHEN Dev_Key_List_Tab 或其他呼叫端呼叫該清單 API 時，THE Dev_Key_Binder SHALL 回傳目前所有已註冊的 Dev_Key_Registration 清單

### Requirement 5: Dev Key Conflict Checking

**User Story:** 作為開發者，我想要在綁定開發者按鍵時檢查與一般按鍵的衝突，以便避免開發者功能覆蓋既有遊戲操作。

#### Acceptance Criteria

1. WHEN 開發者透過 Dev_Key_Binder 綁定按鍵時，THE Dev_Key_Binder SHALL 於寫入 Dev_Key_Registration 之前，透過 Conflict_Checker 對該綁定與所有 Normal_Binding 及既有 Dev_Key_Registration 進行衝突檢查
2. THE Dev_Key_Binder SHALL 以與 Conflict_Checker 相同的衝突判定範圍進行檢查，即在相同 Input_Context 與相同 Input_Device_Type 下，Binding Path 字串以區分大小寫的完整字串相等時視為衝突
3. IF 開發者按鍵綁定與任一 Normal_Binding 或既有 Dev_Key_Registration 產生衝突，THEN THE Dev_Key_Binder SHALL 拒絕該次綁定、不建立 Dev_Key_Registration、不修改任何既有綁定，並回傳包含所有衝突對象清單的檢查結果供呼叫端處理
4. WHEN 衝突檢查完成且衝突清單為空時，THE Dev_Key_Binder SHALL 完成該開發者按鍵的綁定與 Dev_Key_Registration 紀錄
5. IF 綁定請求缺少必要欄位（Input_Context、Input_Device_Type 或 Binding Path），THEN THE Dev_Key_Binder SHALL 不執行衝突檢查、拒絕該次綁定，並回傳表示綁定請求無效的錯誤指示

### Requirement 6: Generic Dev Panel Architecture

**User Story:** 作為開發者，我想要一個支援分頁與子分頁的通用開發者面板架構，以便將不同開發者功能分類組織於同一面板中。

#### Acceptance Criteria

1. THE Dev_Panel SHALL 支援包含 1 至 100 個 Panel_Tab，並以 UI Toolkit 呈現
2. WHEN 開發者選擇某個具有下層 Panel_Sub_Tab 的 Panel_Tab 時，THE Dev_Panel SHALL 顯示該 Panel_Tab 之下的 Panel_Sub_Tab 選項
3. WHEN 開發者選擇某個沒有下層 Panel_Sub_Tab 的 Panel_Tab 或 Panel_Sub_Tab 時，THE Dev_Panel SHALL 顯示該節點的葉節點內容，而非顯示子分頁選項
4. THE Dev_Panel SHALL 以固定的編譯期常數定義 Panel_Max_Depth，作為 Panel_Tab 與 Panel_Sub_Tab 巢狀階層深度的上限，且不提供於執行期變更該上限的 API
5. IF 新增的 Panel_Sub_Tab 階層深度超過 Panel_Max_Depth，THEN THE Dev_Panel SHALL 拒絕該新增操作並保留既有面板結構不變，且回傳表示超過階層上限的錯誤指示
6. WHEN 開發者選擇某個具有下層 Panel_Sub_Tab 的 Panel_Sub_Tab 時，THE Dev_Panel SHALL 顯示其下一層的 Panel_Sub_Tab 選項

### Requirement 7: Declarative Panel Configuration

**User Story:** 作為開發者，我想要在建立 Dev_Panel 時就以宣告方式指定分頁的階層與顯示順序，以便在初始化階段組織面板結構，而不需執行期動態調整。

#### Acceptance Criteria

1. THE Dev_Panel SHALL 於建立時接受一份分頁結構定義，指定各 Panel_Tab 與 Panel_Sub_Tab 的所屬階層（父子關係）與顯示順序
2. WHEN Dev_Panel 依分頁結構定義建立完成時，THE Dev_Panel SHALL 依定義中指定的階層與顯示順序呈現各 Panel_Tab 與 Panel_Sub_Tab
3. IF 分頁結構定義中某個 Panel_Sub_Tab 的巢狀階層深度超過 Panel_Max_Depth，THEN THE Dev_Panel SHALL 拒絕建立該面板結構並回傳表示超過階層上限的錯誤指示
4. IF 分頁結構定義中某個 Panel_Sub_Tab 指定的上層節點不存在於該定義中，THEN THE Dev_Panel SHALL 拒絕建立該面板結構並回傳表示指定上層不存在的錯誤指示
5. THE Dev_Panel SHALL 不對外公開任何於執行期新增、排序、提升或降級 Panel_Tab 或 Panel_Sub_Tab 的 API（內部基礎設施若存在，SHALL 不對外公開）

### Requirement 8: Built-in Dev Key List Tab

**User Story:** 作為開發者，我想要在面板中檢視所有開發者按鍵及其對應功能敘述，以便快速查閱可用的開發者操作。

#### Acceptance Criteria

1. THE DevSystem SHALL 內建 Dev_Key_List_Tab 作為 Dev_Panel 的一個 Panel_Tab
2. WHEN Dev_Key_List_Tab 顯示時，THE Dev_Key_List_Tab SHALL 列出目前所有已註冊的 Dev_Key_Registration，並針對每筆顯示其綁定按鍵與功能敘述
3. WHEN Dev_Key_List_Tab 顯示時，THE Dev_Key_List_Tab SHALL 依綁定按鍵字典序（ordinal 升冪）排序所列出的 Dev_Key_Registration
4. WHILE 目前沒有任何已註冊的 Dev_Key_Registration，THE Dev_Key_List_Tab SHALL 顯示一則空清單提示訊息，並顯示零筆項目
5. WHEN Dev_Key_List_Tab 顯示時，THE Dev_Key_List_Tab SHALL 反映目前最新的 Dev_Key_Registration 清單（不論其間是否曾發生變更）

### Requirement 9: Built-in Debug Log Settings Tab

**User Story:** 作為開發者，我想要在面板中檢視並切換各分類日誌的開關狀態，以便在執行期即時調整要顯示的日誌分類。

#### Acceptance Criteria

1. THE DevSystem SHALL 內建 Debug_Log_Settings_Tab 作為 Dev_Panel 的一個 Panel_Tab
2. WHEN Debug_Log_Settings_Tab 顯示時，THE Debug_Log_Settings_Tab SHALL 為每個透過 Dev_Logger API 取得的 Log_Category 各列出一個項目，並顯示其名稱與目前的開關狀態（啟用或停用）
3. IF 透過 Dev_Logger API 取得的 Log_Category 集合為空，THEN THE Debug_Log_Settings_Tab SHALL 顯示一個指示無可用 Log_Category 的空狀態訊息，且不列出任何項目
4. WHEN 開發者於 Debug_Log_Settings_Tab 切換某個 Log_Category 的開關時，THE Debug_Log_Settings_Tab SHALL 透過 Dev_Logger 的 API 將該 Log_Category 的開關狀態更新為與切換後控制項一致的目標狀態
5. WHEN 開發者透過 Dev_Logger API 完成更新某個 Log_Category 的開關狀態後，THE Debug_Log_Settings_Tab SHALL 在同一影格內將該 Log_Category 對應項目顯示的開關狀態更新為 Dev_Logger 回報的最新狀態
6. WHILE Debug_Log_Settings_Tab 顯示中，WHEN 任一 Log_Category 的開關狀態經由 Debug_Log_Settings_Tab 以外的來源（例如設定檔或程式碼）變更時，THE Debug_Log_Settings_Tab SHALL 在下一次更新週期內將該 Log_Category 對應項目顯示的開關狀態同步為 Dev_Logger 回報的最新狀態
7. IF 切換某個 Log_Category 開關時透過 Dev_Logger API 更新失敗，THEN THE Debug_Log_Settings_Tab SHALL 將該 Log_Category 對應項目顯示的開關狀態還原為更新前的狀態，並顯示一則指示更新失敗的錯誤提示

### Requirement 10: DevSystem Lifecycle

**User Story:** 作為開發者，我想要 DevSystem 有明確的初始化與釋放流程，以便在遊戲生命週期中正確啟用與清理開發者功能。

#### Acceptance Criteria

1. WHEN DevSystem_Manager 的 Init 在系統處於未就緒狀態時被呼叫，THE DevSystem_Manager SHALL 讀取 Log_Toggle_Config、建立 Dev_Panel、並將系統標記為就緒
2. THE DevSystem_Manager SHALL 遵循 Singleton<T> 慣例，提供單一實例的全域存取點
3. WHEN DevSystem_Manager 的 Release 在系統處於就緒狀態時被呼叫，THE DevSystem_Manager SHALL 釋放 Dev_Panel 資源、清除所有 Dev_Key_Registration 紀錄
4. IF Release 無法完整清除 Dev_Panel 資源與所有 Dev_Key_Registration 紀錄（任一部分無法清除），THEN THE Release SHALL 視為失敗並回傳表示清除未完成的錯誤指示，但系統仍會標記為未就緒
5. IF 外部程式碼在 DevSystem_Manager 尚未完成 Init（系統處於未就緒狀態）時呼叫其功能 API，THEN THE DevSystem_Manager SHALL 拒絕該呼叫、不改變系統狀態、並回傳表示系統未就緒的錯誤指示
6. IF DevSystem_Manager 的 Init 在系統已處於就緒狀態時再次被呼叫，THEN THE DevSystem_Manager SHALL 不重複建立 Dev_Panel 或重新讀取 Log_Toggle_Config、維持既有就緒狀態不變
7. IF DevSystem_Manager 的 Release 在系統處於未就緒狀態時被呼叫，THEN THE DevSystem_Manager SHALL 不執行任何釋放動作、維持未就緒狀態不變
