using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>If your creature loses this battle, it returns to hand instead of going out of the game.</summary>
    public sealed class ReturnToHandIfLostEffect : IBattleEffect
    {
        public BattleStep Step => BattleStep.Power;

        public void Apply(BattleCalculation calculation, PlayerId caster) =>
            calculation.ModifyConsequences(consequences =>
            {
                if (consequences.Loser == caster && consequences.FateOf(caster) == CreatureFate.OutOfGame)
                    consequences.SetFate(caster, CreatureFate.ReturnsToHand);
            });
    }
}
