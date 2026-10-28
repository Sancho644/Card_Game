using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Randomness;
using Game.Core.Rules;
using NUnit.Framework;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    public class RandomDeckTests
    {
        [Test]
        public void RandomDecks_AreAlwaysValid()
        {
            var catalog = Catalog();
            var builder = new RandomDeckBuilder(catalog, DeckRules.Default);
            var validator = new DeckValidator(catalog, DeckRules.Default);
            var random = new XorShiftRandom(11);

            for (var i = 0; i < 500; i++)
            {
                var deck = builder.Build(random);
                var result = validator.Validate(deck);
                Assert.IsTrue(result.IsValid, string.Join(", ", deck.CardIds) + ": " + string.Join(", ", result.Errors));
            }
        }

        [Test]
        public void SameSeed_SameDeck()
        {
            var builder = new RandomDeckBuilder(Catalog(), DeckRules.Default);

            CollectionAssert.AreEqual(builder.Build(new XorShiftRandom(5)).CardIds, builder.Build(new XorShiftRandom(5)).CardIds);
        }

        [Test]
        public void SignatureAbility_FitsOnlyWithItsCreature()
        {
            var catalog = Catalog();
            var slots = new DeckSlots(catalog, DeckRules.Default);
            var signature = catalog.Get(EarthSignature);

            Assert.IsTrue(slots.FitsWith(signature, new[] { Earth250 }));
            Assert.IsFalse(slots.FitsWith(signature, new[] { Dark250 }));
            Assert.IsTrue(slots.FitsWith(catalog.Get(Gambit), new string[0]));
        }

        [Test]
        public void Slots_FollowDeckRules_CreaturesBeforeAbilities()
        {
            var slots = new DeckSlots(Catalog(), DeckRules.Default).Slots;

            Assert.AreEqual(DeckRules.Default.DeckSize, slots.Count);
            Assert.Less(
                slots.ToList().FindLastIndex(slot => slot.Type == CardType.Creature),
                slots.ToList().FindIndex(slot => slot.Type == CardType.Ability));
        }
    }
}
