using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class CreaturePlaced : GameEvent
    {
        public CreaturePlaced(PlayerId player, CardRef creature, int fieldId)
        {
            Player = player;
            Creature = creature;
            FieldId = fieldId;
        }

        public PlayerId Player { get; }
        public CardRef Creature { get; }
        public int FieldId { get; }
    }
}
