using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>The caster's creature has the given element.</summary>
    public sealed class CreatureElementRequirement : IEffectRequirement, IAbilityPlayRequirement
    {
        private readonly Element _element;

        public CreatureElementRequirement(Element element)
        {
            _element = element;
        }

        public bool AllowsPlay(PlayerId caster, BattleState battle, CardDefinition creature) =>
            creature is CreatureCard card && card.Element == _element;

        public bool IsMet(BattleCalculation calculation, PlayerId caster) =>
            calculation.CreatureOf(caster).Element == _element;
    }
}
