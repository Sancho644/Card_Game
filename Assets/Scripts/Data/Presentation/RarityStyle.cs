using System;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Presentation
{
    [Serializable]
    public struct RarityStyle
    {
        public Rarity Rarity;
        public Color Color;

        [Tooltip("The rarity gem in the corner of the card, tinted with the color")]
        public Sprite Icon;

        public RarityStyle(Rarity rarity, Color color)
        {
            Rarity = rarity;
            Color = color;
            Icon = null;
        }
    }
}
