using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>"−N to the opponent's creature's power".</summary>
    public sealed class OpponentPowerEffect : IBattleEffect
    {
        private readonly int _amount;

        public OpponentPowerEffect(int amount)
        {
            _amount = amount;
        }

        public BattleStep Step => BattleStep.Power;

        public void Apply(BattleCalculation calculation, PlayerId caster) =>
            calculation.AddPower(caster.Opponent(), _amount);
    }
}
