using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Effects;
using Game.Core.Randomness;
using Game.Core.Rules;

namespace Game.Simulation
{
    /// <summary>
    /// A valid deck built the way a player would: field cards at random, then for every creature slot the
    /// creature whose element gets the most from the deck's own fields (ties at random). With a small chance a
    /// slot gets a random creature instead. Abilities at random. Under «Закон слабейшего» a bonus hurts its
    /// creature, so such a field counts against the elements it boosts.
    /// </summary>
    public sealed class SynergyDeckBuilder
    {
        private readonly DeckSlots _slots;
        private readonly double _randomShare;

        /// <param name="randomShare">Chance that a creature slot ignores the fields and takes a random creature.</param>
        public SynergyDeckBuilder(CardCatalog catalog, DeckRules rules, double randomShare)
        {
            _slots = new DeckSlots(catalog, rules);
            _randomShare = randomShare;
        }

        public DeckList Build(IRandom random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var chosen = new List<string>();
            var fields = new List<FieldCard>();
            foreach (var slot in _slots.Slots)
            {
                var options = _slots.Candidates(slot).Where(card => _slots.FitsWith(card, chosen)).ToList();
                if (options.Count == 0)
                    throw new InvalidOperationException($"No card in the catalog fits the deck slot {slot}.");

                var pickBest = slot.Type == CardType.Creature && random.Next(1000) >= _randomShare * 1000;
                var card = pickBest ? BestFor(fields, options, random) : options[random.Next(options.Count)];
                chosen.Add(card.Id);
                if (card is FieldCard field)
                    fields.Add(field);
            }

            return new DeckList(chosen);
        }

        private static CardDefinition BestFor(IReadOnlyList<FieldCard> fields, IReadOnlyList<CardDefinition> creatures, IRandom random)
        {
            var scores = creatures.Select(card => fields.Sum(field => Bonus(field, ((CreatureCard)card).Element))).ToList();
            var best = scores.Max();
            var top = creatures.Where((card, index) => scores[index] == best).ToList();
            return top[random.Next(top.Count)];
        }

        private static int Bonus(FieldCard field, Element element)
        {
            field.Modifiers.TryGetValue(element, out var value);
            return field.Conditions.OfType<LowerPowerWinsCondition>().Any() ? -value : value;
        }
    }
}
