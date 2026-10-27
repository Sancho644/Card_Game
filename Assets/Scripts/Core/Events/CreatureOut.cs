using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class CreatureOut : GameEvent
    {
        public CreatureOut(PlayerId owner, int creatureId)
        {
            Owner = owner;
            CreatureId = creatureId;
        }

        public PlayerId Owner { get; }
        public int CreatureId { get; }
    }
}
