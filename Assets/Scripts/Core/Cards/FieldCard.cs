using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Effects;

namespace Game.Core.Cards
{
    public sealed class FieldCard : CardDefinition
    {
        public FieldCard(
            string id,
            Rarity rarity,
            IReadOnlyDictionary<Element, int> modifiers,
            IEnumerable<FieldCondition> conditions = null)
            : base(id, rarity)
        {
            if (modifiers == null)
                throw new ArgumentNullException(nameof(modifiers));

            Modifiers = modifiers.ToDictionary(pair => pair.Key, pair => pair.Value);
            Conditions = (conditions ?? Enumerable.Empty<FieldCondition>()).ToList();
        }

        public override CardType Type => CardType.Field;

        /// <summary>Bonuses (positive) and penalties (negative) by element.</summary>
        public IReadOnlyDictionary<Element, int> Modifiers { get; }

        public IReadOnlyList<FieldCondition> Conditions { get; }
    }
}
