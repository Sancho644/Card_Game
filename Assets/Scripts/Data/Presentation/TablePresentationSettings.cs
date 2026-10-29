using UnityEngine;

namespace Game.Data.Presentation
{
    /// <summary>Input and layout numbers of the table scene; animation timings are in <see cref="AnimationSettings"/>.</summary>
    [CreateAssetMenu(menuName = "Game/Config/Table Presentation", fileName = "TablePresentation")]
    public sealed class TablePresentationSettings : ScriptableObject
    {
        [Header("Input")]
        [SerializeField, Min(0.1f)] private float _longPressSeconds = 0.5f;
        [SerializeField, Min(1)] private float _dragThresholdPixels = 14f;
        [Tooltip("Extra room around a drop target, in fractions of the screen height.")]
        [SerializeField, Range(0f, 0.2f)] private float _dropPadding = 0.03f;
        [Tooltip("How far from a target the card may be released and still land on it, in fractions of the screen height.")]
        [SerializeField, Range(0f, 0.5f)] private float _dropRadius = 0.12f;

        [Header("Camera")]
        [Tooltip("Angle between the view and the table, degrees: 90 looks straight down. Can be tuned in Play mode.")]
        [SerializeField, Range(30f, 90f)] private float _cameraPitch = 68f;
        [Tooltip("Height of the camera above the table, in world units.")]
        [SerializeField, Min(1f)] private float _cameraHeight = 15f;
        [Tooltip("Vertical angle of view, degrees. Grows on its own only when the table does not fit, e.g. in a narrow safe area.")]
        [SerializeField, Range(10f, 90f)] private float _cameraFieldOfView = 36f;

        [Header("Hand")]
        [SerializeField, Min(0.1f)] private float _handSpacing = 1.15f;
        [SerializeField] private float _handFanDegrees = 5f;
        [Tooltip("Free space between the hand and the edges of the safe area, in fractions of the screen width.")]
        [SerializeField, Range(0f, 0.2f)] private float _handScreenMargin = 0.03f;

        [Header("Fields")]
        [SerializeField, Min(0.5f)] private float _fieldScale = 1.2f;
        [SerializeField, Min(0.3f)] private float _creatureOnFieldScale = 0.8f;
        [Tooltip("How much of the field's edge a creature covers, in world units.")]
        [SerializeField, Min(0f)] private float _creatureOverlap = 0.3f;

        [Header("Screen")]
        [Tooltip("Top part of the screen covered by the top bar, in fractions of the screen height.")]
        [SerializeField, Range(0f, 0.3f)] private float _topBarShare = 0.08f;

        [Header("Event log")]
        [SerializeField, Min(5)] private int _logLines = 60;

        public float LongPressSeconds => _longPressSeconds;
        public float DragThresholdPixels => _dragThresholdPixels;
        public float DropPadding => _dropPadding;
        public float DropRadius => _dropRadius;
        public float HandSpacing => _handSpacing;
        public float HandFanDegrees => _handFanDegrees;
        public float CameraPitch => _cameraPitch;
        public float CameraHeight => _cameraHeight;
        public float CameraFieldOfView => _cameraFieldOfView;
        public float HandScreenMargin => _handScreenMargin;
        public float FieldScale => _fieldScale;
        public float CreatureOnFieldScale => _creatureOnFieldScale;
        public float CreatureOverlap => _creatureOverlap;
        public float TopBarShare => _topBarShare;
        public int LogLines => _logLines;
    }
}
