using UnityEngine;

namespace Game.Data.Presentation
{
    /// <summary>
    /// Durations and speed of table animations. Durations are at normal speed; the speed settings scale all
    /// of them at once.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Config/Animation Settings", fileName = "AnimationSettings")]
    public sealed class AnimationSettings : ScriptableObject
    {
        [Header("Speed")]
        [Tooltip("Speed of every animation; 1 is as authored.")]
        [SerializeField, Min(0.1f)] private float _speed = 1f;
        [Tooltip("Extra speed when the player turned on fast animations.")]
        [SerializeField, Min(1f)] private float _fastSpeed = 2f;
        [Tooltip("Extra speed while the player taps during animations.")]
        [SerializeField, Min(1f)] private float _tapFastForward = 4f;

        [Header("Cards, seconds")]
        [SerializeField, Min(0)] private float _moveDuration = 0.35f;

        [Header("Looking at cards")]
        [Tooltip("A hand card under the mouse or tapped rises and grows to this share of the screen height.")]
        [SerializeField, Range(0.1f, 0.9f)] private float _handFocusHeight = 0.42f;
        [SerializeField, Min(0)] private float _handFocusSeconds = 0.12f;
        [Tooltip("A card looked at closely flies to the centre and grows to this share of the screen height.")]
        [SerializeField, Range(0.3f, 1f)] private float _zoomHeight = 0.82f;
        [SerializeField, Min(0)] private float _zoomOpenSeconds = 0.22f;
        [SerializeField, Min(0)] private float _zoomCloseSeconds = 0.18f;

        [Header("Battle, seconds")]
        [SerializeField, Min(0)] private float _abilityShowSeconds = 0.8f;
        [SerializeField, Min(0)] private float _powerPopupSeconds = 1.1f;

        [Header("Messages, seconds")]
        [SerializeField, Min(0)] private float _bannerSeconds = 1.6f;

        public float Speed => _speed;
        public float FastSpeed => _fastSpeed;
        public float TapFastForward => _tapFastForward;
        public float MoveDuration => _moveDuration;
        public float HandFocusHeight => _handFocusHeight;
        public float HandFocusSeconds => _handFocusSeconds;
        public float ZoomHeight => _zoomHeight;
        public float ZoomOpenSeconds => _zoomOpenSeconds;
        public float ZoomCloseSeconds => _zoomCloseSeconds;
        public float AbilityShowSeconds => _abilityShowSeconds;
        public float PowerPopupSeconds => _powerPopupSeconds;
        public float BannerSeconds => _bannerSeconds;
    }
}
