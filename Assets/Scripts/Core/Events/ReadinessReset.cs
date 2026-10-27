using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class ReadinessReset : GameEvent
    {
        public ReadinessReset(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }
}
