using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>«Жертва»: the winning creature goes out of the game too.</summary>
    public sealed class WinnerOutOfGameCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences =>
            {
                var winner = consequences.Winner;
                if (winner != null)
                    consequences.SetFate(winner.Value, CreatureFate.OutOfGame);
            });
    }
}
