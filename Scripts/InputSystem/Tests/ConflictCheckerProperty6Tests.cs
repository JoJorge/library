namespace InputSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using FsCheck;
    using FsCheck.Fluent;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// ConflictChecker 的屬性導向測試：Property 6 — Conflict Scope Isolation。
    /// 驗證跨 InputContext 的相同 Binding Path 不被視為衝突。
    /// </summary>
    [TestFixture]
    public class ConflictCheckerProperty6Tests
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

        /// <summary>
        /// Property 6: Conflict Scope Isolation — 對於任意兩個不同的 InputContext，
        /// 即使兩個 Context 下各自存在綁定至相同 Binding Path 的 Action，
        /// 跨 Context 的綁定不應被 CheckConflict 視為衝突。
        /// **Validates: Requirements 3.4**
        /// </summary>
        [Test]
        public void CrossContext_SameBindingPath_DoesNotReportConflict()
        {
            var pathIndexArb = Arb.From(Gen.Choose(0, ValidKeyboardPaths.Length - 1));

            Prop.ForAll(pathIndexArb, pathIndex =>
            {
                string sharedPath = ValidKeyboardPaths[pathIndex];

                // 建立含兩個 ActionMap 的 InputActionAsset，各自屬於不同 Context
                var asset = InputActionAsset.FromJson(@"{
                    ""name"": ""TestAsset"",
                    ""maps"": [
                        {
                            ""name"": ""Gameplay"",
                            ""actions"": [
                                { ""name"": ""Jump"", ""type"": ""Button"" }
                            ],
                            ""bindings"": [
                                { ""path"": """ + sharedPath + @""", ""action"": ""Jump"", ""groups"": ""Keyboard&Mouse"" }
                            ]
                        },
                        {
                            ""name"": ""MainMenu"",
                            ""actions"": [
                                { ""name"": ""Navigate"", ""type"": ""Button"" }
                            ],
                            ""bindings"": [
                                { ""path"": """ + sharedPath + @""", ""action"": ""Navigate"", ""groups"": ""Keyboard&Mouse"" }
                            ]
                        }
                    ],
                    ""controlSchemes"": [
                        { ""name"": ""Keyboard&Mouse"", ""bindingGroup"": ""Keyboard&Mouse"", ""devices"": [{ ""devicePath"": ""<Keyboard>"" }, { ""devicePath"": ""<Mouse>"" }] },
                        { ""name"": ""Gamepad"", ""bindingGroup"": ""Gamepad"", ""devices"": [{ ""devicePath"": ""<Gamepad>"" }] }
                    ]
                }");

                try
                {
                    // 設定 mock context handlers
                    var contextHandlers = new Dictionary<InputContext, AbsContextHandler>();
                    var gameplayHandler = Substitute.For<AbsContextHandler>();
                    gameplayHandler.Context.Returns(InputContext.Gameplay);
                    var mainMenuHandler = Substitute.For<AbsContextHandler>();
                    mainMenuHandler.Context.Returns(InputContext.MainMenu);
                    contextHandlers[InputContext.Gameplay] = gameplayHandler;
                    contextHandlers[InputContext.MainMenu] = mainMenuHandler;

                    var checker = new ConflictChecker();
                    checker.Init(asset, contextHandlers);

                    // 檢查 GameplayAction.Jump：不應包含 MainMenuAction 的衝突
                    var gameplayResult = checker.CheckConflict(
                        GameplayAction.Jump,
                        sharedPath,
                        InputDeviceType.KeyboardMouse);

                    if (gameplayResult == null)
                    {
                        return false.Label("CheckConflict for GameplayAction returned null (error)");
                    }

                    // 驗證結果中不包含任何 MainMenuAction
                    foreach (var conflict in gameplayResult)
                    {
                        foreach (var conflicting in conflict.ConflictingActions)
                        {
                            if (conflicting is MainMenuAction)
                            {
                                return false.Label(
                                    $"GameplayAction.Jump conflict includes MainMenuAction.{conflicting} at path={sharedPath}");
                            }
                        }
                    }

                    // 檢查 MainMenuAction.Navigate：不應包含 GameplayAction 的衝突
                    var mainMenuResult = checker.CheckConflict(
                        MainMenuAction.Navigate,
                        sharedPath,
                        InputDeviceType.KeyboardMouse);

                    if (mainMenuResult == null)
                    {
                        return false.Label("CheckConflict for MainMenuAction returned null (error)");
                    }

                    // 驗證結果中不包含任何 GameplayAction
                    foreach (var conflict in mainMenuResult)
                    {
                        foreach (var conflicting in conflict.ConflictingActions)
                        {
                            if (conflicting is GameplayAction)
                            {
                                return false.Label(
                                    $"MainMenuAction.Navigate conflict includes GameplayAction.{conflicting} at path={sharedPath}");
                            }
                        }
                    }

                    checker.Release();
                    return true.Label("cross-context isolation holds");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }).QuickCheckThrowOnFailure();
        }
    }
}
