using System.Collections.Generic;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Presentation
{
    /// <summary>Colors and icons of elements, rarities, card backs and highlights.</summary>
    [CreateAssetMenu(menuName = "Game/Config/Visual Settings", fileName = "VisualSettings")]
    public sealed class VisualSettings : ScriptableObject
    {
        [Header("Card frames")]
        [SerializeField] private List<ElementColor> _elements = new List<ElementColor>
        {
            new ElementColor(Element.Light, new Color(0.95f, 0.85f, 0.45f)),
            new ElementColor(Element.Dark, new Color(0.45f, 0.30f, 0.60f)),
            new ElementColor(Element.Earth, new Color(0.55f, 0.42f, 0.25f)),
            new ElementColor(Element.Water, new Color(0.25f, 0.55f, 0.90f)),
            new ElementColor(Element.Fire, new Color(0.90f, 0.35f, 0.20f)),
            new ElementColor(Element.Wind, new Color(0.45f, 0.80f, 0.60f)),
        };

        [SerializeField] private Color _abilityFrame = new Color(0.70f, 0.72f, 0.78f);
        [SerializeField] private Color _neutralFrame = new Color(0.60f, 0.60f, 0.60f);
        [SerializeField] private Color _cardBody = new Color(0.12f, 0.12f, 0.15f);
        [SerializeField] private Color _cardBack = new Color(0.20f, 0.18f, 0.35f);
        [SerializeField] private Color _cardText = Color.white;

        [Header("Rarities")]
        [SerializeField] private List<RarityStyle> _rarities = new List<RarityStyle>
        {
            new RarityStyle(Rarity.Common, new Color(0.75f, 0.75f, 0.75f)),
            new RarityStyle(Rarity.Epic, new Color(0.65f, 0.35f, 0.90f)),
            new RarityStyle(Rarity.Legendary, new Color(1.00f, 0.65f, 0.10f)),
        };

        [Header("Highlights")]
        [SerializeField] private Color _selectable = new Color(0.30f, 0.90f, 0.40f);
        [SerializeField] private Color _selected = new Color(1.00f, 0.90f, 0.20f);
        [SerializeField] private Color _target = new Color(0.25f, 0.85f, 1.00f);
        [SerializeField] private Color _battle = new Color(1.00f, 0.45f, 0.15f);

        public Color AbilityFrame => _abilityFrame;
        public Color NeutralFrame => _neutralFrame;
        public Color CardBody => _cardBody;
        public Color CardBack => _cardBack;
        public Color CardText => _cardText;
        public Color Selectable => _selectable;
        public Color Selected => _selected;
        public Color Target => _target;
        public Color Battle => _battle;

        public Color FrameColorOf(Element element)
        {
            var index = _elements.FindIndex(entry => entry.Element == element);
            return index < 0 ? _neutralFrame : _elements[index].Color;
        }

        public RarityStyle StyleOf(Rarity rarity) => _rarities.Find(entry => entry.Rarity == rarity);
    }
}
