using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Rules;

namespace Game.Core.Decks
{
    /// <summary>
    /// The slots a deck consists of and the cards that may fill each. Used to build random decks and
    /// to reason about an opponent's unknown cards.
    /// </summary>
    public sealed class DeckSlots
    {
        private readonly CardCatalog _catalog;

        public DeckSlots(CardCatalog catalog, DeckRules rules)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            // Creatures come before abilities, so a signature ability can be checked against the chosen creature.
            Slots = new[] { CardType.Field, CardType.Creature, CardType.Ability }
                .SelectMany(type => rules.RequiredRarities(type).Select(rarity => new DeckSlot(type, rarity)))
                .ToList();
        }

        public IReadOnlyList<DeckSlot> Slots { get; }

        /// <summary>Every card of the slot's type and rarity, in catalog order.</summary>
        public IReadOnlyList<CardDefinition> Candidates(DeckSlot slot) => _catalog.All.Where(slot.Accepts).ToList();

        /// <summary>A signature ability goes into a deck only together with its creature (docs/rules.md §3).</summary>
        public bool FitsWith(CardDefinition card, IEnumerable<string> chosenCardIds) =>
            !(card is AbilityCard ability && ability.IsSignature) || chosenCardIds.Contains(ability.BoundCreatureId);
    }
}
