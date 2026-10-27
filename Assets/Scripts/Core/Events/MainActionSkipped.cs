using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class MainActionSkipped : GameEvent
    {
        public MainActionSkipped(PlayerId player, SkipReason reason)
        {
            Player = player;
            Reason = reason;
        }

        public PlayerId Player { get; }
        public SkipReason Reason { get; }
    }
}
