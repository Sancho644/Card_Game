using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Cards;
using Game.Presentation.Input;
using Game.Presentation.Ui;
using UnityEngine;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// Connects pointer gestures on the 3D table to the action selector: tap or drag to choose,
    /// long press or right click to look at a card. A dragged card lands on the nearest target on screen,
    /// and the target it would land on is lit while dragging. While animations play, a tap speeds them up.
    /// </summary>
    public sealed class TableInteraction : IDisposable
    {
        private const float DragHeight = 1.6f;

        private readonly PointerInput _input;
        private readonly TableRaycaster _raycaster;
        private readonly CardViewRegistry _registry;
        private readonly ActionSelector _selector;
        private readonly DropTargetPicker _picker;
        private readonly CardZoomView _zoom;
        private readonly CardFaceBuilder _faces;
        private readonly AnimationQueue _queue;
        private readonly VisualSettings _visual;
        private readonly TablePresentationSettings _settings;
        private CardView _dragged;
        private int? _dropTarget;
        private bool _enabled;
        private bool _disposed;

        public TableInteraction(
            PointerInput input,
            TableRaycaster raycaster,
            CardViewRegistry registry,
            ActionSelector selector,
            DropTargetPicker picker,
            CardZoomView zoom,
            CardFaceBuilder faces,
            AnimationQueue queue,
            VisualSettings visual,
            TablePresentationSettings settings)
        {
            _input = input;
            _raycaster = raycaster;
            _registry = registry;
            _selector = selector;
            _picker = picker;
            _zoom = zoom;
            _faces = faces;
            _queue = queue;
            _visual = visual;
            _settings = settings;

            _input.Tapped += OnTap;
            _input.LongPressed += OnLongPress;
            _input.DragStarted += OnDragStarted;
            _input.Dragged += OnDragged;
            _input.DragEnded += OnDragEnded;
            _selector.Changed += ApplyHighlights;
            _queue.Idle += OnQueueIdle;
        }

        /// <summary>A dragged card was dropped somewhere invalid; it should go back to its place.</summary>
        public event Action ResyncRequested;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                ApplyHighlights();
            }
        }

        public void ApplyHighlights()
        {
            if (_disposed)
                return;

            foreach (var card in _registry.All)
                card.SetHighlight(null);
            if (!_enabled)
                return;

            if (!_selector.IsSelecting)
            {
                foreach (var id in _selector.Sources)
                    Highlight(id, _visual.Selectable);
                return;
            }

            if (_selector.Source != null)
                Highlight(_selector.Source.Value, _visual.Selected);
            foreach (var id in _selector.Targets)
                Highlight(id, id == _dropTarget ? _visual.DropTarget : _visual.Target);
        }

        /// <summary>Stops listening to input and never touches the views again; they may already be destroyed.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _input.Tapped -= OnTap;
            _input.LongPressed -= OnLongPress;
            _input.DragStarted -= OnDragStarted;
            _input.Dragged -= OnDragged;
            _input.DragEnded -= OnDragEnded;
            _selector.Changed -= ApplyHighlights;
            _queue.Idle -= OnQueueIdle;
        }

        private void Highlight(int instanceId, Color color)
        {
            var card = _registry.Find(instanceId);
            if (card != null)
                card.SetHighlight(color);
        }

        private void OnTap(Vector2 position)
        {
            if (_queue.IsBusy)
            {
                DOTween.timeScale = _settings.FastForwardScale;
                return;
            }

            if (!_enabled)
                return;

            var card = _raycaster.CardAt(position);
            if (card != null && (_selector.Sources.Contains(card.InstanceId) || _selector.Targets.Contains(card.InstanceId)))
            {
                _selector.TapCard(card.InstanceId);
                return;
            }

            // A tap near a target, e.g. on the opponent's creature that stands on the target field.
            var target = _selector.Source != null ? TargetNear(position) : null;
            if (target != null)
                _selector.TapCard(target.Value);
            else if (card != null)
                _selector.TapCard(card.InstanceId);
            else if (_selector.IsSelecting && _selector.Source != null)
                _selector.Cancel();
        }

        private void OnLongPress(Vector2 position)
        {
            var card = _raycaster.CardAt(position);
            if (card != null && card.IsKnown)
                _zoom.Show(_faces.Build(card.CardId));
        }

        private void OnDragStarted(Vector2 position)
        {
            if (!_enabled || _queue.IsBusy)
                return;

            var card = _raycaster.CardAt(position);
            if (card == null || !_selector.Sources.Contains(card.InstanceId))
                return;

            if (_selector.Source != card.InstanceId)
                _selector.TapCard(card.InstanceId);
            if (_selector.Source != card.InstanceId)
                return;

            _dragged = card;
            _dragged.Collider.enabled = false;
        }

        private void OnDragged(Vector2 position)
        {
            if (_dragged == null)
                return;

            _dragged.transform.position = _raycaster.PointAtHeight(position, DragHeight);
            var target = TargetNear(position);
            if (target == _dropTarget)
                return;
            _dropTarget = target;
            ApplyHighlights();
        }

        private void OnDragEnded(Vector2 position)
        {
            if (_dragged == null)
                return;

            var dragged = _dragged;
            _dragged = null;
            _dropTarget = null;
            if (dragged != null)
                dragged.Collider.enabled = true;

            var target = TargetNear(position);
            if (target != null)
                _selector.TapCard(target.Value);
            else
                _selector.Cancel();
            ResyncRequested?.Invoke();
        }

        /// <summary>The selectable target a release at this point lands on: the one under it or the nearest within the radius.</summary>
        private int? TargetNear(Vector2 position)
        {
            var under = _raycaster.CardAt(position, _dragged);
            if (under != null && _selector.Targets.Contains(under.InstanceId))
                return under.InstanceId;

            var candidates = new List<DropCandidate>();
            foreach (var id in _selector.Targets)
            {
                var card = _registry.Find(id);
                if (card != null)
                    candidates.Add(new DropCandidate(id, _raycaster.ScreenRectOf(card)));
            }

            var height = Screen.height;
            return _picker.Pick(position, candidates, _settings.DropPadding * height, _settings.DropRadius * height);
        }

        private void OnQueueIdle() => DOTween.timeScale = 1f;
    }
}
