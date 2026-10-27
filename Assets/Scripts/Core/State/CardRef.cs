namespace Game.Core.State
{
    /// <summary>Public reference to a card instance: what events and views expose.</summary>
    public sealed class CardRef
    {
        public CardRef(int instanceId, string cardId)
        {
            InstanceId = instanceId;
            CardId = cardId;
        }

        public int InstanceId { get; }
        public string CardId { get; }

        public override string ToString() => $"#{InstanceId} {CardId}";

        internal static CardRef From(CardInstance card) =>
            card == null ? null : new CardRef(card.Id, card.Definition.Id);
    }
}
