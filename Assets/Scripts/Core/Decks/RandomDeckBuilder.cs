using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Randomness;
using Game.Core.Rules;

namespace Game.Core.Decks
{
    /// <summary>A random valid deck: every slot gets a card picked evenly among those that fit.</summary>
    public sealed class RandomDeckBuilder
    {
        private readonly DeckSlots _slots;

        public RandomDeckBuilder(CardCatalog catalog, DeckRules rules)
        {
            _slots = new DeckSlots(catalog, rules);
        }

        public DeckList Build(IRandom random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var chosen = new List<string>();
            foreach (var slot in _slots.Slots)
            {
                var options = _slots.Candidates(slot).Where(card => _slots.FitsWith(card, chosen)).ToList();
                if (options.Count == 0)
                    throw new InvalidOperationException($"No card in the catalog fits the deck slot {slot}.");
                chosen.Add(options[random.Next(options.Count)].Id);
            }

            return new DeckList(chosen);
        }
    }
}
