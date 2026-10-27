using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class CreatureReturnedToHand : GameEvent
    {
        public CreatureReturnedToHand(PlayerId owner, int creatureId)
        {
            Owner = owner;
            CreatureId = creatureId;
        }

        public PlayerId Owner { get; }
        public int CreatureId { get; }
    }
}
