using System.Linq;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Effects;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.State;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using NUnit.Framework;
using UnityEditor;

namespace Game.Data.Tests
{
    /// <summary>The first card set (docs/cards.md) as assets, the configs and the test decks.</summary>
    public class FirstCardSetTests
    {
        private const string DatabasePath = "Assets/Data/CardDatabase.asset";
        private const string RulesPath = "Assets/Data/Config/GameRules.asset";
        private const string BalancePath = "Assets/Data/Config/Balance.asset";

        private CardDatabase _database;
        private GameRulesConfig _rules;

        [SetUp]
        public void SetUp()
        {
            _database = AssetDatabase.LoadAssetAtPath<CardDatabase>(DatabasePath);
            _rules = AssetDatabase.LoadAssetAtPath<GameRulesConfig>(RulesPath);
            Assert.IsNotNull(_database, DatabasePath);
            Assert.IsNotNull(_rules, RulesPath);
        }

        [Test]
        public void Database_HoldsWholeFirstSet_WithoutProblems()
        {
            Assert.AreEqual(16, _database.Cards.OfType<FieldDef>().Count());
            Assert.AreEqual(18, _database.Cards.OfType<CreatureDef>().Count());
            Assert.AreEqual(34, _database.Cards.OfType<AbilityDef>().Count(), "28 abilities and 6 signature abilities.");
            CollectionAssert.IsEmpty(_database.Validate().ToList());
        }

        [Test]
        public void EveryCard_ConvertsToEngineData()
        {
            var catalog = _database.ToCatalog();

            foreach (var card in _database.Cards)
            {
                Assert.IsTrue(catalog.TryGet(card.Id, out var definition), card.Id);
                Assert.AreEqual(card.Type, definition.Type, card.Id);
                Assert.AreEqual(card.Rarity, definition.Rarity, card.Id);
            }
        }

        [Test]
        public void Forge_MatchesCatalog()
        {
            var forge = (FieldCard)_database.Find("field.forge").ToCore();

            Assert.AreEqual(Rarity.Epic, forge.Rarity);
            Assert.AreEqual(60, forge.Modifiers[Element.Fire]);
            Assert.AreEqual(50, forge.Modifiers[Element.Earth]);
            Assert.AreEqual(-30, forge.Modifiers[Element.Water]);
            Assert.IsInstanceOf<NoCommonOrEpicAbilitiesCondition>(forge.Conditions.Single());
        }

        [Test]
        public void CreaturePower_IsFixedPerRarity()
        {
            foreach (var creature in _database.Cards.OfType<CreatureDef>())
            {
                var expected = creature.Rarity == Rarity.Common ? 150 : creature.Rarity == Rarity.Epic ? 200 : 250;
                Assert.AreEqual(expected, creature.Power, creature.Id);
            }
        }

        [TestCase("ability.forge-flame", "creature.lord-of-crimson-forge")]
        [TestCase("ability.ebb", "creature.mistress-of-tides")]
        [TestCase("ability.stone-will", "creature.guardian-of-first-mountain")]
        [TestCase("ability.storm-onslaught", "creature.hurricane-king")]
        [TestCase("ability.dawn", "creature.dawn-seraph")]
        [TestCase("ability.dark-price", "creature.prince-of-abyss")]
        public void SignatureAbility_IsBoundToItsCreature(string abilityId, string creatureId)
        {
            var ability = (AbilityCard)_database.Find(abilityId).ToCore();

            Assert.AreEqual(Rarity.Legendary, ability.Rarity);
            Assert.AreEqual(creatureId, ability.BoundCreatureId);
        }

        [Test]
        public void GameRulesConfig_DefaultsFollowRules()
        {
            var rules = _rules.ToRules();

            Assert.AreEqual(3, rules.WinsToWin);
            Assert.AreEqual(1, rules.LegendaryAbilitiesPerTurn);
            Assert.AreEqual(9, rules.Deck.DeckSize);
            Assert.AreEqual(10, _rules.TurnOrderChoiceSeconds);
            Assert.AreEqual(45, _rules.TurnSeconds);
            Assert.AreEqual(15, _rules.BattleDecisionSeconds);
        }

        [Test]
        public void TestDecks_AreValid()
        {
            var validator = new DeckValidator(_database.ToCatalog(), _rules.ToRules().Deck);
            var decks = LoadDecks();

            Assert.AreEqual(4, decks.Length);
            foreach (var deck in decks)
                CollectionAssert.IsEmpty(validator.Validate(deck.ToDeckList()).Errors, deck.name);
        }

        [Test]
        public void FirstSet_FitsBalanceNumbers()
        {
            var balance = AssetDatabase.LoadAssetAtPath<BalanceConfig>(BalancePath);

            CollectionAssert.IsEmpty(new CardBalanceValidator(balance).Check(_database.Cards).ToList());
        }

        [Test]
        public void RealForge_BattleWithTestDecks()
        {
            var engine = new MatchEngine(
                new MatchSetup(Deck("deck.fire-earth").ToDeckList(), Deck("deck.water-light").ToDeckList()),
                _database.ToCatalog(),
                _rules.ToRules(),
                new XorShiftRandom(1));
            engine.Start();
            var coinWinner = engine.State.CoinTossWinner.Value;
            Ok(engine, new ChooseTurnOrder(coinWinner, goFirst: coinWinner == PlayerId.Player1));

            var forge = engine.State.Fields.First(field => field.Definition.Id == "field.forge");
            Ok(engine, new PlaceCreature(PlayerId.Player1, CardId(engine, PlayerId.Player1, "creature.spark-fox"), forge.Id));
            Ok(engine, new EndTurn(PlayerId.Player1));
            Ok(engine, new PlaceCreature(PlayerId.Player2, CardId(engine, PlayerId.Player2, "creature.deep-siren"), forge.Id));
            Ok(engine, new OpenField(PlayerId.Player1));

            var heat = new PlayAbility(PlayerId.Player1, CardId(engine, PlayerId.Player1, "ability.heat"));
            Assert.AreEqual(RejectReason.BlockedByFieldCondition, engine.Execute(heat).Reason, "«Тишина» forbids common abilities.");

            Ok(engine, new Ready(PlayerId.Player1));
            var result = engine.Execute(new Ready(PlayerId.Player2)).Events.OfType<BattleResolved>().Single();
            Assert.AreEqual(150 + 60, result.HostPower);
            Assert.AreEqual(200 - 30, result.GuestPower);
            Assert.AreEqual(PlayerId.Player1, result.Winner);
        }

        private static void Ok(MatchEngine engine, GameCommand command)
        {
            var result = engine.Execute(command);
            Assert.IsTrue(result.IsAccepted, $"{command.GetType().Name}: {result.Reason}");
        }

        private static int CardId(MatchEngine engine, PlayerId owner, string cardId) =>
            engine.State.Player(owner).Hand.First(card => card.Definition.Id == cardId).Id;

        private static DeckDef Deck(string id) => LoadDecks().Single(deck => deck.Id == id);

        private static DeckDef[] LoadDecks() =>
            AssetDatabase.FindAssets("t:DeckDef")
                .Select(guid => AssetDatabase.LoadAssetAtPath<DeckDef>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
    }
}
