using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class TurnStarted : GameEvent
    {
        public TurnStarted(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }
}
