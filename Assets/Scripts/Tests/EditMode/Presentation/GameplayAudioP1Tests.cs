using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.State;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Cards;
using Game.Presentation.Match;
using Game.Presentation.Network;
using Game.Presentation.Table;
using Game.Services.Match;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    public sealed class GameplayAudioP1Tests
    {
        private const PlayerId Host = PlayerId.Player1;
        private const PlayerId Guest = PlayerId.Player2;
        private const string Altar = "field.old-altar";
        private const string Light = "creature.radiant-stag";
        private const string Earth = "creature.stone-beetle";
        private const string Wind = "creature.storm-swift";
        private const string Common = "ability.home-ground";
        private const string Epic = "ability.intercept";
        private const string Legendary = "ability.retreat";

        private UpdateType _previousUpdateType;
        private float _previousSpeed;
        private GameObject _root;
        private CardDatabase _database;
        private AnimationSettings _animation;
        private RecordingSoundPlayer _sound;
        private AbilityPlayedPresenter _abilities;
        private FieldOpenedPresenter _reveal;
        private TableSync _sync;
        private MatchEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _previousUpdateType = DOTween.defaultUpdateType;
            _previousSpeed = DOTween.timeScale;
            DOTween.defaultUpdateType = UpdateType.Manual;
            DOTween.timeScale = 1f;
            _database = Load<CardDatabase>("Assets/Data/CardDatabase.asset");
            _animation = Load<AnimationSettings>("Assets/Data/Config/AnimationSettings.asset");
            var visual = Load<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
            var texts = Load<UiTexts>("Assets/Data/Config/UiTexts.asset");
            _root = new GameObject("Gameplay audio test");
            var prefab = Load<GameObject>("Assets/Prefabs/Card3D.prefab").GetComponent<CardView>();
            var cards = new CardViewRegistry(prefab, _root.transform, new CardFaceBuilder(_database, visual, texts), new CardSkin(visual));
            var layout = _root.AddComponent<TableLayout>();
            layout.Configure(Load<TablePresentationSettings>("Assets/Data/Config/TablePresentation.asset"), null);
            _sound = new RecordingSoundPlayer();
            _abilities = new AbilityPlayedPresenter(layout, cards, _database, _animation, _sound);
            _reveal = new FieldOpenedPresenter(cards, _database, _animation, _sound);
            _sync = new TableSync(layout, cards, _animation, visual, texts, _sound);
        }

        [TearDown]
        public void TearDown()
        {
            DOTween.KillAll();
            DOTween.defaultUpdateType = _previousUpdateType;
            DOTween.timeScale = _previousSpeed;
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void RealCommonEpicAndLegendaryPlaysAndCancellationHaveDistinctCues()
        {
            Start(Altar, Light, Wind);
            EnterBattle(Host, Light, Wind);
            var initial = _engine.GetView(Host);
            var common = Submit(Ability(Host, Common));
            var withCommon = _engine.GetView(Host);
            Present(_abilities, common.Events, initial, withCommon);
            CollectionAssert.AreEqual(new[] { SoundCue.AbilityPlay }, _sound.Calls);

            var epic = Submit(Ability(Guest, Epic));
            Assert.That(epic.Events.OfType<AbilityCancelled>().Count(), Is.EqualTo(1));
            var withCancellation = _engine.GetView(Host);
            var doubled = epic.Events.Concat(epic.Events.OfType<AbilityCancelled>()).ToArray();
            Present(_abilities, doubled, withCommon, withCancellation);
            CollectionAssert.AreEqual(new[] { SoundCue.AbilityPlay, SoundCue.AbilityEpic }, _sound.Calls);
            AdvanceQueue();
            CollectionAssert.AreEqual(new[] { SoundCue.AbilityPlay, SoundCue.AbilityEpic, SoundCue.AbilityCancel }, _sound.Calls);
            Present(_abilities, epic.Events, withCancellation, withCancellation);
            AdvanceQueue();
            Assert.That(_sound.Calls.Count, Is.EqualTo(3), "An already cancelled effect remains silent.");

            var legendary = Submit(Ability(Host, Legendary));
            Present(_abilities, legendary.Events, withCancellation, _engine.GetView(Host));
            Assert.That(_sound.Calls.Last(), Is.EqualTo(SoundCue.AbilityLegendary));
            Assert.That(legendary.Events.OfType<AbilityCancelled>(), Is.Empty);
        }

        [TestCase("field.forge", "creature.spark-fox", Wind, SoundCue.ElementFire)]
        [TestCase("field.mirror-lake", "creature.lagoon-serpent", Earth, SoundCue.ElementWater)]
        [TestCase("field.mountain-path", Earth, Light, SoundCue.ElementEarth)]
        [TestCase("field.mountain-path", Wind, Light, SoundCue.ElementWind)]
        [TestCase(Altar, Light, Wind, SoundCue.ElementLight)]
        [TestCase(Altar, "creature.shadow-imp", Wind, SoundCue.ElementDark)]
        public void AnElementOnlySoundsAfterARealPublicReveal(string fieldName, string host, string guest, SoundCue cue)
        {
            CheckReveal(fieldName, host, guest, cue, 1f);
        }

        [TestCase(2f)]
        [TestCase(8f)]
        public void FasterAnimationKeepsTheElementAtTheEndOfTheReveal(float speed)
        {
            CheckReveal(Altar, Light, Wind, SoundCue.ElementLight, speed);
        }

        [Test]
        public void ReturnsAreLocalToTheVisibleHandAndAreConsumedByItsMovement()
        {
            Start(Altar, Light, Wind);
            EnterBattle(Host, Light, Wind);
            Submit(new OpenField(Host));
            var before = _engine.GetView(Host);
            var guestBefore = _engine.GetView(Guest);
            _sync.Sync(before, guestBefore, instant: true).Kill();
            var events = Submit(Ability(Host, Legendary)).Events;
            var after = _engine.GetView(Host);
            var guestAfter = _engine.GetView(Guest);
            var doubled = events.Concat(events).ToArray();
            var returns = CardSoundPresenter.HandReturns(doubled, before, after);
            Assert.That(returns.Count, Is.EqualTo(1));
            Assert.That(CardSoundPresenter.HandReturns(doubled, after, after), Is.Empty);
            Assert.That(CardSoundPresenter.HandReturns(
                doubled.OfType<CreatureReturnedToHand>().Where(e => e.Owner == Host).ToArray(), guestBefore, guestAfter), Is.Empty);

            _sync.Sync(after, guestAfter, instant: false, handReturns: returns);
            Assert.That(_sound.Calls, Is.Empty);
            Tick(.02f);
            CollectionAssert.AreEqual(new[] { SoundCue.CardReturn }, _sound.Calls);
            AdvanceQueue();
            _sync.Sync(after, guestAfter, instant: false, handReturns: returns);
            AdvanceQueue();
            CollectionAssert.AreEqual(new[] { SoundCue.CardReturn }, _sound.Calls);
        }

        [Test]
        public void ReturnOfPowerAnnouncesTheRecoveredCardOnlyInItsOwnersHand()
        {
            const string recovery = "ability.return-of-power";
            Start(Altar, Light, Wind, recovery);
            var home = Field(Host);
            Submit(new PlaceCreature(Host, HandId(Host, "creature.whirlwind-griffin"), Field(Guest)));
            Submit(new EndTurn(Host));
            Submit(new PlaceCreature(Guest, HandId(Guest, Wind), home));
            Submit(new EndTurn(Guest));
            Submit(new PlaceCreature(Host, HandId(Host, Light), home));
            Submit(new Ready(Guest));
            var common = Ability(Host, Common);
            Submit(common);
            Submit(new Ready(Guest));
            Submit(new Ready(Host));
            var before = _engine.GetView(Host);
            var guestBefore = _engine.GetView(Guest);
            _sync.Sync(before, guestBefore, instant: true).Kill();

            var events = Submit(new PlayAbility(Host, HandId(Host, recovery), new AbilityTargets(cardId: common.AbilityId))).Events;
            Assert.That(events.OfType<AbilityReturnedToHand>().Single().Ability.InstanceId, Is.EqualTo(common.AbilityId));
            var after = _engine.GetView(Host);
            var guestAfter = _engine.GetView(Guest);
            var returns = CardSoundPresenter.HandReturns(events, before, after);
            CollectionAssert.AreEquivalent(new[] { common.AbilityId }, returns);
            Assert.That(CardSoundPresenter.HandReturns(events, guestBefore, guestAfter), Is.Empty);

            _sync.Sync(after, guestAfter, instant: false, handReturns: returns);
            Tick(.02f);
            CollectionAssert.AreEqual(new[] { SoundCue.CardReturn }, _sound.Calls);
            Assert.That(returns, Is.Empty);
            Assert.That(CardSoundPresenter.HandReturns(events, after, after), Is.Empty);
        }

        [Test]
        public void ManeuverSoundsOnceAtTheNewFieldInsteadOfReplayingPlacement()
        {
            Start(Altar, Light, Wind, "ability.maneuver");
            var source = Field(Host);
            var target = Field(Guest);
            var creature = HandId(Host, Light);
            Submit(new PlaceCreature(Host, creature, source));
            Submit(new EndTurn(Host));
            Submit(new PlaceCreature(Guest, HandId(Guest, Wind), target));
            Submit(new EndTurn(Guest));
            var before = _engine.GetView(Host);
            _sync.Sync(before, _engine.GetView(Guest), instant: true).Kill();

            var events = Submit(new PlayAbility(Host, HandId(Host, "ability.maneuver"),
                new AbilityTargets(creatureId: creature, fieldId: target))).Events;
            var after = _engine.GetView(Host);
            var arrivals = CardSoundPresenter.Arrivals(events.Concat(events).ToArray(), before, after);
            CollectionAssert.AreEquivalent(new[] { SoundCue.CreatureShift }, arrivals.Values);
            _sync.Sync(after, _engine.GetView(Guest), instant: false, placementArrivals: arrivals);
            Tick(_animation.MoveDuration * .5f);
            Assert.That(_sound.Calls, Is.Empty);
            Tick(_animation.MoveDuration);
            CollectionAssert.AreEqual(new[] { SoundCue.CreatureShift }, _sound.Calls);
            _sync.Sync(after, _engine.GetView(Guest), instant: false, placementArrivals: arrivals);
            AdvanceQueue();
            Assert.That(CardSoundPresenter.Arrivals(events, after, after), Is.Empty);
            CollectionAssert.AreEqual(new[] { SoundCue.CreatureShift }, _sound.Calls);
        }

        [Test]
        public void DuplicatedAndStaleOnlineFramesDoNotReplayTheAbilityOrItsCancellation()
        {
            var setup = Setup(Altar, Light, Wind);
            var local = new LocalMatchSession(_database.ToCatalog(), Rules());
            var authority = new OnlineAuthority(local);
            var client = new OnlineMatchSession(Host, (_, _, _) => Assert.Fail("Unexpected client command"));
            PlayerView previous = null;
            OnlineFrame last = null;
            var requests = new long[2];
            client.EventsProduced += events =>
            {
                var next = client.GetView(Host);
                Present(_abilities, events, previous, next);
                previous = next;
            };
            authority.Updated += (player, frame) =>
            {
                if (player != Host)
                    return;
                var copied = OnlineCodec.Decode<OnlineFrame>(OnlineCodec.Encode(frame));
                client.Receive(copied);
                client.Receive(copied);
                if (last != null)
                    client.Receive(last);
                last = copied;
            };
            client.Start(null, 0);
            authority.Start(setup, 1);
            foreach (var player in PlayerIds.All)
                Send(local.GetLegalActions(player).OfType<ChooseCard>().Single(action =>
                    local.GetView(player).Field(action.CardId)?.CardId == Altar));
            var coin = local.GetView(Host).CoinTossWinner.Value;
            Send(new ChooseTurnOrder(coin, coin == Host));
            var field = local.GetView(Host).Fields.Single(slot => slot.IsActive && slot.Owner == Host).Id;
            Send(new PlaceCreature(Host, Find(Host, Light), field));
            Send(new EndTurn(Host));
            Send(new PlaceCreature(Guest, Find(Guest, Wind), field));
            Assert.That(_sound.Calls, Is.Empty, "Hidden abilities and field cards produce no rarity or element cues.");
            Send(new PlayAbility(Host, Find(Host, Common)));
            Send(new PlayAbility(Guest, Find(Guest, Epic)));
            CollectionAssert.AreEqual(new[] { SoundCue.AbilityPlay, SoundCue.AbilityEpic, SoundCue.AbilityCancel }, _sound.Calls);

            int Find(PlayerId player, string id) =>
                local.GetView(player).Hand.Single(c => c.CardId == id).InstanceId;

            void Send(GameCommand command)
            {
                var legal = local.GetLegalActions(command.Player).ToList();
                var index = legal.FindIndex(candidate => OnlineCodec.Encode(candidate) == OnlineCodec.Encode(command));
                Assert.That(index, Is.GreaterThanOrEqualTo(0), command.GetType().Name);
                authority.Submit(command.Player, ++requests[(int)command.Player], authority.Version, index);
                AdvanceQueue();
            }
        }

        [Test]
        public void NewAssignmentsHaveUniquePcmClipsAndExpectedVariants()
        {
            var settings = Load<SoundSettings>("Assets/Data/Config/SoundSettings.asset");
            var paths = new HashSet<string>();
            foreach (var cue in Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>().Where(cue => cue >= SoundCue.AbilityEpic))
            {
                var entry = settings.Find(cue);
                Assert.That(entry.Variants?.Length ?? 0, Is.EqualTo(cue == SoundCue.AbilityEpic || cue == SoundCue.AbilityLegendary ? 1 : 0), cue.ToString());
                foreach (var clip in new[] { entry.Clip }.Concat(entry.Variants ?? Array.Empty<AudioClip>()))
                {
                    Assert.That(clip, Is.Not.Null, cue.ToString());
                    Assert.That(clip.channels, Is.EqualTo(1));
                    Assert.That(clip.frequency, Is.EqualTo(44100));
                    var path = AssetDatabase.GetAssetPath(clip);
                    StringAssert.StartsWith("Assets/Audio/SFX/P1/", path);
                    Assert.That(paths.Add(path), Is.True, path);
                    var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                    foreach (var platform in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
                    {
                        Assert.That(importer.ContainsSampleSettingsOverride(platform), Is.True);
                        var sample = importer.GetOverrideSampleSettings(platform);
                        Assert.That(sample.compressionFormat, Is.EqualTo(AudioCompressionFormat.PCM));
                        Assert.That(sample.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
                        Assert.That(sample.preloadAudioData, Is.True);
                    }
                }
            }
            Assert.That(paths.Count, Is.EqualTo(16));
        }

        private void CheckReveal(string fieldName, string host, string guest, SoundCue cue, float speed)
        {
            DOTween.timeScale = speed;
            Start(fieldName, host, guest);
            EnterBattle(Host, host, guest);
            var before = _engine.GetView(Guest);
            var fieldId = Field(Host);
            Assert.That(before.Field(fieldId).CardId, Is.Null);
            _sync.Sync(before, _engine.GetView(Host), instant: true).Kill();

            var events = Submit(new OpenField(Host)).Events;
            var after = _engine.GetView(Guest);
            Present(_reveal, events, before, before, _engine.GetBattlePower());
            AdvanceQueue();
            Assert.That(_sound.Calls, Is.Empty, "An unrevealed snapshot never leaks an element.");

            _sound.Calls.Clear();
            _sync.Sync(before, _engine.GetView(Host), instant: true).Kill();
            Present(_reveal, events, before, after, _engine.GetBattlePower());
            Tick(_animation.FlipSeconds * .6f / speed);
            CollectionAssert.AreEqual(new[] { SoundCue.FieldFlip }, _sound.Calls);
            Tick(_animation.FlipSeconds * .6f / speed);
            CollectionAssert.AreEqual(new[] { SoundCue.FieldFlip, cue }, _sound.Calls);

            Present(_reveal, events, after, after, _engine.GetBattlePower());
            AdvanceQueue();
            Assert.That(_sound.Calls.Count(sound => sound == cue), Is.EqualTo(1));
        }

        private void Start(string field, string host, string guest, string legendary = Legendary)
        {
            _engine = new MatchEngine(Setup(field, host, guest, legendary), _database.ToCatalog(), Rules(), new XorShiftRandom(1));
            _engine.Start();
            foreach (var player in PlayerIds.All)
                Submit(new ChooseCard(player, _engine.State.Fields.Single(slot => slot.Owner == player && slot.Definition.Id == field).Id));
            var coin = _engine.GetView(Host).CoinTossWinner.Value;
            Submit(new ChooseTurnOrder(coin, coin == Host));
        }

        private MatchSetup Setup(string field, string host, string guest, string legendary = Legendary) =>
            new MatchSetup(Deck(field, host, legendary), Deck(field, guest, Legendary));

        private DeckList Deck(string field, string creature, string legendary)
        {
            var fields = new[] { Altar, "field.forge", "field.sacrificial-altar" };
            var creatures = new[] { Earth, "creature.whirlwind-griffin", "creature.prince-of-abyss" };
            fields[(int)_database.Find(field).Rarity] = field;
            creatures[(int)_database.Find(creature).Rarity] = creature;
            return new DeckList(fields.Concat(creatures).Concat(new[] { Common, Epic, legendary }));
        }

        private void EnterBattle(PlayerId first, string host, string guest)
        {
            var field = Field(first);
            Submit(new PlaceCreature(Host, HandId(Host, host), field));
            Submit(new EndTurn(Host));
            Submit(new PlaceCreature(Guest, HandId(Guest, guest), field));
        }

        private PlayAbility Ability(PlayerId player, string id) => new PlayAbility(player, HandId(player, id));
        private int Field(PlayerId owner) => _engine.GetView(Host).Fields.Single(slot => slot.IsActive && slot.Owner == owner).Id;
        private int HandId(PlayerId owner, string id) => _engine.GetView(owner).Hand.Single(card => card.CardId == id).InstanceId;

        private CommandResult Submit(GameCommand command)
        {
            var result = _engine.Execute(command);
            Assert.That(result.IsAccepted, Is.True, command.GetType().Name + ": " + result.Reason);
            return result;
        }

        private static Game.Core.Rules.GameRules Rules() =>
            Load<GameRulesConfig>("Assets/Data/Config/GameRules.asset").ToRules();

        private static void Present(IEventPresenter presenter, IReadOnlyList<GameEvent> events,
            PlayerView previous, PlayerView view, Game.Core.Battle.BattlePower power = null)
        {
            var context = new PresentationContext(events, previous, view, new AnimationQueue(),
                _ => new ActionStep(() => { }), power);
            for (var i = 0; i < events.Count; i++)
            {
                context.Index = i;
                presenter.Present(events[i], context);
            }
        }

        private static void AdvanceQueue()
        {
            for (var i = 0; i < 10; i++)
            {
                DOTween.CompleteAll(withCallbacks: true);
                Tick(1);
            }
        }

        private static void Tick(float seconds) => DOTween.ManualUpdate(seconds, seconds);
        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
