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
        public Sprite Icon;

        public RarityStyle(Rarity rarity, Color color)
        {
            Rarity = rarity;
            Color = color;
            Icon = null;
        }
    }
}
