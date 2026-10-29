using Game.Data.Presentation;
using Game.Presentation.Cards;
using UnityEngine;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// Lifts a card of the viewer's hand towards the camera, upright and large enough to read: the card
    /// under the mouse, or else the one the player tapped (it stays up until a tap elsewhere). At most one
    /// card is up at a time. Only the visible part moves, so the hand layout is not disturbed.
    /// </summary>
    public sealed class HandFocus
    {
        /// <summary>How much nearer to the camera a lifted card comes, so it covers its neighbours.</summary>
        private const float TowardCamera = 1.5f;

        /// <summary>Gap between the lifted card and the bottom of the safe area, in fractions of the screen height.</summary>
        private const float BottomMargin = 0.02f;

        private readonly Camera _camera;
        private readonly AnimationSettings _animation;
        private readonly float _cardHeight;
        private readonly float _cardAspect;
        private CardView _hovered;
        private CardView _pinned;
        private CardView _lifted;

        /// <param name="cardSize">Size of a card at scale 1, in world units.</param>
        public HandFocus(Camera camera, AnimationSettings animation, Vector2 cardSize)
        {
            _camera = camera;
            _animation = animation;
            _cardHeight = cardSize.y;
            _cardAspect = cardSize.x / cardSize.y;
        }

        /// <summary>The card the player tapped, if it is still in the hand.</summary>
        public CardView Pinned => Valid(_pinned);

        /// <summary>The mouse is over this card (null: over none); only hand cards lift.</summary>
        public void Hover(CardView card)
        {
            _hovered = card;
            Apply();
        }

        /// <summary>Keeps the card up until <see cref="Unpin"/>.</summary>
        public void Pin(CardView card)
        {
            _pinned = card;
            Apply();
        }

        public void Unpin()
        {
            _pinned = null;
            Apply();
        }

        /// <summary>Drops every lifted card at once, e.g. before a drag.</summary>
        public void Clear()
        {
            _hovered = null;
            _pinned = null;
            var lifted = Valid(_lifted);
            if (lifted != null)
                lifted.ResetVisual();
            _lifted = null;
        }

        private void Apply()
        {
            var wanted = Valid(_hovered) ?? Valid(_pinned);
            var lifted = Valid(_lifted);
            if (wanted == lifted)
                return;

            if (lifted != null)
                lifted.ReleaseVisual(_animation.HandFocusSeconds);
            _lifted = wanted;
            if (wanted != null)
                Lift(wanted);
        }

        private void Lift(CardView card)
        {
            var cameraTransform = _camera.transform;
            var depth = Vector3.Dot(card.transform.position - cameraTransform.position, cameraTransform.forward) - TowardCamera;
            var share = _animation.HandFocusHeight;
            var worldHeight = 2f * depth * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * share;

            var safe = Screen.safeArea;
            var heightPixels = share * Screen.height;
            var halfWidthPixels = heightPixels * _cardAspect / 2f;
            var x = Mathf.Clamp(_camera.WorldToScreenPoint(card.transform.position).x, safe.xMin + halfWidthPixels, safe.xMax - halfWidthPixels);
            var y = safe.yMin + BottomMargin * Screen.height + heightPixels / 2f;
            var position = _camera.ScreenToWorldPoint(new Vector3(x, y, depth));
            var rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
            card.FocusVisual(position, rotation, worldHeight / _cardHeight, _animation.HandFocusSeconds);
        }

        /// <summary>The card if it still exists and lies in the hand; destroyed views compare equal to null.</summary>
        private static CardView Valid(CardView card) => card != null && card.IsInHand ? card : null;
    }
}
