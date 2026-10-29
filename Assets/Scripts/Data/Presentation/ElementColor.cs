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

        [Tooltip("Shown on the strip under the art and in the art placeholder, so the element reads not only by color")]
        public Sprite Icon;

        public ElementColor(Element element, Color color)
        {
            Element = element;
            Color = color;
            Icon = null;
        }
    }
}
