namespace Game.Core.Events
{
    /// <summary>The field card is turned face up; its content becomes public.</summary>
    public sealed class FieldOpened : GameEvent
    {
        public FieldOpened(int fieldId, string cardId)
        {
            FieldId = fieldId;
            CardId = cardId;
        }

        public int FieldId { get; }
        public string CardId { get; }
    }
}
