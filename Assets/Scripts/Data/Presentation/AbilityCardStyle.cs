using UnityEngine;

namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Ability Card Style", fileName = "AbilityCardStyle")]
    public sealed class AbilityCardStyle : ScriptableObject
    {
        [SerializeField] private Sprite _frame;
        [SerializeField] private Sprite _bonusPanel;
        [SerializeField] private Color _ink = new Color(1f, .94f, .8f);
        [SerializeField] private Color _placeholder = new Color(.025f, .08f, .10f);
        [SerializeField] private Rect _art = new Rect(.025f, .48f, .95f, .50f);
        [SerializeField] private Rect _title = new Rect(.10f, .394f, .80f, .055f);
        [SerializeField] private Rect _bonus = new Rect(.02f, .477f, .36f, .063f);
        [SerializeField] private Rect _timing = new Rect(.12f, .307f, .76f, .040f);
        [SerializeField] private Rect _rules = new Rect(.07f, .105f, .86f, .19f);
        [SerializeField] private Rect _type = new Rect(.075f, .03f, .32f, .04f);
        [SerializeField] private Rect _rarity = new Rect(.61f, .03f, .31f, .04f);
        [SerializeField] private Rect _rarityIcon = new Rect(.90f, .92f, .085f, .065f);
        [SerializeField] private Vector2 _worldSize = new Vector2(1.4f, 2f);
        [SerializeField] private float _faceDepth = -.03f;

        public Sprite Frame => _frame;
        public Sprite BonusPanel => _bonusPanel;
        public Color Ink => _ink;
        public Color Placeholder => _placeholder;
        public Rect Art => _art;
        public Rect Title => _title;
        public Rect Bonus => _bonus;
        public Rect Timing => _timing;
        public Rect Rules => _rules;
        public Rect Type => _type;
        public Rect Rarity => _rarity;
        public Rect RarityIcon => _rarityIcon;
        public Vector2 WorldSize => _worldSize;
        public float FaceDepth => _faceDepth;
    }
}
