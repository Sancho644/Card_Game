using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName="Game/Config/Card Highlight",fileName="CardHighlight")]
    public sealed class CardHighlightSettings : ScriptableObject
    {
        [SerializeField] private Material _material;
        [SerializeField,Min(.005f)] private float _width=.055f;
        [SerializeField,Min(0)] private float _padding=.015f;
        [SerializeField,Min(.1f)] private float _pulseSeconds=1.6f;
        [SerializeField,Min(.1f)] private float _travelSeconds=2.4f;
        public Material Material=>_material;
        public float Width=>_width;
        public float Padding=>_padding;
        public float PulseSeconds=>_pulseSeconds;
        public float TravelSeconds=>_travelSeconds;
    }
}
