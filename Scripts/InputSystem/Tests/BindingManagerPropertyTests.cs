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
    /// BindingManager 的屬性導向測試（Property-Based Testing）。
    /// 使用 FsCheck 生成隨機綁定組合，驗證儲存與載入的往返一致性。
    /// </summary>
    [TestFixture]
    public class BindingManagerPropertyTests
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
        /// Property 2: Binding Override Round-Trip — 綁定儲存載入往返一致性。
        /// 對於任意合法的綁定覆寫操作，執行 SaveBindings 後再執行 LoadBindings，
        /// 所得到的各 Action 綁定路徑應與儲存前完全一致。
        /// **Validates: Requirements 2.4, 2.6**
        /// </summary>
        [Test]
        public void SaveThenLoad_BindingPathsRemainConsistent()
        {
            // 為每個 Action 生成一個隨機 keyboard path 的陣列（索引對應 Actions）
            var pathIndexArb = Arb.From(Gen.ArrayOf(
                Gen.Choose(0, ValidKeyboardPaths.Length - 1),
                Actions.Length));

            Prop.ForAll(pathIndexArb, pathIndices =>
            {
                InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
                try
                {
                    // 建立 ActionMap 並加入對應 GameplayAction 的 Actions
                    var actionMap = asset.AddActionMap("Gameplay");
                    for (int i = 0; i < Actions.Length; i++)
                    {
                        string actionName = Actions[i].ToString();
                        var inputAction = actionMap.AddAction(actionName, InputActionType.Button);
                        inputAction.AddBinding("<Keyboard>/z", groups: "Keyboard&Mouse");
                    }

                    // 設定 mock storage：捕獲 Save 的 JSON，Load 時回傳
                    string capturedJson = null;
                    var storage = Substitute.For<IBindingStorage>();
                    storage.Save(Arg.Any<InputDeviceType>(), Arg.Any<string>())
                        .Returns(true)
                        .AndDoes(ci => capturedJson = ci.ArgAt<string>(1));
                    storage.Load(InputDeviceType.KeyboardMouse)
                        .Returns(_ => capturedJson);
                    storage.Exists(Arg.Any<InputDeviceType>()).Returns(false);

                    // 初始化 BindingManager
                    var manager = new BindingManager();
                    manager.Init(asset, storage);

                    // 套用隨機綁定
                    for (int i = 0; i < Actions.Length; i++)
                    {
                        string newPath = ValidKeyboardPaths[pathIndices[i]];
                        manager.ApplyBinding(Actions[i], newPath, InputDeviceType.KeyboardMouse);
                    }

                    // 儲存前記錄各 Action 的 BindingPaths
                    var pathsBeforeSave = new Dictionary<GameplayAction, IReadOnlyList<string>>();
                    for (int i = 0; i < Actions.Length; i++)
                    {
                        pathsBeforeSave[Actions[i]] = manager.GetBindingPaths(Actions[i], InputDeviceType.KeyboardMouse);
                    }

                    // 執行儲存
                    bool saveResult = manager.SaveBindings(InputDeviceType.KeyboardMouse);
                    if (!saveResult)
                    {
                        return false.Label("SaveBindings failed");
                    }

                    // 重置所有 override（模擬重新啟動情境）
                    asset.RemoveAllBindingOverrides();

                    // 執行載入
                    bool loadResult = manager.LoadBindings(InputDeviceType.KeyboardMouse);
                    if (!loadResult)
                    {
                        return false.Label("LoadBindings failed");
                    }

                    // 驗證載入後各 Action 的 BindingPaths 與儲存前一致
                    for (int i = 0; i < Actions.Length; i++)
                    {
                        var pathsAfterLoad = manager.GetBindingPaths(Actions[i], InputDeviceType.KeyboardMouse);
                        var expected = pathsBeforeSave[Actions[i]];

                        if (expected.Count != pathsAfterLoad.Count)
                        {
                            return false.Label(
                                $"Action={Actions[i]}: count mismatch before={expected.Count} after={pathsAfterLoad.Count}");
                        }

                        for (int j = 0; j < expected.Count; j++)
                        {
                            if (expected[j] != pathsAfterLoad[j])
                            {
                                return false.Label(
                                    $"Action={Actions[i]}[{j}]: expected={expected[j]} actual={pathsAfterLoad[j]}");
                            }
                        }
                    }

                    manager.Release();
                    return true.Label("round-trip consistent");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }).QuickCheckThrowOnFailure();
        }
    }
}
