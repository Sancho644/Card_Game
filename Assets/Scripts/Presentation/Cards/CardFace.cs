using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>What the face of a card shows, built from card data.</summary>
    public sealed class CardFace
    {
        public CardFace(string title, string stats, string body, string rarity, Color frameColor, Color rarityColor)
        {
            Title = title;
            Stats = stats;
            Body = body;
            Rarity = rarity;
            FrameColor = frameColor;
            RarityColor = rarityColor;
        }

        public string Title { get; }

        /// <summary>Element and power for a creature, bonuses and penalties for a field.</summary>
        public string Stats { get; }

        public string Body { get; }
        public string Rarity { get; }
        public Color FrameColor { get; }
        public Color RarityColor { get; }
    }
}
