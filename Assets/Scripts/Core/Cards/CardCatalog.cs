using System;
using System.Collections.Generic;

namespace Game.Core.Cards
{
    /// <summary>All cards known to a match, looked up by stable id.</summary>
    public sealed class CardCatalog
    {
        private readonly Dictionary<string, CardDefinition> _cards = new Dictionary<string, CardDefinition>();

        public CardCatalog(IEnumerable<CardDefinition> cards)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            foreach (var card in cards)
            {
                if (_cards.ContainsKey(card.Id))
                    throw new ArgumentException($"Duplicate card id '{card.Id}'.", nameof(cards));
                _cards.Add(card.Id, card);
            }
        }

        public bool TryGet(string id, out CardDefinition card) => _cards.TryGetValue(id, out card);

        public CardDefinition Get(string id)
        {
            if (!_cards.TryGetValue(id, out var card))
                throw new KeyNotFoundException($"Unknown card id '{id}'.");
            return card;
        }
    }
}
