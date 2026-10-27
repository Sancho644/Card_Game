using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>Decides the battle winner from the final numbers. Null means a draw.</summary>
    public interface IBattleComparison
    {
        PlayerId? DecideWinner(BattleCalculation calculation);
    }

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

    /// <summary>The creature with lower final power wins.</summary>
    public sealed class LowerPowerWins : IBattleComparison
    {
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
