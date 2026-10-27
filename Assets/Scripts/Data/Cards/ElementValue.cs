using System;
using Game.Core.Cards;

namespace Game.Data.Cards
{
    [Serializable]
    public struct ElementValue
    {
        public Element Element;
        public int Value;

        public ElementValue(Element element, int value)
        {
            Element = element;
            Value = value;
        }
    }
}
