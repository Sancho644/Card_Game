using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>«Милосердие»: the losing creature returns to hand instead of going out of the game.</summary>
    public sealed class LoserReturnsToHandCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences =>
            {
                var loser = consequences.Loser;
                if (loser != null && consequences.FateOf(loser.Value) == CreatureFate.OutOfGame)
                    consequences.SetFate(loser.Value, CreatureFate.ReturnsToHand);
            });
    }
}
