using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName="Game/Config/Field Card Style",fileName="FieldCardStyle")]
    public sealed class FieldCardStyle : ScriptableObject
    {
        [SerializeField] private Sprite _frame;
        [Header("Field state")]
        [SerializeField] private Sprite _closedIcon;
        [SerializeField] private Sprite _lockedIcon;
        [SerializeField] private Rect _stateBadge=new Rect(0.716f, 0.773f, 0.2f, 0.143f);
        [SerializeField] private Color _ink=new Color(1,.94f,.8f);
        [SerializeField] private Rect _art=new Rect(0.035f, 0.443f, 0.93f, 0.53f);
        [SerializeField] private Rect _bonusRail=new Rect(0.064f, 0.278f, 0.872f, 0.081f);
        [SerializeField] private Rect _title=new Rect(0.077f, 0.367f, 0.846f, 0.07f);
        [SerializeField] private Rect _conditionTitle=new Rect(0.078f, 0.240f, 0.844f, 0.037f);
        [SerializeField] private Rect _rules=new Rect(0.077f, 0.087f, 0.846f, 0.149f);
        [SerializeField] private Rect _type=new Rect(0.116f, 0.035f, 0.31f, 0.039f);
        [SerializeField] private Rect _rarity=new Rect(0.54f, 0.031f, 0.364f, 0.047f);
        [SerializeField] private Vector2 _worldSize=new Vector2(1.4f,2);
        [SerializeField] private float _faceDepth=-.03f;
        public Sprite Frame=>_frame;
        public Sprite ClosedIcon=>_closedIcon;
        public Sprite LockedIcon=>_lockedIcon;
        public Rect StateBadge=>_stateBadge;
        public Color Ink=>_ink;
        public Rect Art=>_art;
        public Rect BonusRail=>_bonusRail;
        public Rect Title=>_title;
        public Rect ConditionTitle=>_conditionTitle;
        public Rect Rules=>_rules;
        public Rect Type=>_type;
        public Rect Rarity=>_rarity;
        public Vector2 WorldSize=>_worldSize;
        public float FaceDepth=>_faceDepth;
    }
}
