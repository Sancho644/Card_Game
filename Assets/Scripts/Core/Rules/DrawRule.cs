namespace Game.Core.Rules
{
    /// <summary>
    /// What a draw leads to (docs/rules.md §6). Equal power is not a draw: the creature placed earlier wins;
    /// a draw comes only from a card text («Узкая грань»).
    /// </summary>
    public enum DrawRule
    {
        /// <summary>No win; both creatures return to hand; an open field is removed, a closed one stays.</summary>
        BothReturnToHand,
    }
}
