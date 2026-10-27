using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class CoinTossed : GameEvent
    {
        public CoinTossed(PlayerId winner)
        {
            Winner = winner;
        }

        public PlayerId Winner { get; }
    }
}
