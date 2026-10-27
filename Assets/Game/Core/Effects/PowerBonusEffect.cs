using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>"+N to your creature's power", optionally only when every requirement is met.</summary>
    public sealed class PowerBonusEffect : IAbilityEffect
    {
        private readonly int _amount;
        private readonly IReadOnlyList<IEffectRequirement> _requirements;

        public PowerBonusEffect(int amount, params IEffectRequirement[] requirements)
        {
            _amount = amount;
            _requirements = requirements ?? new IEffectRequirement[0];
        }

        public void Apply(BattleCalculation calculation, PlayerId caster)
        {
            if (_requirements.All(requirement => requirement.IsMet(calculation, caster)))
                calculation.AddPower(caster, _amount);
        }
    }
}
