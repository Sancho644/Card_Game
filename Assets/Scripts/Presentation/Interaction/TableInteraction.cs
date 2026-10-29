using System;
using System.Linq;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Cards;
using Game.Presentation.Input;
using Game.Presentation.Ui;
using UnityEngine;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// Connects pointer gestures on the 3D table to looking at cards and to the action selector.
    /// <list type="bullet">
    /// <item>A hand card lifts to be read while the mouse is over it, or after a tap until a tap elsewhere;
    /// a tap on a playable hand card also chooses it.</item>
    /// <item>A tap on a card the player can choose now (lit) chooses it; a tap on any other face-up card on
    /// the table shows it large; long press or right click shows any face-up card large. Backs are not shown.</item>
    /// <item>While a chosen card waits for its target, only a tap or a release right on a lit target uses it;
    /// anywhere else the choice is dropped and nothing is sent.</item>
    /// <item>Looking at cards never changes the match, so it works on the opponent's turn and while
    /// animations play; a tap on the empty table then speeds the animations up.</item>
    /// <item>While a card is shown large, the table takes no input; a tap anywhere sends it back.</item>
    /// </list>
    /// </summary>
    public sealed class TableInteraction : IDisposable
    {
        private const float DragHeight = 1.6f;

        private readonly PointerInput _input;
        private readonly TableRaycaster _raycaster;
        private readonly CardViewRegistry _registry;
        private readonly ActionSelector _selector;
        private readonly CardZoomView _zoom;
        private readonly HandFocus _focus;
        private readonly CardFaceBuilder _faces;
        private readonly AnimationQueue _queue;
        private readonly AnimationSpeed _speed;
        private readonly VisualSettings _visual;
        private CardView _dragged;
        private int? _dropTarget;
        private bool _enabled;
        private bool _disposed;

        public TableInteraction(
            PointerInput input,
            TableRaycaster raycaster,
            CardViewRegistry registry,
            ActionSelector selector,
            CardZoomView zoom,
            HandFocus focus,
            CardFaceBuilder faces,
            AnimationQueue queue,
            AnimationSpeed speed,
            VisualSettings visual)
        {
            _input = input;
            _raycaster = raycaster;
            _registry = registry;
            _selector = selector;
            _zoom = zoom;
            _focus = focus;
            _faces = faces;
            _queue = queue;
            _speed = speed;
            _visual = visual;

            _input.Tapped += OnTap;
            _input.LongPressed += OnLongPress;
            _input.Hovered += OnHover;
            _input.DragStarted += OnDragStarted;
            _input.Dragged += OnDragged;
            _input.DragEnded += OnDragEnded;
            _selector.Changed += ApplyHighlights;
            _queue.Idle += OnQueueIdle;
        }

        /// <summary>A dragged card was dropped somewhere invalid; it should go back to its place.</summary>
        public event Action ResyncRequested;

        /// <summary>The local player may choose actions now.</summary>
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

        /// <summary>Puts down whatever the player is looking at, e.g. before the table is handed to the other hot-seat player.</summary>
        public void StopLooking()
        {
            if (_disposed)
                return;
            _zoom.HideInstantly();
            _focus.Clear();
        }

        /// <summary>Stops listening to input and never touches the views again; they may already be destroyed.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _input.Tapped -= OnTap;
            _input.LongPressed -= OnLongPress;
            _input.Hovered -= OnHover;
            _input.DragStarted -= OnDragStarted;
            _input.Dragged -= OnDragged;
            _input.DragEnded -= OnDragEnded;
            _selector.Changed -= ApplyHighlights;
            _queue.Idle -= OnQueueIdle;
        }

        /// <summary>Actions can be chosen now: it is the player's decision and nothing is playing.</summary>
        private bool CanAct => _enabled && !_queue.IsBusy;

        private void Highlight(int instanceId, Color color)
        {
            var card = _registry.Find(instanceId);
            if (card != null)
                card.SetHighlight(color);
        }

        private bool IsChoosable(CardView card) =>
            CanAct && (_selector.Sources.Contains(card.InstanceId) || _selector.Targets.Contains(card.InstanceId));

        private void OnHover(Vector2? position)
        {
            if (_zoom.IsOpen || _dragged != null)
                return;

            var card = position.HasValue ? _raycaster.CardAt(position.Value) : null;
            _focus.Hover(card != null && card.IsInHand ? card : null);
        }

        private void OnTap(Vector2 position)
        {
            if (_zoom.IsOpen)
                return;

            var card = _raycaster.CardAt(position);
            if (CanAct && _selector.Source != null && TapWhileChosen(position, card))
                return;

            if (card == null)
            {
                _focus.Unpin();
                if (_queue.IsBusy)
                    _speed.FastForward();
                return;
            }

            if (card.IsInHand)
            {
                _focus.Pin(card);
                if (IsChoosable(card))
                    _selector.TapCard(card.InstanceId);
                return;
            }

            _focus.Unpin();
            if (IsChoosable(card))
                _selector.TapCard(card.InstanceId);
            else
                ShowLarge(card);
        }

        /// <summary>
        /// A card is chosen and waits for its target: a tap on a lit target uses it, a tap on another hand card
        /// chooses that one instead, any other tap drops the choice. True when the tap is handled.
        /// </summary>
        private bool TapWhileChosen(Vector2 position, CardView card)
        {
            var target = TargetAt(position);
            if (target != null)
            {
                _focus.Unpin();
                _selector.TapCard(target.Value);
                return true;
            }

            if (card != null && card.IsInHand)
                return false;

            _focus.Unpin();
            _selector.Cancel();
            return true;
        }

        private void OnLongPress(Vector2 position)
        {
            if (_zoom.IsOpen || _dragged != null)
                return;

            var card = _raycaster.CardAt(position);
            if (card != null)
                ShowLarge(card);
        }

        /// <summary>
        /// The card flies to the centre of the screen; its place stays empty until it flies back. A back has
        /// nothing to show: the opponent's hand and closed fields stay where they are.
        /// </summary>
        private void ShowLarge(CardView card)
        {
            if (!card.IsKnown)
                return;
            _focus.Hover(null);
            var from = _raycaster.ScreenRectOf(card);
            card.SetVisible(false);
            _zoom.Show(_faces.Build(card.CardId), from, () =>
            {
                if (card != null)
                    card.SetVisible(true);
            });
        }

        private void OnDragStarted(Vector2 position)
        {
            if (_zoom.IsOpen || !CanAct)
                return;

            var card = _raycaster.CardAt(position);
            if (card == null || !_selector.Sources.Contains(card.InstanceId))
                return;

            if (_selector.Source != card.InstanceId)
                _selector.TapCard(card.InstanceId);
            if (_selector.Source != card.InstanceId)
                return;

            _focus.Clear();
            _dragged = card;
            _dragged.Collider.enabled = false;
        }

        private void OnDragged(Vector2 position)
        {
            if (_dragged == null)
                return;

            _dragged.transform.position = _raycaster.PointAtHeight(position, DragHeight);
            var target = TargetAt(position);
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

            var target = TargetAt(position);
            if (target != null)
                _selector.TapCard(target.Value);
            else
                _selector.Cancel();
            ResyncRequested?.Invoke();
        }

        /// <summary>The lit target right under the point, if any; a creature standing on a target field does not hide it.</summary>
        private int? TargetAt(Vector2 position)
        {
            var targets = _selector.Targets;
            foreach (var card in _raycaster.CardsAt(position, _dragged))
            {
                if (targets.Contains(card.InstanceId))
                    return card.InstanceId;
            }

            return null;
        }

        private void OnQueueIdle() => _speed.EndFastForward();
    }
}
