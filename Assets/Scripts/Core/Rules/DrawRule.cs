namespace Game.Core.Rules
{
    /// <summary>How a draw is settled. Open question 1 in docs/rules.md; alternatives go here.</summary>
    public enum DrawRule
    {
        /// <summary>No win; both creatures return to hand; an open field is removed, a closed one stays.</summary>
        BothReturnToHand,
    }
}
