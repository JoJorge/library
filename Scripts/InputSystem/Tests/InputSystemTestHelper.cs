namespace InputSystem.Tests
{
    using System.Reflection;
    using NSubstitute;
    using UnityEngine.InputSystem;
    using Utils.Singleton;

    /// <summary>
    /// 輸入系統測試的共用工具方法，集中管理重複的測試輔助邏輯。
    /// </summary>
    internal static class InputSystemTestHelper
    {
        /// <summary>
        /// 建立指定 InputContext 的 mock IContextHandler，
        /// 內含 IsActive 狀態追蹤（Activate/Deactivate 自動切換）。
        /// </summary>
        internal static AbsContextHandler CreateMockHandler(InputContext context)
        {
            var handler = Substitute.For<AbsContextHandler>();
            handler.Context.Returns(context);
            bool isActive = false;
            handler.IsActive.Returns(_ => isActive);
            handler.When(h => h.Activate(Arg.Any<InputActionAsset>())).Do(_ => isActive = true);
            handler.When(h => h.Deactivate()).Do(_ => isActive = false);
            return handler;
        }

        /// <summary>
        /// 透過反射重置 InputSystemManager 的 Singleton 實例為 null。
        /// </summary>
        internal static void ResetSingleton()
        {
            var field = typeof(Singleton<InputSystemManager>)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, null);
        }

        /// <summary>
        /// 透過反射將 InputSystemManager 的 SystemState 設為 Ready（跳過 Init 流程）。
        /// </summary>
        internal static void SetSystemStateReady(InputSystemManager manager)
        {
            var prop = typeof(InputSystemManager).GetProperty("SystemState");
            prop.SetValue(manager, SystemState.Ready);
        }
    }
}
