using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class TurnEnded : GameEvent
    {
        public TurnEnded(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }
}
