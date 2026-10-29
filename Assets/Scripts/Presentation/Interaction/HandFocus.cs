using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using UnityEngine;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// Lifts a card of the viewer's hand towards the camera, upright and large enough to read: the card
    /// under the mouse, or else the one the player tapped (it stays up until a tap elsewhere). At most one
    /// card is up at a time, kept whole inside the safe area; its neighbours move slightly aside. Only the
    /// visible parts move: the cards' places in the fan, and so the areas that catch the pointer, stay put.
    /// </summary>
    public sealed class HandFocus
    {
        /// <summary>How much nearer to the camera a lifted card comes, so it covers its neighbours.</summary>
        private const float TowardCamera = 1.5f;

        /// <summary>Gap between the lifted card and the edges of the safe area, in fractions of the screen height.</summary>
        private const float Margin = 0.015f;

        /// <summary>The highlight around a card is this much larger than the card; it must stay on screen too.</summary>
        private const float OutlineWidth = 1.16f;
        private const float OutlineHeight = 1.11f;

        /// <summary>Neighbours this many places away from the lifted card move aside, the nearer ones more.</summary>
        private const int SpreadReach = 2;

        private readonly Camera _camera;
        private readonly AnimationSettings _animation;
        private readonly Func<IEnumerable<CardView>> _cards;
        private readonly float _cardHeight;
        private readonly float _cardAspect;
        private CardView _hovered;
        private CardView _pinned;
        private CardView _lifted;

        /// <param name="cards">Every card view on the table; the hand is taken from it.</param>
        /// <param name="cardSize">Size of a card at scale 1, in world units.</param>
        public HandFocus(Camera camera, AnimationSettings animation, Func<IEnumerable<CardView>> cards, Vector2 cardSize)
        {
            _camera = camera;
            _animation = animation;
            _cards = cards;
            _cardHeight = cardSize.y;
            _cardAspect = cardSize.x / cardSize.y;
        }

        /// <summary>The pointer is over this card (null: over none); only hand cards lift.</summary>
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

        /// <summary>A card picked up to drag: it grows a little and turns to face the player; the hand settles.</summary>
        public void Grab(CardView card)
        {
            Clear();
            var facing = Quaternion.LookRotation(_camera.transform.forward, _camera.transform.up);
            card.FocusVisual(card.transform.position, facing, _animation.DragScale * card.transform.lossyScale.x, _animation.HandFocusSeconds);
        }

        /// <summary>A dragged card was let go: it shrinks back on its way to wherever it goes.</summary>
        public void Drop(CardView card) => card.ReleaseVisual(_animation.HandFocusSeconds);

        /// <summary>Puts every hand card back at once, e.g. before a drag.</summary>
        public void Clear()
        {
            _hovered = null;
            _pinned = null;
            _lifted = null;
            foreach (var card in Hand())
            {
                if (card.IsDisplaced)
                    card.ResetVisual();
            }
        }

        private void Apply()
        {
            var wanted = Valid(_hovered) ?? Valid(_pinned);
            if (wanted == Valid(_lifted))
                return;

            _lifted = wanted;
            var hand = Hand();
            var liftedIndex = wanted == null ? -1 : hand.IndexOf(wanted);
            var duration = _animation.HandFocusSeconds;
            for (var i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                var distance = liftedIndex < 0 ? 0 : i - liftedIndex;
                if (card == wanted)
                    Lift(card);
                else if (distance != 0 && Math.Abs(distance) <= SpreadReach)
                    card.NudgeVisual(_camera.transform.right * (Math.Sign(distance) * _animation.HandSpread / Math.Abs(distance)), duration);
                else
                    card.ReleaseVisual(duration);
            }
        }

        private List<CardView> Hand() =>
            _cards().Where(card => card != null && card.IsInHand).OrderBy(card => card.transform.position.x).ToList();

        private void Lift(CardView card)
        {
            var cameraTransform = _camera.transform;
            var depth = Vector3.Dot(card.transform.position - cameraTransform.position, cameraTransform.forward) - TowardCamera;
            var safe = Screen.safeArea;
            var margin = Margin * Screen.height;
            var share = Mathf.Min(_animation.HandFocusHeight, (safe.height - 2f * margin) / (Screen.height * OutlineHeight));
            var worldHeight = 2f * depth * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * share;

            // The whole card with its highlight stays inside the safe area: near an edge it moves inwards.
            var heightPixels = share * Screen.height;
            var halfWidth = heightPixels * _cardAspect * OutlineWidth / 2f + margin;
            var x = _camera.WorldToScreenPoint(card.transform.position).x;
            x = safe.width > 2f * halfWidth ? Mathf.Clamp(x, safe.xMin + halfWidth, safe.xMax - halfWidth) : safe.center.x;
            var y = safe.yMin + margin + heightPixels * OutlineHeight / 2f;
            var position = _camera.ScreenToWorldPoint(new Vector3(x, y, depth));
            var rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
            card.FocusVisual(position, rotation, worldHeight / _cardHeight, _animation.HandFocusSeconds);
        }

        /// <summary>The card if it still exists and lies in the hand; destroyed views compare equal to null.</summary>
        private static CardView Valid(CardView card) => card != null && card.IsInHand ? card : null;
    }
}
