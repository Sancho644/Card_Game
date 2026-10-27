using UnityEngine;

namespace Game.Data.Presentation
{
    /// <summary>Timings and layout numbers of the table scene.</summary>
    [CreateAssetMenu(menuName = "Game/Config/Table Presentation", fileName = "TablePresentation")]
    public sealed class TablePresentationSettings : ScriptableObject
    {
        [Header("Animation, seconds")]
        [SerializeField, Min(0)] private float _moveDuration = 0.35f;
        [SerializeField, Min(0)] private float _abilityShowSeconds = 0.8f;
        [SerializeField, Min(1)] private float _fastForwardScale = 4f;

        [Header("Input")]
        [SerializeField, Min(0.1f)] private float _longPressSeconds = 0.5f;
        [SerializeField, Min(1)] private float _dragThresholdPixels = 14f;

        [Header("Hand")]
        [SerializeField, Min(0.1f)] private float _handSpacing = 1.15f;
        [SerializeField] private float _handFanDegrees = 5f;
        [SerializeField] private float _handTiltDegrees = 40f;

        [Header("Event log")]
        [SerializeField, Min(5)] private int _logLines = 60;

        public float MoveDuration => _moveDuration;
        public float AbilityShowSeconds => _abilityShowSeconds;
        public float FastForwardScale => _fastForwardScale;
        public float LongPressSeconds => _longPressSeconds;
        public float DragThresholdPixels => _dragThresholdPixels;
        public float HandSpacing => _handSpacing;
        public float HandFanDegrees => _handFanDegrees;
        public float HandTiltDegrees => _handTiltDegrees;
        public int LogLines => _logLines;
    }
}
