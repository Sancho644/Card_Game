using System;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Presentation
{
    [Serializable]
    public struct ElementColor
    {
        public Element Element;
        public Color Color;

        public ElementColor(Element element, Color color)
        {
            Element = element;
            Color = color;
        }
    }
}
