using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>«Рассвет»: the open field's penalties do not act on your creature; its condition still does.</summary>
    public sealed class IgnoreFieldPenaltiesEffect : IBattleEffect
    {
        public BattleStep Step => BattleStep.FieldValues;

        public void Apply(BattleCalculation calculation, PlayerId caster) =>
            calculation.IgnoreFieldPenalties(caster);
    }
}
