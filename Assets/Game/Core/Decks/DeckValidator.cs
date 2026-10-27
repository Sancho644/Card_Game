using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Rules;

namespace Game.Core.Decks
{
    public enum DeckErrorCode
    {
        UnknownCard,
        MissingCard,
        ExcessCard,
        SignatureCreatureMissing,
    }

    public sealed class DeckError
    {
        internal DeckError(DeckErrorCode code, string cardId = null, CardType? type = null, Rarity? rarity = null)
        {
            Code = code;
            CardId = cardId;
            Type = type;
            Rarity = rarity;
        }

        public DeckErrorCode Code { get; }
        public string CardId { get; }
        public CardType? Type { get; }
        public Rarity? Rarity { get; }

        public override string ToString() =>
            Type == null ? $"{Code} {CardId}" : $"{Code} {Type} {Rarity}";
    }

    public sealed class DeckValidationResult
    {
        internal DeckValidationResult(IReadOnlyList<DeckError> errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<DeckError> Errors { get; }
        public bool IsValid => Errors.Count == 0;
    }

    public sealed class DeckValidator
    {
        private readonly CardCatalog _catalog;
        private readonly DeckRules _rules;

        public DeckValidator(CardCatalog catalog, DeckRules rules)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public DeckValidationResult Validate(DeckList deck)
        {
            var errors = new List<DeckError>();
            var cards = new List<CardDefinition>();

            foreach (var id in deck.CardIds)
            {
                if (_catalog.TryGet(id, out var card))
                    cards.Add(card);
                else
                    errors.Add(new DeckError(DeckErrorCode.UnknownCard, cardId: id));
            }

            CheckComposition(cards, errors);
            CheckSignatureAbilities(cards, errors);
            return new DeckValidationResult(errors);
        }

        private void CheckComposition(List<CardDefinition> cards, List<DeckError> errors)
        {
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
            {
                var required = _rules.RequiredRarities(type);
                foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
                {
                    var expected = required.Count(r => r == rarity);
                    var actual = cards.Count(card => card.Type == type && card.Rarity == rarity);
                    if (actual < expected)
                        errors.Add(new DeckError(DeckErrorCode.MissingCard, type: type, rarity: rarity));
                    else if (actual > expected)
                        errors.Add(new DeckError(DeckErrorCode.ExcessCard, type: type, rarity: rarity));
                }
            }
        }

        /// <summary>A signature ability is useless without its creature, so the deck must contain both.</summary>
        private static void CheckSignatureAbilities(List<CardDefinition> cards, List<DeckError> errors)
        {
            foreach (var ability in cards.OfType<AbilityCard>().Where(ability => ability.IsSignature))
            {
                if (!cards.Any(card => card.Type == CardType.Creature && card.Id == ability.BoundCreatureId))
                    errors.Add(new DeckError(DeckErrorCode.SignatureCreatureMissing, cardId: ability.Id));
            }
        }
    }
}
