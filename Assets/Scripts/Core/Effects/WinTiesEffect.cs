using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>On equal final power, your creature wins.</summary>
    public sealed class WinTiesEffect : IBattleEffect
    {
        public BattleStep Step => BattleStep.Power;

        public void Apply(BattleCalculation calculation, PlayerId caster) => calculation.TieWinner = caster;
    }
}
