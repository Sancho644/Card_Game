using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName="Game/Config/Field Card Style",fileName="FieldCardStyle")]
    public sealed class FieldCardStyle : ScriptableObject
    {
        [SerializeField] private Sprite _frame;
        [SerializeField] private Sprite _bonusPanel;
        [Header("Field state")]
        [SerializeField] private Sprite _closedIcon;
        [SerializeField] private Sprite _lockedIcon;
        [SerializeField] private Rect _stateBadge=new Rect(.69f,.405f,.25f,.175f);
        [SerializeField] private Color _ink=new Color(1,.94f,.8f);
        [SerializeField] private Rect _art=new Rect(.035f,.40f,.93f,.57f);
        [SerializeField] private Rect _bonusRail=new Rect(.045f,.43f,.19f,.48f);
        [SerializeField] private Rect _title=new Rect(.10f,.325f,.80f,.065f);
        [SerializeField] private Rect _conditionTitle=new Rect(.16f,.245f,.68f,.05f);
        [SerializeField] private Rect _rules=new Rect(.08f,.115f,.84f,.125f);
        [SerializeField] private Rect _type=new Rect(.12f,.04f,.33f,.035f);
        [SerializeField] private Rect _rarity=new Rect(.56f,.04f,.33f,.035f);
        [SerializeField] private Rect _rarityIcon=new Rect(.90f,.935f,.07f,.05f);
        [SerializeField] private Vector2 _worldSize=new Vector2(1.4f,2);
        [SerializeField] private float _faceDepth=-.03f;
        public Sprite Frame=>_frame;
        public Sprite BonusPanel=>_bonusPanel;
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
        public Rect RarityIcon=>_rarityIcon;
        public Vector2 WorldSize=>_worldSize;
        public float FaceDepth=>_faceDepth;
    }
}
