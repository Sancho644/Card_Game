using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;

namespace Game.Core.Rules
{
    /// <summary>Deck composition: the exact rarities required for each card type.</summary>
    public sealed class DeckRules
    {
        private readonly Dictionary<CardType, IReadOnlyList<Rarity>> _required;

        public DeckRules(IReadOnlyDictionary<CardType, IReadOnlyList<Rarity>> requiredRarities)
        {
            if (requiredRarities == null)
                throw new ArgumentNullException(nameof(requiredRarities));

            _required = requiredRarities.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<Rarity>)pair.Value.ToList());
        }

        public int DeckSize => _required.Values.Sum(rarities => rarities.Count);

        /// <summary>One common, one epic and one legendary card of each type.</summary>
        public static DeckRules Default
        {
            get
            {
                var oneOfEach = new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary };
                return new DeckRules(new Dictionary<CardType, IReadOnlyList<Rarity>>
                {
                    [CardType.Field] = oneOfEach,
                    [CardType.Creature] = oneOfEach,
                    [CardType.Ability] = oneOfEach,
                });
            }
        }

        public IReadOnlyList<Rarity> RequiredRarities(CardType type) =>
            _required.TryGetValue(type, out var rarities) ? rarities : Array.Empty<Rarity>();
    }
}
