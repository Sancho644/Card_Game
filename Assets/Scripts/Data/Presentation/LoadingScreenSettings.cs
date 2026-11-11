using UnityEngine;

namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Loading Screen", fileName = "LoadingScreen")]
    public sealed class LoadingScreenSettings : ScriptableObject
    {
        [SerializeField] private bool _showOnTransitions;
        [SerializeField] private Sprite _background;
        [SerializeField] private Sprite _seal;
        [SerializeField] private CollectionSettings _style;
        [SerializeField] private Color _accent = new Color(.15f, 1f, .88f);
        [SerializeField, Min(0)] private float _fadeOutSeconds = .2f;
        [SerializeField, Min(.01f)] private float _cometTurnsPerSecond = .12f;
        [SerializeField, Range(0, .03f)] private float _sealPulse = .006f;
        [SerializeField, Min(.1f)] private float _pulseSeconds = 3f;
        [SerializeField, Min(1)] private float _tipSeconds = 6f;
        [SerializeField] private string[] _tips = { "loading.tip.zoom", "loading.tip.field", "loading.tip.ready" };

        public bool ShowOnTransitions => _showOnTransitions;
        public Sprite Background => _background;
        public Sprite Seal => _seal;
        public CollectionSettings Style => _style;
        public Color Accent => _accent;
        public float FadeOutSeconds => _fadeOutSeconds;
        public float CometTurnsPerSecond => _cometTurnsPerSecond;
        public float SealPulse => _sealPulse;
        public float PulseSeconds => _pulseSeconds;
        public float TipSeconds => _tipSeconds;
        public System.Collections.Generic.IReadOnlyList<string> Tips => _tips;
    }
}
