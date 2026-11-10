using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Creature Card Style", fileName = "CreatureCardStyle")]
    public sealed class CreatureCardStyle : ScriptableObject
    {
        [SerializeField] private Sprite _frame;
        [SerializeField] private Color _ink = new Color(1f, .94f, .80f);
        [SerializeField] private Rect _art = new Rect(.035f,.265f,.93f,.71f);
        [SerializeField] private Rect _power = new Rect(.055f,.77f,.29f,.18f);
        [SerializeField] private Rect _elementIcon = new Rect(.73f,.818f,.20f,.16f);
        [SerializeField] private Rect _element = new Rect(.732f,.778f,.21f,.042f);
        [SerializeField] private Rect _title = new Rect(.08f,.15f,.84f,.105f);
        [SerializeField] private Rect _type = new Rect(.155f,.06f,.30f,.04f);
        [SerializeField] private Rect _rarity = new Rect(.565f,.06f,.30f,.04f);
        [SerializeField] private Rect _rules = new Rect(.08f,.29f,.84f,.13f);
        [SerializeField] private Vector2 _worldSize = new Vector2(1.4f,2f);
        [SerializeField] private float _faceDepth = -.03f;
        public Sprite Frame => _frame;
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
