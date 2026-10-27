namespace Game.Core.Events
{
    public sealed class FieldRemoved : GameEvent
    {
        public FieldRemoved(int fieldId)
        {
            FieldId = fieldId;
        }

        public int FieldId { get; }
    }
}
