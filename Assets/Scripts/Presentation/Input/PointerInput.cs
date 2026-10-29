using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.Presentation.Input
{
    /// <summary>
    /// Turns mouse and touch into the same gestures: tap, long press, drag. The right mouse button is a
    /// shortcut for long press. The mouse also reports hovering, but only as a convenience: whatever it
    /// shows is reachable by a tap too, so every gesture works with a finger. Presses that start on the
    /// interface are left to the interface.
    /// </summary>
    public sealed class PointerInput : MonoBehaviour
    {
        [SerializeField] private float _longPressSeconds = 0.5f;
        [SerializeField] private float _dragThresholdPixels = 14f;

        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();
        private bool _pressed;
        private bool _dragging;
        private bool _longPressFired;
        private Vector2 _pressPosition;
        private float _pressTime;
        private Vector2? _hover;
        private Vector2? _hoverRaw;

        public event Action<Vector2> Tapped;
        public event Action<Vector2> LongPressed;
        public event Action<Vector2> DragStarted;
        public event Action<Vector2> Dragged;
        public event Action<Vector2> DragEnded;

        /// <summary>The mouse moved over the table with no button down; null when it left the table or went over the interface.</summary>
        public event Action<Vector2?> Hovered;

        public void Configure(float longPressSeconds, float dragThresholdPixels)
        {
            _longPressSeconds = longPressSeconds;
            _dragThresholdPixels = dragThresholdPixels;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null)
                return;

            var position = pointer.position.ReadValue();
            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame && !IsOverInterface(position))
            {
                LongPressed?.Invoke(position);
                return;
            }

            var press = pointer.press;
            if (mouse == null || pointer != mouse)
                TrackHover(null);
            else if (!press.isPressed)
                TrackHover(position);
            if (press.wasPressedThisFrame)
            {
                _pressed = !IsOverInterface(position);
                _dragging = false;
                _longPressFired = false;
                _pressPosition = position;
                _pressTime = Time.unscaledTime;
            }

            if (!_pressed)
                return;

            if (press.isPressed)
                TrackHold(position);

            if (press.wasReleasedThisFrame)
            {
                if (_dragging)
                    DragEnded?.Invoke(position);
                else if (!_longPressFired)
                    Tapped?.Invoke(position);
                _pressed = false;
                _dragging = false;
            }
        }

        /// <summary>Reports the hovered point when it changes; the interface check runs only after a move.</summary>
        private void TrackHover(Vector2? position)
        {
            if (position == _hoverRaw)
                return;
            _hoverRaw = position;
            var hover = position.HasValue && IsOverInterface(position.Value) ? null : position;
            if (hover == _hover)
                return;
            _hover = hover;
            Hovered?.Invoke(hover);
        }

        private void TrackHold(Vector2 position)
        {
            if (!_dragging && !_longPressFired && (position - _pressPosition).magnitude > _dragThresholdPixels)
            {
                _dragging = true;
                DragStarted?.Invoke(_pressPosition);
            }

            if (_dragging)
            {
                Dragged?.Invoke(position);
            }
            else if (!_longPressFired && Time.unscaledTime - _pressTime >= _longPressSeconds)
            {
                _longPressFired = true;
                LongPressed?.Invoke(position);
            }
        }

        private bool IsOverInterface(Vector2 position)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            _uiHits.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = position }, _uiHits);
            return _uiHits.Count > 0;
        }
    }
}
