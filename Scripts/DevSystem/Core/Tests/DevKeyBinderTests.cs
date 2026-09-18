namespace DevSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FsCheck;
    using FsCheck.Fluent;
    using InputSystem;
    using NSubstitute;
    using NUnit.Framework;

    /// <summary>
    /// DevKeyBinder 的屬性導向測試（PBT）與範例導向測試。
    /// 依 design.md 測試策略，透過 NSubstitute 對 <see cref="BindingManager"/> 與
    /// <see cref="ConflictChecker"/> 建立測試替身：<see cref="ConflictChecker.CheckConflict"/>
    /// 以受控清單回傳，<see cref="BindingManager.ApplyBinding"/> / <see cref="BindingManager.ResetBinding"/>
    /// 以呼叫紀錄驗證，藉此在不依賴實際 Unity Input System 資產的情況下驗證綁定流程。
    ///
    /// Feature: dev-system, Property 4: Dev Key Registration Query Consistency
    /// Feature: dev-system, Property 5: Dev Key Conflict Gate and Scope Reuse
    /// </summary>
    [TestFixture]
    public class DevKeyBinderTests
    {
        /// <summary>
        /// 有效的鍵盤裝置按鍵路徑集合，皆可經路徑前綴解析為 KeyboardMouse 裝置類型。
        /// </summary>
        private static readonly string[] ValidKeyboardPaths =
        {
            "<Keyboard>/a",
            "<Keyboard>/b",
            "<Keyboard>/c",
            "<Keyboard>/d",
            "<Keyboard>/space",
            "<Keyboard>/enter",
            "<Keyboard>/tab",
            "<Mouse>/leftButton",
        };

        /// <summary>
        /// 全部可用的 DevAction 列舉值。
        /// </summary>
        private static readonly DevAction[] AllActions =
            (DevAction[])Enum.GetValues(typeof(DevAction));

        // =========================================================
        // Test doubles
        // =========================================================

        /// <summary>
        /// 建立回傳空衝突清單（即無衝突）的 ConflictChecker 測試替身。
        /// </summary>
        /// <returns>對任意輸入皆回傳空集合的替身。</returns>
        private static ConflictChecker CreateNoConflictChecker()
        {
            var checker = Substitute.For<ConflictChecker>();
            checker.CheckConflict(Arg.Any<Enum>(), Arg.Any<string>(), Arg.Any<InputDeviceType>())
                .Returns(Array.Empty<BindingConflict>());
            return checker;
        }

        /// <summary>
        /// 建立一個 BindingManager 測試替身（所有相關方法皆為 virtual，可攔截與驗證）。
        /// </summary>
        /// <returns>BindingManager 替身。</returns>
        private static BindingManager CreateBindingManager()
        {
            return Substitute.For<BindingManager>();
        }

        /// <summary>
        /// 建立一筆代表衝突的 BindingConflict，供受控衝突清單使用。
        /// </summary>
        /// <param name="action">觸發衝突的動作。</param>
        /// <param name="bindingPath">衝突路徑。</param>
        /// <returns>單一衝突項。</returns>
        private static BindingConflict MakeConflict(DevAction action, string bindingPath)
        {
            return new BindingConflict(
                bindingPath,
                action,
                new List<Enum> { action },
                InputContext.Gameplay,
                InputDeviceType.KeyboardMouse);
        }

        // =========================================================
        // Property 4: Dev Key Registration Query Consistency
        // =========================================================

        /// <summary>
        /// Property 4: Dev Key Registration Query Consistency — 按鍵註冊查詢一致性。
        /// 對於任意一組具唯一 DevAction、有效路徑且互不衝突的註冊請求，依序 RegisterDevKey 後，
        /// GetRegistrations 回傳的集合應與成功註冊的請求一一對應（action、bindingPath、description 相符）；
        /// 對已註冊的 DevAction 再次註冊應被拒絕且既有註冊不變；對已註冊的 DevAction 呼叫 Unbind 後，
        /// GetRegistrations 不再包含該項。
        /// **Validates: Requirements 4.3, 4.4, 4.7, 4.9**
        /// </summary>
        [Test]
        public void Property4_RegistrationQueryConsistency()
        {
            // 生成一組「要註冊的 action 子集合」以及對應的路徑索引與敘述。
            // 使用 action 子集合確保每個 action 唯一（無重複），符合屬性前提。
            var arb = Arb.From(GenRegistrationBatch());

            Prop.ForAll(arb, batch =>
            {
                ConflictChecker checker = CreateNoConflictChecker();
                BindingManager binding = CreateBindingManager();
                var binder = new DevKeyBinder(binding, checker);

                // 依序註冊每筆唯一請求，皆應成功（無衝突、路徑有效、action 唯一）。
                foreach (var req in batch)
                {
                    DevKeyBindResult result = binder.RegisterDevKey(req.Action, req.Path, req.Description);
                    if (!result.Success)
                    {
                        return false.Label($"unique valid request should succeed: {req.Action}/{req.Path}");
                    }
                }

                // 一一對應：GetRegistrations 與註冊請求數量相同，且每筆欄位相符。
                IReadOnlyList<DevKeyRegistration> registrations = binder.GetRegistrations();
                if (registrations.Count != batch.Count)
                {
                    return false.Label($"count mismatch: expected {batch.Count}, got {registrations.Count}");
                }

                foreach (var req in batch)
                {
                    DevKeyRegistration match = registrations.FirstOrDefault(r => r.Action == req.Action);
                    if (match == null
                        || match.BindingPath != req.Path
                        || match.Description != req.Description)
                    {
                        return false.Label($"registration mismatch for {req.Action}");
                    }
                }

                // 重複註冊：對第一筆 action 以不同路徑再次註冊 → 被拒（Duplicate）且既有不變。
                if (batch.Count > 0)
                {
                    var first = batch[0];
                    string otherPath = ValidKeyboardPaths[(Array.IndexOf(ValidKeyboardPaths, first.Path) + 1)
                        % ValidKeyboardPaths.Length];
                    DevKeyBindResult dup = binder.RegisterDevKey(first.Action, otherPath, "dup-desc");
                    if (dup.Success || dup.FailureReason != DevKeyBindFailure.Duplicate)
                    {
                        return false.Label("duplicate registration should be rejected with Duplicate");
                    }

                    DevKeyRegistration afterDup = binder.GetRegistrations()
                        .First(r => r.Action == first.Action);
                    if (afterDup.BindingPath != first.Path || afterDup.Description != first.Description)
                    {
                        return false.Label("existing registration must be unchanged after duplicate reject");
                    }
                }

                // 解綁：對第一筆 action 解綁後，GetRegistrations 不再包含該項且數量減一。
                if (batch.Count > 0)
                {
                    var first = batch[0];
                    bool unbound = binder.Unbind(first.Action);
                    if (!unbound)
                    {
                        return false.Label("Unbind of registered action should return true");
                    }

                    IReadOnlyList<DevKeyRegistration> afterUnbind = binder.GetRegistrations();
                    if (afterUnbind.Any(r => r.Action == first.Action))
                    {
                        return false.Label("Unbound action must not remain in registrations");
                    }

                    if (afterUnbind.Count != batch.Count - 1)
                    {
                        return false.Label("registration count should decrease by one after Unbind");
                    }
                }

                return true.Label("registration query consistency holds");
            }).QuickCheckThrowOnFailure();
        }

        // =========================================================
        // Property 5: Dev Key Conflict Gate and Scope Reuse
        // =========================================================

        /// <summary>
        /// Property 5: Dev Key Conflict Gate and Scope Reuse — 按鍵衝突閘控與範圍重用。
        /// 對於任意欄位齊全的註冊請求，DevKeyBinder 應先呼叫 ConflictChecker.CheckConflict 再決定是否寫入：
        /// 當回傳的衝突清單非空時，該次註冊被拒絕、不新增 DevKeyRegistration、不呼叫 ApplyBinding，
        /// 且回傳結果攜帶與 CheckConflict 回傳完全相同的衝突清單；當衝突清單為空時，該次註冊完成且呼叫 ApplyBinding。
        /// DevKeyBinder 不施加任何自訂衝突範圍邏輯，衝突判定完全由 ConflictChecker 決定。
        /// **Validates: Requirements 5.1, 5.2, 5.3, 5.4**
        /// </summary>
        [Test]
        public void Property5_ConflictGateAndScopeReuse()
        {
            // 生成：action 索引、路徑索引、以及「是否製造衝突」的布林旗標。
            var arb = Arb.From(
                from actionIdx in Gen.Choose(0, AllActions.Length - 1)
                from pathIdx in Gen.Choose(0, ValidKeyboardPaths.Length - 1)
                from hasConflict in Gen.Elements(true, false)
                select new { ActionIdx = actionIdx, PathIdx = pathIdx, HasConflict = hasConflict });

            Prop.ForAll(arb, data =>
            {
                DevAction action = AllActions[data.ActionIdx];
                string path = ValidKeyboardPaths[data.PathIdx];

                // 受控衝突清單：hasConflict 為 true 時回傳單一衝突，否則回傳空集合。
                IReadOnlyList<BindingConflict> controlled = data.HasConflict
                    ? new List<BindingConflict> { MakeConflict(action, path) }
                    : (IReadOnlyList<BindingConflict>)Array.Empty<BindingConflict>();

                ConflictChecker checker = Substitute.For<ConflictChecker>();
                checker.CheckConflict(Arg.Any<Enum>(), Arg.Any<string>(), Arg.Any<InputDeviceType>())
                    .Returns(controlled);
                BindingManager binding = CreateBindingManager();
                var binder = new DevKeyBinder(binding, checker);

                DevKeyBindResult result = binder.RegisterDevKey(action, path, "desc");

                // 寫入前先呼叫 CheckConflict（範圍重用：完全委派 ConflictChecker）。
                checker.Received(1).CheckConflict(
                    Arg.Any<Enum>(), path, InputDeviceType.KeyboardMouse);

                if (data.HasConflict)
                {
                    // 非空衝突 → 拒絕、回傳相同清單、未寫入、未呼叫 ApplyBinding。
                    if (result.Success || result.FailureReason != DevKeyBindFailure.Conflict)
                    {
                        return false.Label("non-empty conflict list should reject with Conflict");
                    }

                    if (!ReferenceEquals(result.Conflicts, controlled)
                        && !result.Conflicts.SequenceEqual(controlled))
                    {
                        return false.Label("returned conflict list must equal CheckConflict output");
                    }

                    if (binder.GetRegistrations().Count != 0)
                    {
                        return false.Label("no registration should be written on conflict");
                    }

                    binding.DidNotReceive().ApplyBinding(
                        Arg.Any<Enum>(), Arg.Any<string>(), Arg.Any<InputDeviceType>());
                }
                else
                {
                    // 空衝突 → 完成、呼叫 ApplyBinding、寫入一筆。
                    if (!result.Success)
                    {
                        return false.Label("empty conflict list should complete binding");
                    }

                    binding.Received(1).ApplyBinding(action, path, InputDeviceType.KeyboardMouse);

                    if (binder.GetRegistrations().Count != 1)
                    {
                        return false.Label("exactly one registration should be written on success");
                    }
                }

                return true.Label("conflict gate and scope reuse holds");
            }).QuickCheckThrowOnFailure();
        }

        // =========================================================
        // Example tests
        // =========================================================

        /// <summary>
        /// 成功路徑：無衝突且欄位齊全時，RegisterDevKey 透過 BindingManager.ApplyBinding 綁定。
        /// **Validates: Requirements 4.1**
        /// </summary>
        [Test]
        public void RegisterDevKey_SuccessPath_CallsApplyBinding()
        {
            ConflictChecker checker = CreateNoConflictChecker();
            BindingManager binding = CreateBindingManager();
            var binder = new DevKeyBinder(binding, checker);

            DevKeyBindResult result = binder.RegisterDevKey(
                DevAction.DevTogglePanel, "<Keyboard>/f1", "Toggle dev panel");

            Assert.That(result.Success, Is.True);
            binding.Received(1).ApplyBinding(
                DevAction.DevTogglePanel, "<Keyboard>/f1", InputDeviceType.KeyboardMouse);
        }

        /// <summary>
        /// 空路徑：缺少必要欄位（Binding Path）時回傳 InvalidRequest，且不建立任何註冊。
        /// **Validates: Requirements 5.5**
        /// </summary>
        [Test]
        public void RegisterDevKey_EmptyPath_ReturnsInvalidRequest_AndSkipsConflictCheck()
        {
            ConflictChecker checker = CreateNoConflictChecker();
            BindingManager binding = CreateBindingManager();
            var binder = new DevKeyBinder(binding, checker);

            DevKeyBindResult result = binder.RegisterDevKey(DevAction.DevTogglePanel, string.Empty, "desc");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(DevKeyBindFailure.InvalidRequest));
            Assert.That(binder.GetRegistrations(), Is.Empty);

            // 缺欄位時不做衝突檢查（需求 5.5）。
            checker.DidNotReceive().CheckConflict(
                Arg.Any<Enum>(), Arg.Any<string>(), Arg.Any<InputDeviceType>());
        }

        /// <summary>
        /// 無效路徑：非空但無法解析為有效裝置路徑時回傳 InvalidPath，且不建立任何註冊。
        /// **Validates: Requirements 4.2**
        /// </summary>
        [Test]
        public void RegisterDevKey_UnresolvablePath_ReturnsInvalidPath()
        {
            ConflictChecker checker = CreateNoConflictChecker();
            BindingManager binding = CreateBindingManager();
            var binder = new DevKeyBinder(binding, checker);

            DevKeyBindResult result = binder.RegisterDevKey(
                DevAction.DevTogglePanel, "not-a-valid-device-path", "desc");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(DevKeyBindFailure.InvalidPath));
            Assert.That(binder.GetRegistrations(), Is.Empty);
        }

        /// <summary>
        /// DevAction 命名慣例：反射驗證所有列舉成員名稱皆以 Dev 前綴命名。
        /// **Validates: Requirements 4.5**
        /// </summary>
        [Test]
        public void DevAction_AllMembers_HaveDevPrefix()
        {
            foreach (string name in Enum.GetNames(typeof(DevAction)))
            {
                Assert.That(
                    name.StartsWith("Dev", StringComparison.Ordinal),
                    Is.True,
                    $"DevAction member '{name}' should start with 'Dev'");
            }
        }

        /// <summary>
        /// 解綁不存在的 action：回傳 false 且不變更任何綁定。
        /// **Validates: Requirements 4.8**
        /// </summary>
        [Test]
        public void Unbind_NonexistentAction_ReturnsFalse_AndDoesNotResetBinding()
        {
            ConflictChecker checker = CreateNoConflictChecker();
            BindingManager binding = CreateBindingManager();
            var binder = new DevKeyBinder(binding, checker);

            bool result = binder.Unbind(DevAction.DevReloadScene);

            Assert.That(result, Is.False);
            binding.DidNotReceive().ResetBinding(Arg.Any<Enum>(), Arg.Any<InputDeviceType>());
        }

        // =========================================================
        // Generators
        // =========================================================

        /// <summary>
        /// 生成一批具「唯一 DevAction、有效路徑、任意敘述」的註冊請求。
        /// 以 action 的隨機排列子集合確保 action 唯一，路徑由有效路徑集合隨機取樣（允許重複）。
        /// </summary>
        /// <returns>註冊請求批次的產生器。</returns>
        private static Gen<List<RegistrationRequest>> GenRegistrationBatch()
        {
            return
                from take in Gen.Choose(0, AllActions.Length)
                from shuffled in GenShuffle(AllActions)
                from paths in Gen.ArrayOf(Gen.Choose(0, ValidKeyboardPaths.Length - 1), take)
                from descriptions in Gen.ArrayOf(
                    Gen.Elements("desc-a", "desc-b", "desc-c", string.Empty), take)
                select Enumerable.Range(0, take)
                    .Select(i => new RegistrationRequest(
                        shuffled[i],
                        ValidKeyboardPaths[paths[i]],
                        descriptions[i]))
                    .ToList();
        }

        /// <summary>
        /// 產生一個陣列的隨機排列。
        /// </summary>
        /// <typeparam name="T">元素型別。</typeparam>
        /// <param name="source">來源陣列。</param>
        /// <returns>隨機排列後的陣列產生器。</returns>
        private static Gen<T[]> GenShuffle<T>(T[] source)
        {
            return Gen.ArrayOf(Gen.Choose(0, int.MaxValue), source.Length)
                .Select(keys =>
                {
                    var indexed = source
                        .Select((item, i) => new { item, key = keys[i] })
                        .OrderBy(x => x.key)
                        .Select(x => x.item)
                        .ToArray();
                    return indexed;
                });
        }

        /// <summary>
        /// 單筆註冊請求（測試用資料）。
        /// </summary>
        private sealed class RegistrationRequest
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrationRequest"/> class.
            /// </summary>
            /// <param name="action">開發者動作。</param>
            /// <param name="path">綁定路徑。</param>
            /// <param name="description">功能敘述。</param>
            public RegistrationRequest(DevAction action, string path, string description)
            {
                this.Action = action;
                this.Path = path;
                this.Description = description;
            }

            /// <summary>Gets 開發者動作。</summary>
            public DevAction Action { get; }

            /// <summary>Gets 綁定路徑。</summary>
            public string Path { get; }

            /// <summary>Gets 功能敘述。</summary>
            public string Description { get; }
        }
    }
}
