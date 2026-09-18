namespace InputSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FsCheck;
    using FsCheck.Fluent;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// BindingManager 的屬性導向測試（Property-Based Testing）。
    /// Property 7: Reset Idempotence — 重置操作冪等性。
    /// 驗證 ResetBinding 與 ResetAllBindings 連續執行兩次的結果一致。
    /// **Validates: Requirements 2.9, 2.10**
    /// </summary>
    [TestFixture]
    public class BindingManagerProperty7Tests
    {
        private static readonly string[] ActionNames = { "Move", "Jump", "Attack", "Interact" };

        private static readonly string[] DefaultPaths =
        {
            "<Keyboard>/w",
            "<Keyboard>/space",
            "<Keyboard>/j",
            "<Keyboard>/e",
        };

        private static readonly string[] RandomPaths =
        {
            "<Keyboard>/a",
            "<Keyboard>/b",
            "<Keyboard>/c",
            "<Keyboard>/d",
            "<Keyboard>/e",
            "<Keyboard>/f",
        };

        /// <summary>
        /// Property 7a: ResetBinding Idempotence — 對單一 Action 執行重置兩次，結果一致。
        /// **Validates: Requirements 2.9**
        /// </summary>
        [Test]
        public void ResetBinding_CalledTwice_ProducesSameResult()
        {
            var actionArb = Arb.From(Gen.Elements(
                GameplayAction.Move,
                GameplayAction.Jump,
                GameplayAction.Attack,
                GameplayAction.Interact));
            var pathArb = Arb.From(Gen.Elements(RandomPaths));

            Prop.ForAll(actionArb, pathArb, (GameplayAction action, string bindingPath) =>
            {
                var asset = CreateTestAsset();
                var storage = CreateMockStorage();
                var manager = new BindingManager();
                manager.Init(asset, storage);

                // 修改綁定
                manager.ApplyBinding(action, bindingPath, InputDeviceType.KeyboardMouse);

                // 第一次重置
                manager.ResetBinding(action, InputDeviceType.KeyboardMouse);
                var afterFirstReset = manager.GetBindingPaths(action, InputDeviceType.KeyboardMouse);

                // 第二次重置
                manager.ResetBinding(action, InputDeviceType.KeyboardMouse);
                var afterSecondReset = manager.GetBindingPaths(action, InputDeviceType.KeyboardMouse);

                manager.Release();
                var labelMsg = $"action={action}, path={bindingPath}, " +
                    $"afterFirst=[{string.Join(",", afterFirstReset)}], " +
                    $"afterSecond=[{string.Join(",", afterSecondReset)}]";
                return Prop.Label(
                    afterFirstReset.SequenceEqual(afterSecondReset), labelMsg);
            }).QuickCheckThrowOnFailure();
        }

        /// <summary>
        /// Property 7b: ResetAllBindings Idempotence — 對所有綁定執行重置兩次，結果一致。
        /// **Validates: Requirements 2.10**
        /// </summary>
        [Test]
        public void ResetAllBindings_CalledTwice_ProducesSameResult()
        {
            var pathArb = Arb.From(Gen.Elements(RandomPaths));

            Prop.ForAll(pathArb, pathArb, pathArb, (string p1, string p2, string p3) =>
            {
                // 使用三組隨機路徑 + 固定第四組以滿足四個 action 覆蓋
                string[] paths = { p1, p2, p3, RandomPaths[0] };
                var asset = CreateTestAsset();
                var storage = CreateMockStorage();
                var manager = new BindingManager();
                manager.Init(asset, storage);

                var actions = new GameplayAction[]
                {
                    GameplayAction.Move,
                    GameplayAction.Jump,
                    GameplayAction.Attack,
                    GameplayAction.Interact,
                };

                // 對每個 action 套用隨機綁定
                for (int i = 0; i < actions.Length; i++)
                {
                    manager.ApplyBinding(actions[i], paths[i], InputDeviceType.KeyboardMouse);
                }

                // 第一次重置所有綁定
                manager.ResetAllBindings(InputDeviceType.KeyboardMouse);
                var afterFirstReset = new List<IReadOnlyList<string>>();
                foreach (var action in actions)
                {
                    afterFirstReset.Add(
                        manager.GetBindingPaths(action, InputDeviceType.KeyboardMouse));
                }

                // 第二次重置所有綁定
                manager.ResetAllBindings(InputDeviceType.KeyboardMouse);
                var afterSecondReset = new List<IReadOnlyList<string>>();
                foreach (var action in actions)
                {
                    afterSecondReset.Add(
                        manager.GetBindingPaths(action, InputDeviceType.KeyboardMouse));
                }

                manager.Release();

                bool allEqual = true;
                for (int i = 0; i < actions.Length; i++)
                {
                    if (!afterFirstReset[i].SequenceEqual(afterSecondReset[i]))
                    {
                        allEqual = false;
                        break;
                    }
                }

                return Prop.Label(
                    allEqual,
                    $"paths=[{string.Join(",", paths)}], " +
                    $"firstReset counts=[{string.Join(",", afterFirstReset.Select(r => r.Count))}], " +
                    $"secondReset counts=[{string.Join(",", afterSecondReset.Select(r => r.Count))}]");
            }).QuickCheckThrowOnFailure();
        }

        private static InputActionAsset CreateTestAsset()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = asset.AddActionMap("Gameplay");

            for (int i = 0; i < ActionNames.Length; i++)
            {
                var action = map.AddAction(ActionNames[i], InputActionType.Button);
                action.AddBinding(DefaultPaths[i], groups: "Keyboard&Mouse");
            }

            return asset;
        }

        private static IBindingStorage CreateMockStorage()
        {
            var storage = Substitute.For<IBindingStorage>();
            storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);
            return storage;
        }
    }
}
