using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>A challenge: the creature moved to a field where an opponent's creature stands.</summary>
    public sealed class CreatureMoved : GameEvent
    {
        public CreatureMoved(PlayerId player, int creatureId, int fromFieldId, int toFieldId)
        {
            Player = player;
            CreatureId = creatureId;
            FromFieldId = fromFieldId;
            ToFieldId = toFieldId;
        }

        public PlayerId Player { get; }
        public int CreatureId { get; }
        public int FromFieldId { get; }
        public int ToFieldId { get; }
    }
}
