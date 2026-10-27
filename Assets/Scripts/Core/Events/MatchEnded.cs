using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class MatchEnded : GameEvent
    {
        public MatchEnded(PlayerId? winner)
        {
            Winner = winner;
        }

        /// <summary>Null on a draw.</summary>
        public PlayerId? Winner { get; }
    }
}
