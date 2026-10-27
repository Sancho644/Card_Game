namespace Game.Core.Events
{
    public sealed class FieldLocked : GameEvent
    {
        public FieldLocked(int fieldId)
        {
            FieldId = fieldId;
        }

        public int FieldId { get; }
    }
}
