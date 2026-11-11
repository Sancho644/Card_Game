using UnityEngine;

namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Opponent Search", fileName = "Searching")]
    public sealed class SearchingSettings : ScriptableObject
    {
        [SerializeField] private Sprite _background;
        [SerializeField] private Sprite _unknownCard;
        [SerializeField] private Color _accent = new Color(.12f, 1f, .92f);
        [SerializeField, Min(.1f)] private float _pulseSeconds = 1.6f;
        [SerializeField, Min(.1f)] private float _floatSeconds = 3f;
        [SerializeField, Min(0)] private float _floatPixels = 5f;
        [SerializeField, Range(0, 5)] private float _swayDegrees = .8f;

        public Sprite Background => _background;
        public Sprite UnknownCard => _unknownCard;
        public Color Accent => _accent;
        public float PulseSeconds => _pulseSeconds;
        public float FloatSeconds => _floatSeconds;
        public float FloatPixels => _floatPixels;
        public float SwayDegrees => _swayDegrees;
    }
}
