using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class MainActionSkipped : GameEvent
    {
        public MainActionSkipped(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }
}
