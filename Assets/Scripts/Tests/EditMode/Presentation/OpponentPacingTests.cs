using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Match;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public class OpponentPacingTests
    {
        private AnimationSettings _settings;

        [SetUp]
        public void SetUp() => _settings = ScriptableObject.CreateInstance<AnimationSettings>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_settings);

        [Test]
        public void AbilityMovingEnemyCreature_KeepsInitiatorAndAbility()
        {
            var events = new GameEvent[] {
                new AbilityPlayed(PlayerId.Player2, new CardRef(10, "ability")),
                new CreatureMoved(PlayerId.Player1, 20, 1, 2),
                new FieldOpened(2, "field") };
            var cue = OpponentActionCue.FromBatch(events, id => PlayerId.Player1);
            Assert.AreEqual(PlayerId.Player2, cue.Player);
            Assert.AreEqual(10, cue.CardId);
        }

        [Test]
        public void LocalAbilityWithOpponentEffects_DoesNotGetOpponentPause()
        {
            var calls = 0;
            var presenter = Presenter((id, seconds) => { calls++; return new ActionStep(() => { }); });
            var step = presenter.CreateStep(new GameEvent[] {
                new AbilityPlayed(PlayerId.Player1, new CardRef(10, "ability")),
                new CreatureMoved(PlayerId.Player2, 20, 1, 2) }, PlayerId.Player1, id => PlayerId.Player2);
            Assert.IsNull(step);
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void OneCommandWithManyEvents_ProducesOnePause()
        {
            var calls = 0;
            var presenter = Presenter((id, seconds) => {
                calls++;
                Assert.AreEqual(10, id);
                Assert.AreEqual(_settings.OpponentActionSeconds, seconds);
                return new ActionStep(() => { });
            });
            Assert.IsNotNull(presenter.CreateStep(new GameEvent[] {
                new CreaturePlaced(PlayerId.Player2, new CardRef(10, "creature"), 1),
                new PlayerReady(PlayerId.Player2), new TurnEnded(PlayerId.Player2) }, PlayerId.Player1, id => null));
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Ready_PausesWithoutInventingACard()
        {
            int? card = 999;
            var presenter = Presenter((id, seconds) => { card = id; return new ActionStep(() => { }); });
            Assert.IsNotNull(presenter.CreateStep(new GameEvent[] { new PlayerReady(PlayerId.Player2) }, PlayerId.Player1, id => null));
            Assert.IsNull(card);
        }

        [Test]
        public void FieldOpened_UsesOwnerOnlyWhenNotAnAbilityEffect()
        {
            var cue = OpponentActionCue.FromBatch(new GameEvent[] { new FieldOpened(5, "field") }, id => PlayerId.Player2);
            Assert.AreEqual(PlayerId.Player2, cue.Player);
            Assert.AreEqual(5, cue.CardId);
        }

        [Test]
        public void CardReturnedToHiddenHand_IsNotHighlighted()
        {
            var cue = OpponentActionCue.FromBatch(new GameEvent[] {
                new AbilityReturnedToHand(PlayerId.Player2, new CardRef(10, "private")) }, id => null);
            Assert.AreEqual(PlayerId.Player2, cue.Player);
            Assert.IsNull(cue.CardId);
        }

        [Test]
        public void TurnStartedAlone_IsNotAnOpponentAction()
        {
            Assert.IsNull(OpponentActionCue.FromBatch(new GameEvent[] { new TurnStarted(PlayerId.Player2) }, id => null));
        }

        [Test]
        public void HotSeat_DoesNotAddPacing()
        {
            var presenter = new OpponentPacingPresenter(MatchMode.HotSeat("p1", "p2"), _settings,
                (id, seconds) => { Assert.Fail("Hot-seat must not create a pause"); return null; });
            Assert.IsNull(presenter.CreateStep(new GameEvent[] { new PlayerReady(PlayerId.Player2) }, PlayerId.Player1, id => null));
        }

        [Test]
        public void ZeroDuration_DisablesPacing()
        {
            var data = new SerializedObject(_settings);
            data.FindProperty("_opponentActionSeconds").floatValue = 0f;
            data.ApplyModifiedPropertiesWithoutUndo();
            var presenter = Presenter((id, seconds) => { Assert.Fail("Disabled pacing must not create a step"); return null; });
            Assert.IsNull(presenter.CreateStep(new GameEvent[] { new PlayerReady(PlayerId.Player2) }, PlayerId.Player1, id => null));
        }

        [Test]
        public void FastAnimations_ShortenEffectivePause()
        {
            var normal = _settings.OpponentActionSeconds / AnimationSpeed.TimeScale(_settings, false, false);
            var fast = _settings.OpponentActionSeconds / AnimationSpeed.TimeScale(_settings, true, false);
            var tapped = _settings.OpponentActionSeconds / AnimationSpeed.TimeScale(_settings, true, true);
            Assert.Less(fast, normal);
            Assert.Less(tapped, fast);
        }

        private OpponentPacingPresenter Presenter(System.Func<int?, float, IAnimationStep> create) =>
            new OpponentPacingPresenter(MatchMode.VersusOpponent(PlayerId.Player1, "you", "opponent"), _settings, create);
    }
}
