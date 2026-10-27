using System;
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
    /// long press or right click to look at a card. While animations play, a tap speeds them up.
    /// </summary>
    public sealed class TableInteraction : IDisposable
    {
        private const float DragHeight = 1.6f;

        private readonly PointerInput _input;
        private readonly TableRaycaster _raycaster;
        private readonly CardViewRegistry _registry;
        private readonly ActionSelector _selector;
        private readonly CardZoomView _zoom;
        private readonly CardFaceBuilder _faces;
        private readonly AnimationQueue _queue;
        private readonly VisualSettings _visual;
        private readonly TablePresentationSettings _settings;
        private CardView _dragged;
        private bool _enabled;

        public TableInteraction(
            PointerInput input,
            TableRaycaster raycaster,
            CardViewRegistry registry,
            ActionSelector selector,
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
            foreach (var card in _registry.All)
                card.SetHighlight(null);
            if (!_enabled)
                return;

            if (!_selector.IsSelecting)
            {
                foreach (var id in _selector.Sources)
                    _registry.Find(id)?.SetHighlight(_visual.Selectable);
                return;
            }

            if (_selector.Source != null)
                _registry.Find(_selector.Source.Value)?.SetHighlight(_visual.Selected);
            foreach (var id in _selector.Targets)
                _registry.Find(id)?.SetHighlight(_visual.Target);
        }

        public void Dispose()
        {
            _input.Tapped -= OnTap;
            _input.LongPressed -= OnLongPress;
            _input.DragStarted -= OnDragStarted;
            _input.Dragged -= OnDragged;
            _input.DragEnded -= OnDragEnded;
            _selector.Changed -= ApplyHighlights;
            _queue.Idle -= OnQueueIdle;
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
            if (card != null)
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
            if (_dragged != null)
                _dragged.transform.position = _raycaster.PointAtHeight(position, DragHeight);
        }

        private void OnDragEnded(Vector2 position)
        {
            if (_dragged == null)
                return;

            var dragged = _dragged;
            _dragged = null;
            dragged.Collider.enabled = true;

            var target = _raycaster.CardAt(position, dragged);
            if (target != null && _selector.Targets.Contains(target.InstanceId))
                _selector.TapCard(target.InstanceId);
            else
                _selector.Cancel();
            ResyncRequested?.Invoke();
        }

        private void OnQueueIdle() => DOTween.timeScale = 1f;
    }
}
