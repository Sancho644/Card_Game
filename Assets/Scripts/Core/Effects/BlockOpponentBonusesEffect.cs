using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>The open field's bonuses do not act on the opponent's creature; penalties still do.</summary>
    public sealed class BlockOpponentBonusesEffect : IBattleEffect
    {
        public BattleStep Step => BattleStep.FieldValues;

        public void Apply(BattleCalculation calculation, PlayerId caster) =>
            calculation.MapFieldValues(caster.Opponent(), value => value > 0 ? 0 : value);
    }
}
