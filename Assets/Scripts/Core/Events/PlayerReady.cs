using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class PlayerReady : GameEvent
    {
        public PlayerReady(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }
}
