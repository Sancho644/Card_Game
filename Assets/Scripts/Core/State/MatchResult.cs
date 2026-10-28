namespace Game.Core.State
{
    public sealed class MatchResult
    {
        internal MatchResult(PlayerId? winner, MatchEndReason reason)
        {
            Winner = winner;
            Reason = reason;
        }

        public PlayerId? Winner { get; }
        public MatchEndReason Reason { get; }
        public bool IsDraw => Winner == null;
    }
}
