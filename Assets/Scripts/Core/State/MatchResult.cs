namespace Game.Core.State
{
    public sealed class MatchResult
    {
        internal MatchResult(PlayerId? winner)
        {
            Winner = winner;
        }

        public PlayerId? Winner { get; }
        public bool IsDraw => Winner == null;
    }
}
