using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>The creature with lower final power wins.</summary>
    public sealed class LowerPowerWins : IBattleComparison
    {
        public ComparisonRule Rule => ComparisonRule.LowerPowerWins;

        public PlayerId? DecideWinner(BattleCalculation calculation)
        {
            var host = calculation.PowerOf(calculation.Host);
            var guest = calculation.PowerOf(calculation.Guest);
            if (host == guest)
                return null;
            return host < guest ? calculation.Host : calculation.Guest;
        }
    }
}
