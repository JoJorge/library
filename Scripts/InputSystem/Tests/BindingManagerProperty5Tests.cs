namespace InputSystem.Tests
{
    using System;
    using System.Linq;
    using FsCheck;
    using FsCheck.Fluent;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// BindingManager 的屬性導向測試：Property 5 — Binding Query Consistency。
    /// 驗證 ApplyBinding 後立即 GetBindingPaths 查詢包含新路徑。
    /// </summary>
    [TestFixture]
    public class BindingManagerProperty5Tests
    {
        private static readonly string[] KeyboardMousePaths =
        {
            "<Keyboard>/a",
            "<Keyboard>/b",
            "<Keyboard>/space",
            "<Mouse>/leftButton",
        };

        private static readonly string[] GamepadPaths =
        {
            "<Gamepad>/buttonSouth",
            "<Gamepad>/buttonNorth",
            "<Gamepad>/leftStick",
        };

        /// <summary>
        /// Property 5: Binding Query Consistency — 對任意合法 action/bindingPath/deviceType，
        /// ApplyBinding 後立即 GetBindingPaths，回傳結果應包含剛設定的 bindingPath。
        /// **Validates: Requirements 2.3, 4.6**
        /// </summary>
        [Test]
        public void ApplyBinding_ThenGetBindingPaths_ContainsAppliedPath()
        {
            var actionArb = Arb.From(Gen.Elements(
                GameplayAction.Move,
                GameplayAction.Jump,
                GameplayAction.Attack,
                GameplayAction.Interact));

            var deviceArb = Arb.From(Gen.Elements(
                InputDeviceType.KeyboardMouse,
                InputDeviceType.Gamepad));

            Prop.ForAll(actionArb, deviceArb, (action, deviceType) =>
            {
                // 根據裝置類型選擇路徑集合
                string[] paths = deviceType == InputDeviceType.KeyboardMouse
                    ? KeyboardMousePaths
                    : GamepadPaths;

                var pathArb = Arb.From(Gen.Elements(paths));

                return Prop.ForAll(pathArb, bindingPath =>
                {
                    var asset = CreateTestAsset();
                    try
                    {
                        var storage = Substitute.For<IBindingStorage>();
                        storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);

                        var manager = new BindingManager();
                        manager.Init(asset, storage);

                        manager.ApplyBinding(action, bindingPath, deviceType);
                        var result = manager.GetBindingPaths(action, deviceType);

                        manager.Release();

                        return result.Contains(bindingPath).Label(
                            $"action={action}, device={deviceType}, path={bindingPath}, " +
                            $"result=[{string.Join(", ", result)}]");
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(asset);
                    }
                });
            }).QuickCheckThrowOnFailure();
        }

        private static InputActionAsset CreateTestAsset()
        {
            var asset = InputActionAsset.FromJson(@"{
                ""name"": ""TestAsset"",
                ""maps"": [{
                    ""name"": ""Gameplay"",
                    ""actions"": [
                        { ""name"": ""Move"", ""type"": ""Value"" },
                        { ""name"": ""Jump"", ""type"": ""Button"" },
                        { ""name"": ""Attack"", ""type"": ""Button"" },
                        { ""name"": ""Interact"", ""type"": ""Button"" }
                    ],
                    ""bindings"": [
                        { ""path"": ""<Keyboard>/w"", ""action"": ""Move"", ""groups"": ""Keyboard&Mouse"" },
                        { ""path"": ""<Gamepad>/leftStick"", ""action"": ""Move"", ""groups"": ""Gamepad"" },
                        { ""path"": ""<Keyboard>/space"", ""action"": ""Jump"", ""groups"": ""Keyboard&Mouse"" },
                        { ""path"": ""<Gamepad>/buttonSouth"", ""action"": ""Jump"", ""groups"": ""Gamepad"" },
                        { ""path"": ""<Keyboard>/j"", ""action"": ""Attack"", ""groups"": ""Keyboard&Mouse"" },
                        { ""path"": ""<Gamepad>/buttonWest"", ""action"": ""Attack"", ""groups"": ""Gamepad"" },
                        { ""path"": ""<Keyboard>/e"", ""action"": ""Interact"", ""groups"": ""Keyboard&Mouse"" },
                        { ""path"": ""<Gamepad>/buttonNorth"", ""action"": ""Interact"", ""groups"": ""Gamepad"" }
                    ]
                }],
                ""controlSchemes"": [
                    { ""name"": ""Keyboard&Mouse"", ""bindingGroup"": ""Keyboard&Mouse"", ""devices"": [{ ""devicePath"": ""<Keyboard>"" }, { ""devicePath"": ""<Mouse>"" }] },
                    { ""name"": ""Gamepad"", ""bindingGroup"": ""Gamepad"", ""devices"": [{ ""devicePath"": ""<Gamepad>"" }] }
                ]
            }");
            return asset;
        }
    }
}
