using System;
using DG.Tweening;
using Game.Data.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Presentation.Menu
{
    /// <summary>Mouse/touch tilt of a preview card; dragging never counts as the click that closes it.</summary>
    public sealed class CardPreviewRotation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private RectTransform _card;
        private CollectionSettings _settings;
        private Action _clicked;
        private Tween _returnTween;
        private int _pointerId;
        private bool _pressed;
        private bool _dragged;
        private Vector2 _start;
        private Vector2 _size;
        private Vector2 _startAngles;

        public bool Interactable { get; set; }

        public void Configure(RectTransform card, CollectionSettings settings, Action clicked)
        {
            _card = card; _settings = settings; _clicked = clicked;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (!Interactable || _pressed || data.button != PointerEventData.InputButton.Left) return;
            _returnTween?.Kill();
            _pointerId = data.pointerId; _pressed = true; _dragged = false; _start = data.position;
            var canvas = _card.GetComponentInParent<Canvas>().rootCanvas;
            _size = _card.rect.size * canvas.scaleFactor;
            var angles = _card.localEulerAngles;
            _startAngles = new Vector2(Mathf.DeltaAngle(0, angles.x), Mathf.DeltaAngle(0, angles.y));
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (!Owns(data)) return;
            _dragged = true; data.eligibleForClick = false;
        }

        public void OnDrag(PointerEventData data)
        {
            if (!Owns(data)) return;
            _dragged = true; data.eligibleForClick = false;
            var delta = data.position - _start;
            var limit = _settings.PreviewTiltDegrees;
            var x = Mathf.Clamp(_startAngles.x - delta.y / Mathf.Max(1, _size.y) * limit * 2, -limit, limit);
            var y = Mathf.Clamp(_startAngles.y + delta.x / Mathf.Max(1, _size.x) * limit * 2, -limit, limit);
            _card.localRotation = Quaternion.Euler(x, y, 0);
        }

        public void OnPointerUp(PointerEventData data) => Release(data);
        public void OnEndDrag(PointerEventData data) => Release(data);

        public void OnPointerClick(PointerEventData data)
        {
            if (Interactable && data.pointerId == _pointerId && !_dragged && data.button == PointerEventData.InputButton.Left)
                _clicked?.Invoke();
        }

        /// <summary>Cancel gestures and pending return animation before the preview closes or is destroyed.</summary>
        public void Stop()
        {
            Interactable = false; _pressed = false;
            _returnTween?.Kill(); _returnTween = null;
            DOTween.Kill(this, complete: false);
        }

        private bool Owns(PointerEventData data) => Interactable && _pressed && data.pointerId == _pointerId;

        private void Release(PointerEventData data)
        {
            if (!Owns(data)) return;
            _pressed = false;
            _returnTween?.Kill();
            _returnTween = _card.DOLocalRotateQuaternion(Quaternion.identity, _settings.PreviewTiltReturnSeconds)
                .SetEase(Ease.OutCubic).SetUpdate(true).SetTarget(this);
        }

        private void OnDisable() => Stop();
        private void OnDestroy() => Stop();
    }
}
