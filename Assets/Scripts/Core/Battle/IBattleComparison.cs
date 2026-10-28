using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>Decides the battle winner from the final numbers. Null means a draw, which only a card text allows.</summary>
    public interface IBattleComparison
    {
        ComparisonRule Rule { get; }

        PlayerId? DecideWinner(BattleCalculation calculation);

        /// <summary>The winner if the final powers were equal; null if that is a draw by this rule.</summary>
        PlayerId? WinnerOnEqualPower(BattleCalculation calculation);
    }
}
