# Requirement Test Coverage Report

## Overall Coverage

- Total Requirements: 69
- Covered Requirements: 68
- Coverage Rate: 98.55%

## Coverage by Requirement

| Requirement | Description | Test Methods | Status |
|-------------|-------------|--------------|--------|
| 1.1 | WHERE DEV_MODE 已定義，THE DevSystem SHALL 編譯並提供其所有公開 API | `DevSystemManagerApis_CompileAndReturnDefaults`, `DevLoggerApis_CompileCleanly`, `DevKeyBinderApis_CompileCleanly`, `DevPanelApis_CompileCleanly` | Covered |
| 1.2 | WHERE DEV_MODE 未定義，THE DevSystem SHALL 仍可編譯，但其公開 API 方法主體為空實作（no-op） | Property 11, `AllReturnValueApis_NoOpAndReturnDefault_WhenDevModeUndefined` | Covered |
| 1.3 | WHERE DEV_MODE 未定義且外部呼叫公開 API，THE DevSystem SHALL 使該呼叫成為 no-op（無副作用） | Property 11, `AllReturnValueApis_NoOpAndReturnDefault_WhenDevModeUndefined` | Covered |
| 1.4 | WHERE DEV_MODE 未定義且外部呼叫公開 API，THE DevSystem SHALL 保持呼叫端可成功編譯且零錯誤/警告 | `DevSystemManagerApis_CompileAndReturnDefaults`, `DevLoggerApis_CompileCleanly`, `DevKeyBinderApis_CompileCleanly`, `DevPanelApis_CompileCleanly` | Covered |
| 1.5 | IF 公開 API 有回傳值且於 DEV_MODE 未定義時被呼叫，THEN SHALL 回傳該型別的預設值 | Property 11, `AllReturnValueApis_NoOpAndReturnDefault_WhenDevModeUndefined`, `DevSystemManagerApis_CompileAndReturnDefaults` | Covered |
| 1.6 | THE DevSystem SHALL 以 DEV_MODE 作為唯一控制開發者功能編譯與作用的 compilation symbol | Property 11, `AllReturnValueApis_NoOpAndReturnDefault_WhenDevModeUndefined` | Covered |
| 2.1 | THE Dev_Logger SHALL 提供接受 Log_Category 與訊息字串並輸出一筆日誌的公開 API | `Log_PublicSignature_AcceptsCategoryAndString` | Covered |
| 2.2 | WHEN 輸出日誌時，SHALL 依 `[Dev][分類] MSG` 格式化訊息 | Property 1, `Log_FormatStructure_AlwaysMatchesExpected` | Covered |
| 2.3 | THE Dev_Logger SHALL 以 enum 型別（Log_Category）作為分類參數 | `LogCategory_IsEnumType` | Covered |
| 2.4 | THE Dev_Logger SHALL 提供擴充機制，使新增 Log_Category 列舉值不需修改既有輸出邏輯 | `GetCategories_ReflectsAllDefinedEnumMembers` | Covered |
| 2.5 | IF 訊息為 null 或空字串，THEN SHALL 仍輸出日誌且 MSG 為空字串、格式前綴不變 | Property 1, `Log_FormatStructure_AlwaysMatchesExpected` | Covered |
| 2.6 | IF Log_Category 值未對應任何已定義成員，THEN SHALL 以固定預設分類名輸出且不拋例外 | Property 1, `Log_FormatStructure_AlwaysMatchesExpected` | Covered |
| 2.7 | THE Dev_Logger SHALL 接受訊息上限 4096 字元，超過時截斷後輸出 | Property 1, `Log_LongMessage_TruncatedToPrefix` | Covered |
| 2.8 | IF 底層日誌輸出系統失敗或不可用，THEN SHALL 抑制輸出且不拋例外 | Property 1, `Log_SinkFailsOrThrows_NeverPropagatesException` | Covered |
| 3.1 | WHILE 某分類開關為開啟，THE Dev_Logger SHALL 輸出該分類日誌 | Property 2, `Log_ToggleGate_WriteCountMatchesEnabledState` | Covered |
| 3.2 | WHILE 某分類開關為關閉，THE Dev_Logger SHALL 不輸出該分類日誌 | Property 2, `Log_ToggleGate_WriteCountMatchesEnabledState` | Covered |
| 3.3 | THE Dev_Logger SHALL 於載入時讀取 Log_Toggle_Config 為暫存狀態 | Property 3, `LoadToggleConfig_AppliesListAsAuthoritativeAndNeverPersists` | Covered |
| 3.4 | WHEN 讀取設定完成後，SHALL 依設定套用各分類暫存開關（清單內開啟、清單外關閉） | Property 3, `LoadToggleConfig_AppliesListAsAuthoritativeAndNeverPersists` | Covered |
| 3.5 | IF 無可用設定檔，THEN SHALL 將所有分類暫存開關預設為開啟 | Property 3, `LoadToggleConfig_AppliesListAsAuthoritativeAndNeverPersists` | Covered |
| 3.6 | WHILE 設定尚未讀取完成，SHALL 將所有分類視為開啟並輸出 | `Log_BeforeConfigLoaded_AllCategoriesEnabled` | Covered |
| 3.7 | THE Dev_Logger SHALL 僅維護暫存開關且永不寫回設定檔或持久化 | Property 3, `LoadToggleConfig_AppliesListAsAuthoritativeAndNeverPersists` | Covered |
| 3.8 | WHEN 系統啟動時，SHALL 重新依設定套用暫存開關（前次調整不影響本次） | Property 3, `LoadToggleConfig_AppliesListAsAuthoritativeAndNeverPersists` | Covered |
| 3.9 | THE Dev_Logger SHALL 提供供 Debug_Log_Settings_Tab 於執行期切換暫存開關的公開 API | `SetCategoryEnabled_PublicSignature_ReturnsBool`, `SetCategoryEnabled_WhenInvoked_ReturnsTrue` | Covered |
| 3.10 | WHEN 執行期切換開關時，SHALL 更新暫存狀態並於後續第一筆該分類日誌請求即生效 | Property 2, `Log_ToggleGate_WriteCountMatchesEnabledState` | Covered |
| 4.1 | THE Dev_Key_Binder SHALL 提供接受 Dev_Action_Enum、按鍵路徑與敘述向 Binding_Manager 註冊的公開 API | Property 4, `RegisterDevKey_SuccessPath_CallsApplyBinding`, `RegisterDevKey_NoConflict_AppliesBindingAndRecordsRegistration` | Covered |
| 4.2 | IF 按鍵路徑為空或無法解析，THEN SHALL 拒絕註冊、不建立紀錄並回傳路徑無效錯誤 | `RegisterDevKey_UnresolvablePath_ReturnsInvalidPath`, `RegisterDevKey_EmptyPath_ReturnsInvalidRequest_AndSkipsConflictCheck` | Covered |
| 4.3 | WHEN 綁定完成時，SHALL 建立並儲存一筆 Dev_Key_Registration | Property 4, `Property4_RegistrationQueryConsistency` | Covered |
| 4.4 | IF Dev_Action_Enum 已存在於清單，THEN SHALL 拒絕重複註冊、保留既有並回傳已註冊錯誤 | Property 4, `Property4_RegistrationQueryConsistency` | Covered |
| 4.5 | THE Dev_Action_Enum SHALL 以 enum 管理且每個成員名以 `Dev` 前綴 | `DevAction_AllMembers_HaveDevPrefix` | Covered |
| 4.6 | THE Dev_Key_Binder SHALL 提供擴充機制，使新增 Dev_Action_Enum 值不需修改既有綁定邏輯 | `DevAction_AllMembers_HaveDevPrefix` | Covered |
| 4.7 | WHEN 以某 Dev_Action_Enum 呼叫解除綁定，SHALL 向 Binding_Manager 解綁並移除對應紀錄 | Property 4, `Property4_RegistrationQueryConsistency` | Covered |
| 4.8 | IF 解除綁定的 Dev_Action_Enum 不存在於清單，THEN SHALL 不變更綁定並回傳查無錯誤 | `Unbind_NonexistentAction_ReturnsFalse_AndDoesNotResetBinding` | Covered |
| 4.9 | WHEN 呼叫清單 API 時，SHALL 回傳目前所有已註冊的 Dev_Key_Registration 清單 | Property 4, `Property4_RegistrationQueryConsistency` | Covered |
| 5.1 | WHEN 綁定按鍵時，SHALL 於寫入前透過 Conflict_Checker 對所有 Normal_Binding 與既有註冊進行衝突檢查 | Property 5, `Property5_ConflictGateAndScopeReuse`, `RegisterDevKey_NoConflict_AppliesBindingAndRecordsRegistration` | Covered |
| 5.2 | THE Dev_Key_Binder SHALL 以與 Conflict_Checker 相同的衝突判定範圍進行檢查 | Property 5, `Property5_ConflictGateAndScopeReuse` | Covered |
| 5.3 | IF 綁定與任一 Normal_Binding 或既有註冊衝突，THEN SHALL 拒絕綁定並回傳含所有衝突對象清單 | Property 5, `Property5_ConflictGateAndScopeReuse`, `RegisterDevKey_ConflictingPath_RejectedWithConflictsAndNotRecorded` | Covered |
| 5.4 | WHEN 衝突清單為空時，SHALL 完成綁定與 Dev_Key_Registration 紀錄 | Property 5, `Property5_ConflictGateAndScopeReuse` | Covered |
| 5.5 | IF 綁定請求缺必要欄位，THEN SHALL 不執行衝突檢查、拒絕綁定並回傳請求無效錯誤 | `RegisterDevKey_EmptyPath_ReturnsInvalidRequest_AndSkipsConflictCheck` | Covered |
| 6.1 | THE Dev_Panel SHALL 支援 1 至 100 個 Panel_Tab，並以 UI Toolkit 呈現 | `TryBuild_EmptyTopLevelRejected`, `Init_FullFlow_ReadyWithBuiltInTabsMounted` | Covered |
| 6.2 | WHEN 選擇具下層子分頁的 Panel_Tab 時，SHALL 顯示其下的 Panel_Sub_Tab 選項 | Property 7, `PanelNavigation_MultiLevelTree_SelectsContainerAndLeaf` | Covered |
| 6.3 | WHEN 選擇無下層子分頁的節點時，SHALL 顯示該節點葉內容而非子分頁選項 | Property 7, `PanelNavigation_MultiLevelTree_SelectsContainerAndLeaf` | Covered |
| 6.4 | THE Dev_Panel SHALL 以固定編譯期常數定義 Panel_Max_Depth，且不提供執行期變更 API | `TryBuild_ChainAtMaxDepthSucceeds`, `TryBuild_ChainExceedingMaxDepthRejected` | Covered |
| 6.5 | IF 新增子分頁深度超過 Panel_Max_Depth，THEN SHALL 拒絕並保留既有結構、回傳超過上限錯誤 | Property 6, `TryBuild_RespectsDepthValidityInvariant`, `TryBuild_ChainExceedingMaxDepthRejected` | Covered |
| 6.6 | WHEN 選擇具下層子分頁的 Panel_Sub_Tab 時，SHALL 顯示其下一層 Panel_Sub_Tab 選項 | Property 7, `PanelNavigation_MultiLevelTree_SelectsContainerAndLeaf` | Covered |
| 7.1 | THE Dev_Panel SHALL 於建立時接受指定階層與顯示順序的分頁結構定義 | `TryBuild_TopLevelOrderMatchesDefinitionListOrder`, `TryBuild_EmptyTopLevelRejected` | Covered |
| 7.2 | WHEN 依定義建立完成時，SHALL 依定義的階層與顯示順序呈現各分頁 | Property 7, `TryBuild_TopLevelOrderMatchesDefinitionListOrder`, `TryBuild_NestedOrderAndParentChildStructurePreserved` | Covered |
| 7.3 | IF 定義中某子分頁深度超過 Panel_Max_Depth，THEN SHALL 拒絕建立並回傳超過上限錯誤 | Property 6, `TryBuild_RespectsDepthValidityInvariant`, `TryBuild_ChainExceedingMaxDepthRejected` | Covered |
| 7.4 | IF 定義中某子分頁指定的上層不存在，THEN SHALL 拒絕建立並回傳上層不存在錯誤 | `TryBuild_NestedOrderAndParentChildStructurePreserved` | Covered |
| 7.5 | THE Dev_Panel SHALL 不對外公開任何執行期新增/排序/提升/降級分頁的 API | - | Not Covered |
| 8.1 | THE DevSystem SHALL 內建 Dev_Key_List_Tab 作為 Dev_Panel 的一個 Panel_Tab | `DevKeyListTab_CanBeMountedAsPanelNodeDefinitionTab`, `Init_FullFlow_ReadyWithBuiltInTabsMounted` | Covered |
| 8.2 | WHEN 顯示時，SHALL 列出所有已註冊 Dev_Key_Registration 並顯示綁定按鍵與敘述 | Property 8, `Property8_ListReflectsSortedCurrentRegistrations` | Covered |
| 8.3 | WHEN 顯示時，SHALL 依綁定按鍵字典序（ordinal 升冪）排序 | Property 8, `Property8_ListReflectsSortedCurrentRegistrations` | Covered |
| 8.4 | WHILE 沒有任何已註冊項目，SHALL 顯示空清單提示並顯示零筆項目 | `EmptyRegistrations_ShowsEmptyMessageAndZeroEntries` | Covered |
| 8.5 | WHEN 顯示時，SHALL 反映目前最新的 Dev_Key_Registration 清單 | Property 8, `Property8_ListReflectsSortedCurrentRegistrations` | Covered |
| 9.1 | THE DevSystem SHALL 內建 Debug_Log_Settings_Tab 作為 Dev_Panel 的一個 Panel_Tab | `DebugLogSettingsTab_CanBeMountedAsPanelNodeDefinitionTab`, `Init_FullFlow_ReadyWithBuiltInTabsMounted` | Covered |
| 9.2 | WHEN 顯示時，SHALL 為每個 Log_Category 列出項目並顯示名稱與開關狀態 | Property 9, `Property9_UiSyncsWithModelOnToggleAndUpdate` | Covered |
| 9.3 | IF 取得的 Log_Category 集合為空，THEN SHALL 顯示空狀態訊息且不列出任何項目 | `EmptyCategories_ShowsEmptyStateAndNoToggles` | Covered |
| 9.4 | WHEN 切換某 Log_Category 開關時，SHALL 透過 Dev_Logger API 更新為與控制項一致的目標狀態 | Property 9, `Property9_UiSyncsWithModelOnToggleAndUpdate` | Covered |
| 9.5 | WHEN 透過 Dev_Logger API 完成更新後，SHALL 於同一影格將該項目顯示狀態更新為最新狀態 | Property 9, `Property9_UiSyncsWithModelOnToggleAndUpdate` | Covered |
| 9.6 | WHILE 顯示中，WHEN 開關狀態由外部來源變更時，SHALL 於下一更新週期同步顯示狀態 | Property 9, `Property9_UiSyncsWithModelOnToggleAndUpdate` | Covered |
| 9.7 | IF 切換時透過 Dev_Logger API 更新失敗，THEN SHALL 還原該項目顯示狀態並顯示更新失敗錯誤提示 | `ToggleUpdateFails_RevertsRowAndShowsError` | Covered |
| 10.1 | WHEN Init 於未就緒時被呼叫，SHALL 讀取設定、建立面板並標記就緒 | Property 10, `Property10_RepeatedInit_IsIdempotentAndReady`, `Property10_ReadyThenRelease_ClearsRegistrationsPanelAndState`, `Init_FullFlow_ReadyWithBuiltInTabsMounted` | Covered |
| 10.2 | THE DevSystem_Manager SHALL 遵循 Singleton<T> 慣例並提供單一實例全域存取點 | `Instance_ProvidesGlobalSingletonAccess` | Covered |
| 10.3 | WHEN Release 於就緒時被呼叫，SHALL 釋放面板資源並清除所有 Dev_Key_Registration | Property 10, `Property10_ReadyThenRelease_ClearsRegistrationsPanelAndState`, `Release_AfterInitAndRegistration_ClearsStateAndRegistrations` | Covered |
| 10.4 | IF Release 無法完整清除，THEN Release SHALL 視為失敗並回傳錯誤，但系統仍標記為未就緒 | Property 10, `Property10_ReadyThenRelease_ClearsRegistrationsPanelAndState` | Covered |
| 10.5 | IF 於未就緒時呼叫功能 API，THEN SHALL 拒絕呼叫、不改變狀態並回傳未就緒錯誤 | Property 10, `Property10_NotReady_RejectsFunctionalApisAndStateUnchanged` | Covered |
| 10.6 | IF Init 於已就緒時再次被呼叫，THEN SHALL 不重建面板或重讀設定、維持就緒狀態 | Property 10, `Property10_RepeatedInit_IsIdempotentAndReady` | Covered |
| 10.7 | IF Release 於未就緒時被呼叫，THEN SHALL 不執行任何釋放動作、維持未就緒狀態 | `Release_WhenNotReady_IsNoOpAndReturnsTrue` | Covered |
