using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>What the face of a card shows, built from card data.</summary>
    public sealed class CardFace
    {
        public CardFace(
            string title,
            string stats,
            string body,
            string rarity,
            Color frameColor,
            Color rarityColor,
            Sprite rarityIcon,
            Color stripColor,
            Sprite elementIcon,
            Sprite art,
            int? power,
            bool hasCondition = false)
        {
            Title = title;
            Stats = stats;
            Body = body;
            Rarity = rarity;
            FrameColor = frameColor;
            RarityColor = rarityColor;
            RarityIcon = rarityIcon;
            StripColor = stripColor;
            ElementIcon = elementIcon;
            Art = art;
            Power = power;
            HasCondition = hasCondition;
        }

        public string Title { get; }

        /// <summary>The element of a creature, bonuses and penalties of a field, the card type of an ability.</summary>
        public string Stats { get; }

        public string Body { get; }
        public string Rarity { get; }

        /// <summary>Color of the element: of the creature, of the field's largest bonus; neutral for abilities.</summary>
        public Color FrameColor { get; }

        public Color RarityColor { get; }

        /// <summary>The rarity gem, tinted with <see cref="RarityColor"/>; null shows a plain square.</summary>
        public Sprite RarityIcon { get; }

        /// <summary>Color of the strip under the art, where the element or the bonuses are.</summary>
        public Color StripColor { get; }

        /// <summary>Icon of the card's element, on the strip and in the art placeholder; null for abilities.</summary>
        public Sprite ElementIcon { get; }

        /// <summary>The card art; null shows the placeholder: element color with the element icon.</summary>
        public Sprite Art { get; }

        /// <summary>Base power of a creature; null for other cards.</summary>
        public int? Power { get; }

        /// <summary>A field card with a condition: the face shows the condition badge.</summary>
        public bool HasCondition { get; }
    }
}
