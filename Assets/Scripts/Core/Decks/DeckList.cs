using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Decks
{
    /// <summary>A deck as a list of card ids.</summary>
    public sealed class DeckList
    {
        public DeckList(IEnumerable<string> cardIds)
        {
            CardIds = (cardIds ?? throw new ArgumentNullException(nameof(cardIds))).ToList();
        }

        public IReadOnlyList<string> CardIds { get; }
    }
}
