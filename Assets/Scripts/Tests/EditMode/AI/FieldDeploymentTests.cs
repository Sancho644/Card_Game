using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Effects;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;
using NUnit.Framework;

namespace Game.AI.Tests
{
    public sealed class FieldDeploymentTests
    {
        private const string Plain = "test.field.plain";
        private const string Fire = "test.field.fire";
        private const string Water = "test.field.water";
        private static readonly string[][] Orders =
        {
            new[] { Plain, Fire, Water }, new[] { Plain, Water, Fire },
            new[] { Fire, Plain, Water }, new[] { Fire, Water, Plain },
            new[] { Water, Plain, Fire }, new[] { Water, Fire, Plain },
        };

        [TestCase(false, PlayerId.Player1, false)]
        [TestCase(true, PlayerId.Player1, false)]
        [TestCase(false, PlayerId.Player2, false)]
        [TestCase(true, PlayerId.Player2, false)]
        public void ChoosesFieldForOwnCreatures_RegardlessOfListOrder(bool water, PlayerId viewer, bool easy)
        {
            var catalog = Catalog();
            foreach (var order in Orders)
            {
                var rules = GameRules.Default;
                var engine = Begin(catalog, Deck(water, order), Deck(water, order), rules);
                if (viewer == PlayerId.Player2)
                    Ok(engine, new ChooseCard(PlayerId.Player1, Field(engine, PlayerId.Player1, Plain)));
                var profile = new FirstCardSet();
                var difficulty = easy ? profile.Easy : profile.Normal;
                difficulty.Noise = 0;
                var chosen = Decide(engine, viewer, catalog, rules, difficulty);
                Assert.AreEqual(water ? Water : Fire, engine.State.FindField(chosen.CardId).Definition.Id,
                    string.Join(",", order));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EasyProfile_UsesSameChoiceWhenOnlyListOrderChanges(bool water)
        {
            var catalog = Catalog();
            string expected = null;
            foreach (var order in Orders)
            {
                var engine = Begin(catalog, Deck(water, order), Deck(water, order), GameRules.Default);
                var profile = new FirstCardSet().Easy;
                profile.Noise = 0;
                var choice = Decide(engine, PlayerId.Player1, catalog, GameRules.Default, profile);
                var id = engine.State.FindField(choice.CardId).Definition.Id;
                if (expected == null) expected = id;
                Assert.AreEqual(expected, id);
            }
        }

        [Test]
        public void LowerPowerCondition_CanMakeFieldBestWithoutAnElementBonus()
        {
            var catalog = Catalog(neutral: true, lowerPower: true);
            var engine = Begin(catalog, Deck(false, Orders[0]), Deck(true, Orders[0]), GameRules.Default);
            var chosen = Decide(engine, PlayerId.Player1, catalog, GameRules.Default, new FirstCardSet().Normal);
            Assert.AreEqual(Water, engine.State.FindField(chosen.CardId).Definition.Id);
        }

        [Test]
        public void EqualFields_UseStableTieBreakInsteadOfFirstListedCard()
        {
            var catalog = Catalog(neutral: true);
            foreach (var order in Orders)
            {
                var engine = Begin(catalog, Deck(false, order), Deck(true, order), GameRules.Default);
                var chosen = Decide(engine, PlayerId.Player1, catalog, GameRules.Default, new FirstCardSet().Normal);
                Assert.AreEqual(Fire, engine.State.FindField(chosen.CardId).Definition.Id);
            }
        }

        [Test]
        public void HiddenOpponentCards_DoNotAffectChoice_AndForecastLeavesMatchUntouched()
        {
            var catalog = Catalog();
            var a = Begin(catalog, Deck(false, Orders[0]), Deck(false, Orders[0]), GameRules.Default);
            var b = Begin(catalog, Deck(false, Orders[0]), Deck(true, Orders[0]), GameRules.Default);
            var before = a.GetView(PlayerId.Player1);
            var pending = a.State.PendingChoice;
            var first = Decide(a, PlayerId.Player1, catalog, GameRules.Default, new FirstCardSet().Normal);
            var second = Decide(b, PlayerId.Player1, catalog, GameRules.Default, new FirstCardSet().Normal);
            Assert.AreEqual(a.State.FindField(first.CardId).Definition.Id, b.State.FindField(second.CardId).Definition.Id);
            Assert.AreSame(pending, a.State.PendingChoice);
            Assert.AreEqual(before.Phase, a.State.Phase);
            CollectionAssert.AreEqual(before.Hand.Select(c => c.InstanceId), a.GetView(PlayerId.Player1).Hand.Select(c => c.InstanceId));
            Assert.IsTrue(a.State.Fields.All(f => f.IsInReserve && !f.IsOpen && !f.IsRemoved));
            Assert.IsNull(a.State.Battle);
        }

        [TestCase(PlayerId.Player1)]
        [TestCase(PlayerId.Player2)]
        public void ReplacementField_UsesRemainingCreatures(PlayerId viewer)
        {
            var catalog = Catalog();
            var rules = new GameRules(10, 1, DeckRules.Default);
            var engine = Begin(catalog, Deck(false, Orders[0]), Deck(true, Orders[0]), rules);
            foreach (var player in PlayerIds.All)
                Ok(engine, new ChooseCard(player, Field(engine, player, Plain)));
            var coin = engine.State.CoinTossWinner.Value;
            Ok(engine, new ChooseTurnOrder(coin, coin == PlayerId.Player1));
            foreach (var rarity in new[] { "common", "epic" })
            {
                var owner = rarity == "common" ? PlayerId.Player1 : PlayerId.Player2;
                var field = Field(engine, owner, Plain);
                Ok(engine, new PlaceCreature(PlayerId.Player1, Card(engine, PlayerId.Player1, "test.fire." + rarity), field));
                Ok(engine, new EndTurn(PlayerId.Player1));
                Ok(engine, new PlaceCreature(PlayerId.Player2, Card(engine, PlayerId.Player2, "test.water." + rarity), field));
                if (engine.State.Battle.CurrentActor != owner)
                    Ok(engine, new Ready(engine.State.Battle.CurrentActor));
                Ok(engine, new OpenField(owner));
                while (engine.State.Battle != null)
                    Ok(engine, new Ready(engine.State.Battle.CurrentActor));
                Ok(engine, new EndTurn(PlayerId.Player2));
            }
            Assert.AreEqual(ChoicePurpose.ReplacementField, engine.State.PendingChoice.Purpose);
            if (viewer == PlayerId.Player2)
                Ok(engine, new ChooseCard(PlayerId.Player1, Field(engine, PlayerId.Player1, Fire)));
            var chosen = Decide(engine, viewer, catalog, rules, new FirstCardSet().Normal);
            Assert.AreEqual(viewer == PlayerId.Player1 ? Fire : Water, engine.State.FindField(chosen.CardId).Definition.Id);
        }

        private static ChooseCard Decide(MatchEngine engine, PlayerId player, CardCatalog catalog, GameRules rules, AiProfile profile)
        {
            var actions = engine.GetLegalActions(player);
            var choice = new AiBrain(player, profile, catalog, rules, new XorShiftRandom(19)).Decide(engine.GetView(player), actions);
            Assert.Contains(choice, actions.ToList());
            return (ChooseCard)choice;
        }

        private static MatchEngine Begin(CardCatalog catalog, DeckList p1, DeckList p2, GameRules rules)
        {
            var engine = new MatchEngine(new MatchSetup(p1, p2), catalog, rules, new XorShiftRandom(3));
            engine.Start();
            return engine;
        }

        private static CardCatalog Catalog(bool neutral = false, bool lowerPower = false)
        {
            var cards = new List<CardDefinition>
            {
                new FieldCard(Plain, Rarity.Common, new Dictionary<Element, int>()),
                new FieldCard(Fire, Rarity.Epic, neutral ? new Dictionary<Element, int>() : new Dictionary<Element, int> { [Element.Fire] = 100, [Element.Water] = -100 }),
                new FieldCard(Water, Rarity.Legendary, neutral ? new Dictionary<Element, int>() : new Dictionary<Element, int> { [Element.Water] = 100, [Element.Fire] = -100 },
                    lowerPower ? new FieldCondition[] { new LowerPowerWinsCondition() } : new FieldCondition[0]),
            };
            var rarities = new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary };
            var names = new[] { "common", "epic", "legendary" };
            for (var i = 0; i < rarities.Length; i++)
            {
                cards.Add(new CreatureCard("test.fire." + names[i], rarities[i], Element.Fire, lowerPower ? 50 + 50 * i : 120 + 60 * i));
                cards.Add(new CreatureCard("test.water." + names[i], rarities[i], Element.Water, 120 + 60 * i));
                cards.Add(new AbilityCard("test.ability." + names[i], rarities[i], AbilityTiming.InBattle, new IAbilityEffect[] { new PowerBonusEffect(30) }));
            }
            return new CardCatalog(cards);
        }

        private static DeckList Deck(bool water, IEnumerable<string> fields) => new DeckList(fields.Concat(new[]
        {
            "test." + (water ? "water" : "fire") + ".common",
            "test." + (water ? "water" : "fire") + ".epic",
            "test." + (water ? "water" : "fire") + ".legendary",
            "test.ability.common", "test.ability.epic", "test.ability.legendary",
        }));

        private static int Field(MatchEngine engine, PlayerId owner, string id) =>
            engine.State.Fields.Single(f => f.Owner == owner && f.Definition.Id == id).Id;

        private static int Card(MatchEngine engine, PlayerId owner, string id) =>
            engine.State.Player(owner).Hand.Single(c => c.Definition.Id == id).Id;

        private static void Ok(MatchEngine engine, GameCommand command)
        {
            var result = engine.Execute(command);
            Assert.IsTrue(result.IsAccepted, command.GetType().Name + ": " + result.Reason);
        }
    }
}
