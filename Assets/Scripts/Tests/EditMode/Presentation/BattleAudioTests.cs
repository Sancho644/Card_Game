using System;
using System.Linq;
using DG.Tweening;
using Game.Core;
using Game.Core.Battle;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.State;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Audio;
using Game.Presentation.Cards;
using Game.Presentation.Match;
using Game.Presentation.Table;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    public sealed class BattleAudioTests
    {
        private const PlayerId Host = PlayerId.Player1;
        private const PlayerId Guest = PlayerId.Player2;
        private const string Altar = "field.sacrificial-altar";
        private const string Weak = "creature.stone-beetle";
        private const string Strong = "creature.prince-of-abyss";
        private GameObject _root;
        private AnimationSettings _animation;
        private VisualSettings _visual;
        private CardDatabase _database;
        private RecordingSoundPlayer _sound;
        private CardFaceBuilder _faces;
        private CardViewRegistry _cards;
        private TableSync _sync;
        private MatchEngine _engine;
        private FieldSlot _field;

        [SetUp]
        public void SetUp()
        {
            _database = Load<CardDatabase>("Assets/Data/CardDatabase.asset");
            _animation = Load<AnimationSettings>("Assets/Data/Config/AnimationSettings.asset");
            _visual = Load<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
            var texts = Load<UiTexts>("Assets/Data/Config/UiTexts.asset");
            _root = new GameObject("Battle audio test");
            _faces = new CardFaceBuilder(_database, _visual, texts);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab").GetComponent<CardView>();
            _cards = new CardViewRegistry(prefab, _root.transform, _faces, new CardSkin(_visual));
            var layout = _root.AddComponent<TableLayout>();
            layout.Configure(Load<TablePresentationSettings>("Assets/Data/Config/TablePresentation.asset"), null);
            _sound = new RecordingSoundPlayer();
            _sync = new TableSync(layout, _cards, _animation, _visual, texts, _sound);
        }

        [TearDown]
        public void TearDown()
        {
            DOTween.KillAll();
            Object.DestroyImmediate(_root);
        }

        [TestCase(20, 0, new[] { SoundCue.PowerUp })]
        [TestCase(20, 40, new[] { SoundCue.PowerUp })]
        [TestCase(-20, -40, new[] { SoundCue.PowerDown })]
        [TestCase(20, -40, new[] { SoundCue.PowerUp, SoundCue.PowerDown })]
        public void PowerSoundsOncePerDirectionAndDoesNotReplayTheSameSnapshot(int hostDelta, int guestDelta, SoundCue[] expected)
        {
            StartBattle(guestWins: false);
            Sync(instant: true).Kill();
            var view = _engine.GetView(Host);
            var context = new PresentationContext(Array.Empty<GameEvent>(), view, view, new AnimationQueue(), null);
            var presenter = new BattlePowerPresenter(_cards, null, new PowerDiff(new CardNameLookup(_faces)), _visual, _animation, _sound);

            presenter.Present(Power(180, 160), context);
            CollectionAssert.IsEmpty(_sound.Calls, "The first display must be silent.");
            var changed = Power(180 + hostDelta, 160 + guestDelta);
            presenter.Present(changed, context);
            DOTween.CompleteAll();
            CollectionAssert.AreEqual(expected, _sound.Calls);
            presenter.Present(changed, context);
            CollectionAssert.AreEqual(expected, _sound.Calls, "Replaying an unchanged count must be silent.");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FieldSoundStartsWithItsMovementAndIgnoresRestorationAndReplay(bool instant)
        {
            StartBattle(guestWins: false);
            Sync(instant: true).Kill();
            var position = _cards.Find(_field.Id).transform.position;
            var before = _engine.GetView(Host);
            Submit(new Ready(Host));
            var events = Submit(new Ready(Guest)).Events;
            var after = _engine.GetView(Host);
            Assert.IsTrue(after.Field(_field.Id).IsRemoved);
            var duplicated = events.Concat(events).ToArray();
            var removals = BattleOutcomeAudio.RemovedFields(duplicated, before, after);
            CollectionAssert.AreEquivalent(new[] { _field.Id }, removals);

            var move = Sync(instant, removals).SetUpdate(UpdateType.Manual);
            CollectionAssert.IsEmpty(_sound.Calls, "Creating the tween must be silent.");
            DOTween.ManualUpdate(.01f, .01f);
            Assert.AreNotEqual(position, _cards.Find(_field.Id).transform.position);
            var expected = instant ? Array.Empty<SoundCue>() : new[] { SoundCue.FieldRemove };
            CollectionAssert.AreEqual(expected, _sound.Calls);
            move.Complete();
            Sync(instant: false, removals).SetUpdate(UpdateType.Manual);
            DOTween.ManualUpdate(1f, 1f);
            CollectionAssert.IsEmpty(BattleOutcomeAudio.RemovedFields(duplicated, after, after));
            CollectionAssert.AreEqual(expected, _sound.Calls);
        }

        [Test]
        public void WinningGuestOnAltarKeepsItsFieldAndProducesNoRemovalCue()
        {
            StartBattle(guestWins: true);
            Sync(instant: true).Kill();
            var before = _engine.GetView(Host);
            Submit(new Ready(Host));
            var events = Submit(new Ready(Guest)).Events;
            var after = _engine.GetView(Host);
            Assert.IsTrue(after.Field(_field.Id).IsOpen);
            Assert.IsTrue(after.Field(_field.Id).IsActive);
            Assert.That(events.OfType<CreatureOut>().Count(), Is.EqualTo(1));
            var removals = BattleOutcomeAudio.RemovedFields(events, before, after);
            CollectionAssert.IsEmpty(removals);
            Sync(instant: false, removals).SetUpdate(UpdateType.Manual);
            DOTween.ManualUpdate(1f, 1f);
            CollectionAssert.IsEmpty(_sound.Calls);
        }

        private void StartBattle(bool guestWins)
        {
            var deck = new DeckList(new[]
            {
                "field.old-altar", "field.forge", Altar,
                Weak, "creature.whirlwind-griffin", Strong,
                "ability.heat", "ability.break-condition", "ability.retreat"
            });
            var rules = Load<GameRulesConfig>("Assets/Data/Config/GameRules.asset").ToRules();
            _engine = new MatchEngine(new MatchSetup(deck, deck), _database.ToCatalog(), rules, new XorShiftRandom(1));
            _engine.Start();
            foreach (var player in PlayerIds.All)
                Submit(new ChooseCard(player, _engine.State.Fields.Single(field => field.Owner == player && field.Definition.Id == Altar).Id));
            var coin = _engine.State.CoinTossWinner.Value;
            Submit(new ChooseTurnOrder(coin, coin == Host));
            _field = _engine.State.Fields.Single(field => field.Owner == Host && field.Definition.Id == Altar);
            var host = _engine.GetView(Host).Hand.Single(card => card.CardId == (guestWins ? Weak : Strong));
            var guest = _engine.GetView(Guest).Hand.Single(card => card.CardId == (guestWins ? Strong : Weak));
            Submit(new PlaceCreature(Host, host.InstanceId, _field.Id));
            Submit(new EndTurn(Host));
            Submit(new PlaceCreature(Guest, guest.InstanceId, _field.Id));
            Submit(new OpenField(Host));
        }

        private CommandResult Submit(GameCommand command)
        {
            var result = _engine.Execute(command);
            Assert.IsTrue(result.IsAccepted, command.GetType().Name + ": " + result.Reason);
            return result;
        }

        private BattlePower Power(int host, int guest) => new BattlePower(_field.Id, Host,
            new PowerBreakdown(180, 0, Array.Empty<PowerContribution>(), host),
            new PowerBreakdown(160, 0, Array.Empty<PowerContribution>(), guest), ComparisonRule.HigherPowerWins, null);

        private Tween Sync(bool instant, System.Collections.Generic.HashSet<int> removals = null) =>
            _sync.Sync(_engine.GetView(Host), _engine.GetView(Guest), instant, fieldRemovals: removals);

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
