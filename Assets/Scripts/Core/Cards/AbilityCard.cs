using System.Collections.Generic;
using System.Linq;
using Game.Core.Effects;

namespace Game.Core.Cards
{
    public sealed class AbilityCard : CardDefinition
    {
        public AbilityCard(
            string id,
            Rarity rarity,
            AbilityTiming timing,
            IEnumerable<IAbilityEffect> effects,
            string boundCreatureId = null)
            : base(id, rarity)
        {
            Timing = timing;
            Effects = (effects ?? Enumerable.Empty<IAbilityEffect>()).ToList();
            BoundCreatureId = boundCreatureId;
        }

        public override CardType Type => CardType.Ability;
        public AbilityTiming Timing { get; }
        public IReadOnlyList<IAbilityEffect> Effects { get; }

        /// <summary>Creature id for a signature legendary ability, otherwise null.</summary>
        public string BoundCreatureId { get; }

        public bool IsSignature => BoundCreatureId != null;

        public bool CanBePlayedAt(AbilityTiming moment) => (Timing & moment) != 0;
    }
}
