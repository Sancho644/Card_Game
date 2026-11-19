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
        [SerializeField] private Rect _art = new Rect(0.03f, 0.478f, 0.94f, 0.493f);
        [SerializeField] private Rect _title = new Rect(0.075f, 0.398f, 0.85f, 0.073f);
        [SerializeField] private Rect _bonus = new Rect(0.057f, 0.34f, 0.337f, 0.056f);
        [SerializeField] private Rect _timing = new Rect(0.404f, 0.342f, 0.53f, 0.052f);
        [SerializeField] private Rect _rules = new Rect(0.071f, 0.085f, 0.858f, 0.248f);
        [SerializeField] private Rect _type = new Rect(0.075f, 0.032f, 0.35f, 0.042f);
        [SerializeField] private Rect _rarity = new Rect(0.54f, 0.026f, 0.389f, 0.053f);
        [SerializeField] private Rect _rarityIcon = new Rect(.9f, .92f, .085f, .065f);
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
