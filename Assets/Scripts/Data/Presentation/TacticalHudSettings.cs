using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName="Game/Config/Tactical HUD",fileName="TacticalHud")]
    public sealed class TacticalHudSettings : ScriptableObject
    {
        [SerializeField] private CollectionSettings _style;
        [SerializeField] private float _fieldHorizontalOffset=0f;
        [SerializeField,Min(0)] private float _creatureGap=.22f;
        [SerializeField,Min(2)] private float _opponentHandDistance=4.8f;
        [SerializeField,Min(.1f)] private float _opponentHandSpacing=.95f;
        [Tooltip("Moves the opponent's hand towards the top without widening the camera framing.")]
        [SerializeField,Min(0)] private float _opponentHandRaise=1f;
        public CollectionSettings Style=>_style;
        public float FieldHorizontalOffset=>_fieldHorizontalOffset;
        public float CreatureGap=>_creatureGap;
        public float OpponentHandDistance=>_opponentHandDistance;
        public float OpponentHandSpacing=>_opponentHandSpacing;
        public float OpponentHandRaise=>_opponentHandRaise;
    }
}
