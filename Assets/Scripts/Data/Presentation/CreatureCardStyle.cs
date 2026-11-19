using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Creature Card Style", fileName = "CreatureCardStyle")]
    public sealed class CreatureCardStyle : ScriptableObject
    {
        [SerializeField] private Sprite _frame;
        [Tooltip("Frame drawing area compensating for transparent padding in the source sprites; the visible border fills the card.")]
        [SerializeField] private Rect _frameArea = new Rect(-.0178f,-.0186f,1.0376f,1.0309f);
        [SerializeField] private Color _ink = new Color(1f, .94f, .80f);
        [SerializeField] private Rect _art = new Rect(0.037f, 0.264f, 0.926f, 0.719f);
        [SerializeField] private Rect _power = new Rect(0.05f, 0.779f, 0.302f, 0.16f);
        [SerializeField] private Rect _elementIcon = new Rect(0.708f, 0.815f, 0.231f, 0.154f);
        [SerializeField] private Rect _element = new Rect(0.726f, 0.774f, 0.216f, 0.044f);
        [SerializeField] private Rect _title = new Rect(00.079f, 0.145f, 0.842f, 0.111f);
        [SerializeField] private Rect _type = new Rect(0.14f, 0.056f, 0.32f, 0.049f);
        [SerializeField] private Rect _rarity = new Rect(0.537f, 0.054f, 0.356f, 0.052f);
        [SerializeField] private Rect _rules = new Rect(.08f,.29f,.84f,.13f);
        [SerializeField] private Vector2 _worldSize = new Vector2(1.4f,2f);
        [SerializeField] private float _faceDepth = -.03f;
        public Sprite Frame => _frame;
        public Rect FrameArea => _frameArea;
        public Color Ink => _ink;
        public Rect Art => _art;
        public Rect Power => _power;
        public Rect ElementIcon => _elementIcon;
        public Rect Element => _element;
        public Rect Title => _title;
        public Rect Type => _type;
        public Rect Rarity => _rarity;
        public Rect Rules => _rules;
        public Vector2 WorldSize => _worldSize;
        public float FaceDepth => _faceDepth;
    }
}
