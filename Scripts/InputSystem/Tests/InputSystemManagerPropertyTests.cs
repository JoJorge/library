namespace InputSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;

    /// <summary>
    /// InputSystemManager 的屬性導向測試（Property-Based Testing）。
    /// </summary>
    [TestFixture]
    public class InputSystemManagerPropertyTests
    {
        [TearDown]
        public void TearDown()
        {
            InputSystemTestHelper.ResetSingleton();
        }

        /// <summary>
        /// Property 4: Context Exclusivity — 情境處理器互斥啟用。
        /// 對於任意長度的情境切換序列，在每次 SwitchContext 完成後，
        /// 系統中所有已註冊的 IContextHandler 中 IsActive == true 的數量最多為 1，
        /// 且僅有切換目標的處理器處於啟用狀態。
        /// **Validates: Requirements 5.2, 5.4, 5.5**
        /// </summary>
        [Test]
        public void SwitchContext_AtMostOneHandlerActive_AfterEachSwitch()
        {
            var contextValues = (InputContext[])Enum.GetValues(typeof(InputContext));
            var sequenceArb = Arb.From(
                Gen.Choose(0, contextValues.Length - 1)
                   .ArrayOf()
                   .Where(a => a.Length >= 2 && a.Length <= 100));

            Prop.ForAll(sequenceArb, indices =>
            {
                InputSystemTestHelper.ResetSingleton();
                var manager = InputSystemManager.Instance;

                // 建立 mock handlers 並追蹤各自的 IsActive 狀態
                var handlers = new Dictionary<InputContext, AbsContextHandler>();
                foreach (var ctx in contextValues)
                {
                    var handler = InputSystemTestHelper.CreateMockHandler(ctx);
                    handlers[ctx] = handler;
                    manager.RegisterContextHandler(handler);
                }

                // 設定系統狀態為 Ready
                InputSystemTestHelper.SetSystemStateReady(manager);

                // 依序切換 context 並驗證互斥性
                foreach (int idx in indices)
                {
                    var target = contextValues[idx];
                    bool result = manager.SwitchContext(target);

                    // 計算所有 handler 中 IsActive == true 的數量
                    int activeCount = handlers.Values.Count(h => h.IsActive);

                    if (activeCount > 1)
                    {
                        return false.Label(
                            $"After switching to {target}: activeCount={activeCount} (expected ≤ 1)");
                    }

                    // 切換成功時，僅目標 handler 應處於啟用狀態
                    if (result && !handlers[target].IsActive)
                    {
                        return false.Label(
                            $"After switching to {target}: target handler is not active");
                    }
                }

                return true.Label("Context exclusivity maintained throughout sequence");
            }).QuickCheckThrowOnFailure();
        }
    }
}
