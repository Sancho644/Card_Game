using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>«Жертва»: the winning creature goes out of the game too; optionally only a winning guest.</summary>
    public sealed class WinnerOutOfGameCondition : FieldCondition
    {
        private readonly bool _onlyGuest;

        public WinnerOutOfGameCondition(bool onlyGuest = false)
        {
            _onlyGuest = onlyGuest;
        }

        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences =>
            {
                var winner = consequences.Winner;
                if (winner != null && (!_onlyGuest || winner == consequences.Guest))
                    consequences.SetFate(winner.Value, CreatureFate.OutOfGame);
            });
    }
}
