using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>The battle ended without a power count because a player withdrew the creature.</summary>
    public sealed class BattleRetreated : GameEvent
    {
        public BattleRetreated(int fieldId, PlayerId player)
        {
            FieldId = fieldId;
            Player = player;
        }

        public int FieldId { get; }
        public PlayerId Player { get; }
    }
}
