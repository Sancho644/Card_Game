using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Rules;
using NUnit.Framework;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Deck composition from docs/rules.md §3.</summary>
    public class DeckValidatorTests
    {
        private DeckValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new DeckValidator(Catalog(), DeckRules.Default);
        }

        [Test]
        public void OneCardOfEachRarityPerType_IsValid()
        {
            Assert.IsTrue(_validator.Validate(Deck()).IsValid);
            Assert.AreEqual(9, DeckRules.Default.DeckSize);
        }

        [Test]
        public void MissingCard_IsReported()
        {
            var deck = new DeckList(new[] { Plain, Forge, Temple, Fire180, Earth250, Boost50, Boost40, Gambit });

            var error = _validator.Validate(deck).Errors.Single();
            Assert.AreEqual(DeckErrorCode.MissingCard, error.Code);
            Assert.AreEqual(CardType.Creature, error.Type);
            Assert.AreEqual(Rarity.Epic, error.Rarity);
        }

        [Test]
        public void TwoCardsOfSameRarity_AreReported()
        {
            var deck = new DeckList(new[] { Plain, Forge, Temple, Fire180, Fire180, Earth250, Boost50, Boost40, Gambit });

            var codes = _validator.Validate(deck).Errors.Select(error => error.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { DeckErrorCode.ExcessCard, DeckErrorCode.MissingCard }, codes);
        }

        [Test]
        public void UnknownCard_IsReported()
        {
            var deck = new DeckList(new[] { Plain, Forge, Temple, Fire180, Water200, Earth250, Boost50, Boost40, "no.such.card" });

            var result = _validator.Validate(deck);
            Assert.IsTrue(result.Errors.Any(error => error.Code == DeckErrorCode.UnknownCard && error.CardId == "no.such.card"));
        }

        [Test]
        public void SignatureAbility_WithItsCreature_IsValid()
        {
            Assert.IsTrue(_validator.Validate(Deck(legendaryAbility: EarthSignature)).IsValid);
        }

        [Test]
        public void SignatureAbility_WithoutItsCreature_IsRejected()
        {
            var deck = new DeckList(new[]
            {
                Plain, Forge, Temple, Fire180, Water200, Dark250, Boost50, Boost40, EarthSignature,
            });

            var error = _validator.Validate(deck).Errors.Single();
            Assert.AreEqual(DeckErrorCode.SignatureCreatureMissing, error.Code);
            Assert.AreEqual(EarthSignature, error.CardId);
        }

        [Test]
        public void Engine_RejectsInvalidDeck()
        {
            var invalid = new DeckList(new[] { Plain, Forge, Temple });

            Assert.Throws<System.ArgumentException>(() => MatchDriver.Create(player2Deck: invalid));
        }
    }
}
