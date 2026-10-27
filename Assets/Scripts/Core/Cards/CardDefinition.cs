using System;

namespace Game.Core.Cards
{
    /// <summary>Immutable card data, referenced by a stable string id.</summary>
    public abstract class CardDefinition
    {
        protected CardDefinition(string id, Rarity rarity)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Card id is required.", nameof(id));

            Id = id;
            Rarity = rarity;
        }

        public string Id { get; }
        public Rarity Rarity { get; }
        public abstract CardType Type { get; }

        public override string ToString() => Id;
    }
}
