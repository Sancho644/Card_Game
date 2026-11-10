using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Collection", fileName = "Collection")]
    public sealed class CollectionSettings : ScriptableObject
    {
        [SerializeField] private VisualSettings _cardVisuals;
        [Header("Library artwork")]
        [SerializeField] private Sprite _backgroundArt;
        [SerializeField] private Sprite _panelArt;
        [SerializeField] private Sprite _buttonArt;
        [SerializeField] private Rect _panelCrop = new Rect(.008f,.012f,.984f,.976f);
        [SerializeField] private Rect _buttonCrop = new Rect(.002f,.24f,.996f,.58f);
        [SerializeField, Min(.1f)] private float _sliceDensity = 3;
        [SerializeField, Min(.1f)] private float _buttonSliceDensity = 8;
        public Sprite BackgroundArt => _backgroundArt;
        public Sprite PanelArt => _panelArt;
        public Sprite ButtonArt => _buttonArt;
        public Rect PanelCrop => _panelCrop;
        public Rect ButtonCrop => _buttonCrop;
        public float SliceDensity => _sliceDensity;
        public float ButtonSliceDensity => _buttonSliceDensity;
        public VisualSettings CardVisuals => _cardVisuals;
        [SerializeField] private Color _background = new Color(0.055f, 0.075f, 0.11f, 1f);
        [SerializeField] private Color _panel = new Color(0.09f, 0.12f, 0.17f, 1f);
        [SerializeField] private Color _button = new Color(0.15f, 0.21f, 0.28f, 1f);
        [SerializeField] private Color _selected = new Color(0.14f, 0.38f, 0.43f, 1f);
        [SerializeField] private Color _text = new Color(0.94f, 0.96f, 0.98f, 1f);
        [SerializeField] private Color _muted = new Color(0.65f, 0.75f, 0.83f, 1f);
        [SerializeField, Min(12)] private float _fontSize = 24;
        [SerializeField, Min(16)] private float _titleSize = 34;
        [SerializeField, Min(10)] private float _smallSize = 19;
        public Color Background => _background;
        public Color Panel => _panel;
        public Color Button => _button;
        public Color Selected => _selected;
        public Color Text => _text;
        public Color Muted => _muted;
        public float FontSize => _fontSize;
        public float TitleSize => _titleSize;
        public float SmallSize => _smallSize;
    }
}
