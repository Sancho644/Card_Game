using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class MatchEnded : GameEvent
    {
        public MatchEnded(PlayerId? winner, MatchEndReason reason)
        {
            Winner = winner;
            Reason = reason;
        }

        /// <summary>Null on a draw.</summary>
        public PlayerId? Winner { get; }

        public MatchEndReason Reason { get; }
    }
}
