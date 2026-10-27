using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class BattleTurnPassed : GameEvent
    {
        public BattleTurnPassed(PlayerId actor)
        {
            Actor = actor;
        }

        public PlayerId Actor { get; }
    }
}
