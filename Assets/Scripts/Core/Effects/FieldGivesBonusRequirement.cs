using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>The field is open and gives the caster's creature a bonus.</summary>
    public sealed class FieldGivesBonusRequirement : IEffectRequirement
    {
        public bool IsMet(BattleCalculation calculation, PlayerId caster) =>
            calculation.FieldOpen && calculation.FieldModifierFor(caster) > 0;
    }
}
