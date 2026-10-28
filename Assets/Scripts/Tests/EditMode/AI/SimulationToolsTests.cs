using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Randomness;
using Game.Simulation;
using NUnit.Framework;

namespace Game.AI.Tests
{
    public class SimulationToolsTests
    {
        [Test]
        public void ShareEstimate_WilsonInterval()
        {
            var half = new ShareEstimate(50, 100);
            Assert.AreEqual(0.404, half.Low, 0.001);
            Assert.AreEqual(0.596, half.High, 0.001);
            Assert.IsFalse(half.ExcludesHalf);

            var high = new ShareEstimate(650, 1000);
            Assert.AreEqual(0.620, high.Low, 0.001);
            Assert.AreEqual(0.679, high.High, 0.001);
            Assert.IsTrue(high.ExcludesHalf);

            var none = new ShareEstimate(0, 10);
            Assert.AreEqual(0, none.Low);
            Assert.Less(none.High, 0.5);
        }

        [Test]
        public void SynergyDecks_AreValid_AndCreaturesSuitTheirFields()
        {
            var cards = new FirstCardSet();
            var validator = new DeckValidator(cards.Catalog, cards.Rules.Deck);
            var synergy = new SynergyDeckBuilder(cards.Catalog, cards.Rules.Deck, randomShare: 0);
            var random = new RandomDeckBuilder(cards.Catalog, cards.Rules.Deck);
            double synergyBonus = 0, randomBonus = 0;
            for (ulong seed = 1; seed <= 200; seed++)
            {
                var deck = synergy.Build(new XorShiftRandom(seed));
                Assert.IsTrue(validator.Validate(deck).IsValid, $"Seed {seed}: invalid synergy deck.");
                synergyBonus += CreatureBonus(cards.Catalog, deck);
                randomBonus += CreatureBonus(cards.Catalog, random.Build(new XorShiftRandom(seed)));
            }

            Assert.Greater(synergyBonus, randomBonus + 200 * 50, "Synergy creatures should get clearly more from their own fields.");
        }

        /// <summary>Sum of the deck's field values for each of its creatures' elements.</summary>
        private static int CreatureBonus(CardCatalog catalog, DeckList deck)
        {
            var cardsInDeck = deck.CardIds.Select(catalog.Get).ToList();
            var fields = cardsInDeck.OfType<FieldCard>().ToList();
            return cardsInDeck.OfType<CreatureCard>()
                .Sum(creature => fields.Sum(field => field.Modifiers.TryGetValue(creature.Element, out var value) ? value : 0));
        }
    }
}
