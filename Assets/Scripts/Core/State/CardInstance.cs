using Game.Core.Cards;

namespace Game.Core.State
{
    /// <summary>
    /// A physical card in a match. Both players may own the same card id,
    /// so commands and events refer to instances by their match-local id.
    /// </summary>
    public sealed class CardInstance
    {
        internal CardInstance(int id, PlayerId owner, CardDefinition definition)
        {
            Id = id;
            Owner = owner;
            Definition = definition;
        }

        public int Id { get; }
        public PlayerId Owner { get; }
        public CardDefinition Definition { get; }
        public CardType Type => Definition.Type;

        public override string ToString() => $"#{Id} {Definition.Id}";
    }
}
