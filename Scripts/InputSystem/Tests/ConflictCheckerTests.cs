namespace InputSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using NSubstitute;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// ConflictChecker 的單元測試，驗證衝突檢查、事件通知與全域掃描行為。
    /// </summary>
    [TestFixture]
    public class ConflictCheckerTests
    {
        private ConflictChecker _checker;
        private InputActionAsset _asset;

        [SetUp]
        public void Setup()
        {
            this._checker = new ConflictChecker();
        }

        [TearDown]
        public void TearDown()
        {
            this._checker.Release();
            if (this._asset != null)
            {
                UnityEngine.Object.DestroyImmediate(this._asset);
                this._asset = null;
            }
        }

        /// <summary>
        /// 未註冊 Action 所屬 Context 時，CheckConflict 應回傳 null（Req 3.6）。
        /// </summary>
        [Test]
        public void CheckConflict_UnregisteredContext_ReturnsNull()
        {
            this._asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = this._asset.AddActionMap("Gameplay");
            map.AddAction("Jump", InputActionType.Button)
                .AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");

            // 使用不含 Gameplay 的空 context handlers 字典
            var contextHandlers = new Dictionary<InputContext, AbsContextHandler>();

            this._checker.Init(this._asset, contextHandlers);

            var result = this._checker.CheckConflict(
                GameplayAction.Jump,
                "<Keyboard>/space",
                InputDeviceType.KeyboardMouse);

            Assert.IsNull(result);
        }

        /// <summary>
        /// 動態綁定後呼叫 NotifyConflict 應觸發 OnConflictDetected 事件（Req 3.3）。
        /// </summary>
        [Test]
        public void NotifyConflict_WithConflict_FiresOnConflictDetected()
        {
            this._asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = this._asset.AddActionMap("Gameplay");
            map.AddAction("Jump", InputActionType.Button)
                .AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
            map.AddAction("Attack", InputActionType.Button)
                .AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");

            var gameplayHandler = Substitute.For<AbsContextHandler>();
            gameplayHandler.Context.Returns(InputContext.Gameplay);
            var contextHandlers = new Dictionary<InputContext, AbsContextHandler>
            {
                { InputContext.Gameplay, gameplayHandler },
            };

            this._checker.Init(this._asset, contextHandlers);

            Enum firedAction = null;
            IReadOnlyList<Enum> firedConflicts = null;
            this._checker.OnConflictDetected += (action, conflicts) =>
            {
                firedAction = action;
                firedConflicts = conflicts;
            };

            this._checker.NotifyConflict(
                GameplayAction.Jump,
                "<Keyboard>/space",
                InputDeviceType.KeyboardMouse);

            Assert.IsNotNull(firedAction);
            Assert.AreEqual(GameplayAction.Jump, firedAction);
            Assert.IsNotNull(firedConflicts);
            Assert.AreEqual(1, firedConflicts.Count);
            Assert.AreEqual(GameplayAction.Attack, firedConflicts[0]);
        }

        /// <summary>
        /// 全域掃描應涵蓋所有衝突，且不包含無衝突的 Action（Req 3.5）。
        /// </summary>
        [Test]
        public void ScanAllConflicts_ReturnsAllConflictingActions()
        {
            this._asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = this._asset.AddActionMap("Gameplay");
            map.AddAction("Jump", InputActionType.Button)
                .AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
            map.AddAction("Attack", InputActionType.Button)
                .AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
            map.AddAction("Interact", InputActionType.Button)
                .AddBinding("<Keyboard>/e", groups: "Keyboard&Mouse");

            var gameplayHandler = Substitute.For<AbsContextHandler>();
            gameplayHandler.Context.Returns(InputContext.Gameplay);
            var contextHandlers = new Dictionary<InputContext, AbsContextHandler>
            {
                { InputContext.Gameplay, gameplayHandler },
            };

            this._checker.Init(this._asset, contextHandlers);

            var results = this._checker.ScanAllConflicts(InputDeviceType.KeyboardMouse);

            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);

            // 驗證衝突都是針對 <Keyboard>/space
            foreach (var conflict in results)
            {
                Assert.AreEqual("<Keyboard>/space", conflict.BindingPath);
                Assert.AreEqual(1, conflict.ConflictingActions.Count);
            }

            // 收集所有涉及衝突的 Action
            var involvedActions = new List<Enum>();
            foreach (var conflict in results)
            {
                involvedActions.Add(conflict.Action);
            }

            Assert.That(involvedActions, Has.Member(GameplayAction.Jump));
            Assert.That(involvedActions, Has.Member(GameplayAction.Attack));
            Assert.That(involvedActions, Has.No.Member(GameplayAction.Interact));
        }
    }
}
