namespace Game.Core.State
{
    /// <summary>How the match was decided (docs/rules.md §9).</summary>
    public enum MatchEndReason
    {
        /// <summary>A player won the battles needed.</summary>
        WinsReached,

        /// <summary>Both players skipped the main action; the player with more wins won.</summary>
        MoreWins,

        /// <summary>Both skipped with equal wins; the player with more creatures not out of the game won.</summary>
        MoreCreatures,

        /// <summary>Both skipped with equal wins and equal creatures.</summary>
        Draw,
    }
}
