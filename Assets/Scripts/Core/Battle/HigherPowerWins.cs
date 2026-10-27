using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>Default rule: the creature with greater final power wins.</summary>
    public sealed class HigherPowerWins : IBattleComparison
    {
        public PlayerId? DecideWinner(BattleCalculation calculation)
        {
            var host = calculation.PowerOf(calculation.Host);
            var guest = calculation.PowerOf(calculation.Guest);
            if (host == guest)
                return null;
            return host > guest ? calculation.Host : calculation.Guest;
        }
    }
}
