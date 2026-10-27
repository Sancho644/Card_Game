using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Data.Effects;
using UnityEngine;

namespace Game.Data.Cards
{
    [CreateAssetMenu(menuName = "Game/Cards/Ability", fileName = "Ability")]
    public sealed class AbilityDef : CardDef
    {
        [Tooltip("When the card text allows playing it.")]
        [SerializeField] private AbilityTiming _timing = AbilityTiming.InBattle;

        [SerializeReference] private List<AbilityEffectData> _effects = new List<AbilityEffectData>();

        [Tooltip("For a signature legendary ability: the creature it belongs to. Empty otherwise.")]
        [SerializeField] private CreatureDef _boundCreature;

        public override CardType Type => CardType.Ability;
        public AbilityTiming Timing => _timing;
        public IReadOnlyList<AbilityEffectData> Effects => _effects;
        public CreatureDef BoundCreature => _boundCreature;

        public override CardDefinition ToCore() =>
            new AbilityCard(
                Id,
                Rarity,
                _timing,
                _effects.Where(effect => effect != null).Select(effect => effect.Create()),
                _boundCreature == null ? null : _boundCreature.Id);

        public override IEnumerable<string> Validate()
        {
            foreach (var problem in base.Validate())
                yield return problem;
            if (_timing == AbilityTiming.None)
                yield return "no timing selected";
            if (_effects.Any(effect => effect == null))
                yield return "an effect has no type selected";
            if (_boundCreature != null && Rarity != Rarity.Legendary)
                yield return "a signature ability must be legendary";
        }
    }
}
