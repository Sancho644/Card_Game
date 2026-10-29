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
        [Tooltip("Pause between cards flying in one by one when they are dealt.")]
        [SerializeField, Min(0)] private float _dealInterval = 0.09f;
        [Tooltip("A dragged card grows by this much and turns to face the player.")]
        [SerializeField, Range(1f, 2f)] private float _dragScale = 1.2f;

        [Header("Looking at cards")]
        [Tooltip("A hand card under the mouse or tapped rises and grows to this share of the screen height.")]
        [SerializeField, Range(0.1f, 0.9f)] private float _handFocusHeight = 0.42f;
        [SerializeField, Min(0)] private float _handFocusSeconds = 0.12f;
        [Tooltip("How far the next card moves aside from a lifted one, in world units; the one after moves half as far.")]
        [SerializeField, Min(0)] private float _handSpread = 0.35f;
        [Tooltip("A card looked at closely flies to the centre and grows to this share of the screen height.")]
        [SerializeField, Range(0.3f, 1f)] private float _zoomHeight = 0.82f;
        [SerializeField, Min(0)] private float _zoomOpenSeconds = 0.22f;
        [SerializeField, Min(0)] private float _zoomCloseSeconds = 0.18f;

        [Header("Battle")]
        [Tooltip("A field card turning over when it is opened, seconds.")]
        [SerializeField, Min(0)] private float _flipSeconds = 0.45f;
        [Tooltip("How high a field card rises while it turns over, in world units.")]
        [SerializeField, Min(0)] private float _flipLift = 1.2f;
        [Tooltip("How long the «Битва!» banner stays when a battle starts, seconds.")]
        [SerializeField, Min(0)] private float _battleStartSeconds = 1.0f;
        [Tooltip("How much the cards of a starting battle swell for a moment.")]
        [SerializeField, Range(0f, 0.5f)] private float _battlePulse = 0.15f;
        [Tooltip("A played ability is shown in the middle of the table this much larger.")]
        [SerializeField, Range(1f, 3f)] private float _abilityShowScale = 1.6f;
        [SerializeField, Min(0)] private float _abilityShowSeconds = 0.8f;
        [Tooltip("The power on a creature counts to its new value over this time, seconds.")]
        [SerializeField, Min(0)] private float _powerCountSeconds = 0.6f;
        [SerializeField, Min(0)] private float _powerPopupSeconds = 1.1f;

        [Header("Messages, seconds")]
        [SerializeField, Min(0)] private float _bannerSeconds = 1.6f;

        public float Speed => _speed;
        public float FastSpeed => _fastSpeed;
        public float TapFastForward => _tapFastForward;
        public float MoveDuration => _moveDuration;
        public float DealInterval => _dealInterval;
        public float DragScale => _dragScale;
        public float HandFocusHeight => _handFocusHeight;
        public float HandFocusSeconds => _handFocusSeconds;
        public float HandSpread => _handSpread;
        public float ZoomHeight => _zoomHeight;
        public float ZoomOpenSeconds => _zoomOpenSeconds;
        public float ZoomCloseSeconds => _zoomCloseSeconds;
        public float FlipSeconds => _flipSeconds;
        public float FlipLift => _flipLift;
        public float BattleStartSeconds => _battleStartSeconds;
        public float BattlePulse => _battlePulse;
        public float AbilityShowScale => _abilityShowScale;
        public float AbilityShowSeconds => _abilityShowSeconds;
        public float PowerCountSeconds => _powerCountSeconds;
        public float PowerPopupSeconds => _powerPopupSeconds;
        public float BannerSeconds => _bannerSeconds;
    }
}
