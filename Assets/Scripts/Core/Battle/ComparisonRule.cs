namespace Game.Core.Battle
{
    /// <summary>Which rule decided the winner, reported to the presentation.</summary>
    public enum ComparisonRule
    {
        HigherPowerWins,
        LowerPowerWins,

        /// <summary>A small difference is a draw («Узкая грань»).</summary>
        NarrowMarginDraw,
    }
}
