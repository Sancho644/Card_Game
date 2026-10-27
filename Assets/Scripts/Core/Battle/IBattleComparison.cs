using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>Decides the battle winner from the final numbers. Null means a draw.</summary>
    public interface IBattleComparison
    {
        ComparisonRule Rule { get; }

        PlayerId? DecideWinner(BattleCalculation calculation);
    }
}
