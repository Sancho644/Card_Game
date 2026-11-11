using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName="Game/Config/Tactical HUD",fileName="TacticalHud")]
    public sealed class TacticalHudSettings : ScriptableObject
    {
        [SerializeField] private CollectionSettings _style;
        [SerializeField,Range(.22f,.34f)] private float _sidebarShare=.25f;
        [SerializeField,Min(1)] private float _battleFieldScale=1.7f;
        [SerializeField,Min(1)] private float _battleCreatureScale=1.8f;
        [SerializeField,Min(2)] private float _battleSpacing=3.1f;
        [SerializeField,Min(2)] private float _opponentHandDistance=4.8f;
        public CollectionSettings Style=>_style;
        public float SidebarShare=>_sidebarShare;
        public float BattleFieldScale=>_battleFieldScale;
        public float BattleCreatureScale=>_battleCreatureScale;
        public float BattleSpacing=>_battleSpacing;
        public float OpponentHandDistance=>_opponentHandDistance;
    }
}
