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
    /// ConflictChecker 的屬性導向測試（Property-Based Testing）。
    /// </summary>
    [TestFixture]
    public class ConflictCheckerPropertyTests
    {
        private static readonly string[] ValidKeyboardPaths =
        {
            "<Keyboard>/a",
            "<Keyboard>/b",
            "<Keyboard>/c",
            "<Keyboard>/d",
            "<Keyboard>/e",
            "<Keyboard>/f",
            "<Keyboard>/space",
            "<Keyboard>/enter",
            "<Keyboard>/leftShift",
            "<Keyboard>/rightShift",
            "<Keyboard>/tab",
        };

        private static readonly GameplayAction[] Actions =
        {
            GameplayAction.Jump,
            GameplayAction.Attack,
            GameplayAction.Interact,
        };

        /// <summary>
        /// Property 3: Conflict Symmetry — 衝突檢測對稱性。
        /// 對於任意兩個動作列舉值 A 和 B（屬於相同 InputContext），若 A 綁定至路徑 P 時
        /// CheckConflict 回傳 B 為衝突項，則 B 綁定至路徑 P 時 CheckConflict 也應包含 A 為衝突項。
        /// **Validates: Requirements 3.1, 3.2**
        /// </summary>
        [Test]
        public void CheckConflict_IsSymmetric_ForSameContextActions()
        {
            // 生成隨機的 action 配對索引（兩個不同的 action）與 binding path 索引
            var pairArb = Arb.From(
                from firstIdx in Gen.Choose(0, Actions.Length - 1)
                from secondIdx in Gen.Choose(0, Actions.Length - 2).Select(i => i >= firstIdx ? i + 1 : i)
                from pathIdx in Gen.Choose(0, ValidKeyboardPaths.Length - 1)
                select new { FirstIdx = firstIdx, SecondIdx = secondIdx, PathIdx = pathIdx });

            Prop.ForAll(pairArb, data =>
            {
                GameplayAction actionA = Actions[data.FirstIdx];
                GameplayAction actionB = Actions[data.SecondIdx];
                string sharedPath = ValidKeyboardPaths[data.PathIdx];

                InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
                try
                {
                    // 建立 ActionMap，為兩個 action 設定相同 binding path 模擬衝突
                    var actionMap = asset.AddActionMap("Gameplay");
                    var inputActionA = actionMap.AddAction(actionA.ToString(), InputActionType.Button);
                    inputActionA.AddBinding(sharedPath, groups: "Keyboard&Mouse");

                    var inputActionB = actionMap.AddAction(actionB.ToString(), InputActionType.Button);
                    inputActionB.AddBinding(sharedPath, groups: "Keyboard&Mouse");

                    // 建立 mock context handler
                    var mockHandler = Substitute.For<AbsContextHandler>();
                    mockHandler.Context.Returns(InputContext.Gameplay);
                    var contextHandlers = new Dictionary<InputContext, AbsContextHandler>
                    {
                        { InputContext.Gameplay, mockHandler },
                    };

                    // 初始化 ConflictChecker
                    var checker = new ConflictChecker();
                    checker.Init(asset, contextHandlers);

                    // 檢查 A 綁定至 sharedPath 是否回傳 B 為衝突
                    var conflictsForA = checker.CheckConflict(actionA, sharedPath, InputDeviceType.KeyboardMouse);
                    if (conflictsForA == null || conflictsForA.Count == 0)
                    {
                        return false.Label($"CheckConflict({actionA}) should detect conflict with {actionB}");
                    }

                    bool aSeesB = conflictsForA.Any(c =>
                        c.ConflictingActions.Any(e => e.Equals(actionB)));

                    // 檢查 B 綁定至 sharedPath 是否回傳 A 為衝突
                    var conflictsForB = checker.CheckConflict(actionB, sharedPath, InputDeviceType.KeyboardMouse);
                    if (conflictsForB == null || conflictsForB.Count == 0)
                    {
                        return false.Label($"CheckConflict({actionB}) should detect conflict with {actionA}");
                    }

                    bool bSeesA = conflictsForB.Any(c =>
                        c.ConflictingActions.Any(e => e.Equals(actionA)));

                    checker.Release();

                    return (aSeesB && bSeesA).Label(
                        $"Symmetry: A({actionA})→B({actionB})={aSeesB}, B({actionB})→A({actionA})={bSeesA}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }).QuickCheckThrowOnFailure();
        }
    }
}
