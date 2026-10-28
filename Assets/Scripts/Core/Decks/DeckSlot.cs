using Game.Core.Cards;

namespace Game.Core.Decks
{
    /// <summary>One place in a deck: a card of this type and rarity (docs/rules.md §3).</summary>
    public sealed class DeckSlot
    {
        public DeckSlot(CardType type, Rarity rarity)
        {
            Type = type;
            Rarity = rarity;
        }

        public CardType Type { get; }
        public Rarity Rarity { get; }

        public bool Accepts(CardDefinition card) => card.Type == Type && card.Rarity == Rarity;

        public override string ToString() => $"{Type}/{Rarity}";
    }
}
