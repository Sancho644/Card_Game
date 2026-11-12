using UnityEngine;
namespace Game.Presentation.Cards
{
    public sealed class FieldBonusFace
    {
        public FieldBonusFace(string element, int value, Sprite icon) { Element=element; Value=value; Icon=icon; }
        public string Element { get; }
        public int Value { get; }
        public Sprite Icon { get; }
    }
}
