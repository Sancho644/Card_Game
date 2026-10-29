using System;
using DG.Tweening;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>
    /// A card shown large over the table, laid out like the card itself: it flies from where the card lies
    /// to the centre of the screen over a darkened table, and back when the player taps anywhere. Runs on real
    /// time, so it stays quick while animations are sped up.
    /// </summary>
    public sealed class CardZoomView : MonoBehaviour
    {
        [SerializeField] private Image _dim;
        [SerializeField] private RectTransform _card;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _rarityBadge;
        [SerializeField] private Image _art;
        [SerializeField] private Image _artPlaceholder;
        [SerializeField] private Image _artIcon;
        [SerializeField] private Image _strip;
        [SerializeField] private Image _stripIcon;
        [SerializeField] private GameObject _basePower;
        [SerializeField] private TMP_Text _basePowerText;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _stats;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private TMP_Text _rarity;
        [SerializeField] private Button _close;

        private CardSkin _skin;
        private AnimationSettings _animation;
        private float _dimAlpha;
        private Rect _from;
        private Action _closed;
        private Sequence _tween;

        public bool IsOpen => gameObject.activeSelf;

        public void Init(CardSkin skin, AnimationSettings animation)
        {
            _skin = skin;
            _animation = animation;
            _dimAlpha = _dim.color.a;
        }

        /// <param name="from">Where the card is on the screen, in pixels; the view grows out of it and returns there.</param>
        /// <param name="closed">Called once the view has flown back.</param>
        public void Show(CardFace face, Rect from, Action closed)
        {
            _from = from;
            _closed = closed;
            ShowFace(face);
            UiButtons.Bind(_close, Hide);
            _close.interactable = true;
            gameObject.SetActive(true);
            Fly(opening: true);
        }

        /// <summary>Flies back to the card's place and closes.</summary>
        public void Hide()
        {
            if (!IsOpen)
                return;
            _close.interactable = false;
            Fly(opening: false);
        }

        /// <summary>Closes at once, without the flight, e.g. before the next hot-seat player looks at the table.</summary>
        public void HideInstantly()
        {
            _tween?.Kill();
            Closed();
        }

        private void Fly(bool opening)
        {
            _tween?.Kill();
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var area = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, _from.center, camera, out var start);
            var pixelsPerUnit = canvas.scaleFactor;
            var cardHeight = _card.rect.height * pixelsPerUnit;
            var startScale = Mathf.Max(0.05f, _from.height / cardHeight);
            var safe = Screen.safeArea;
            var endScale = Mathf.Min(_animation.ZoomHeight * safe.height / cardHeight, 0.95f * safe.width / (_card.rect.width * pixelsPerUnit));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, safe.center, camera, out var end);

            if (opening)
            {
                _card.anchoredPosition = start;
                _card.localScale = Vector3.one * startScale;
                SetDim(0f);
            }

            var duration = opening ? _animation.ZoomOpenSeconds : _animation.ZoomCloseSeconds;
            var ease = opening ? Ease.OutCubic : Ease.InCubic;
            _tween = DOTween.Sequence()
                .Join(_card.DOAnchorPos(opening ? end : start, duration).SetEase(ease))
                .Join(_card.DOScale(opening ? endScale : startScale, duration).SetEase(ease))
                .Join(_dim.DOFade(opening ? _dimAlpha : 0f, duration))
                .SetUpdate(true)
                .SetTarget(this);
            if (!opening)
                _tween.OnComplete(Closed);
        }

        private void Closed()
        {
            gameObject.SetActive(false);
            var closed = _closed;
            _closed = null;
            closed?.Invoke();
        }

        private void SetDim(float alpha)
        {
            var color = _dim.color;
            color.a = alpha;
            _dim.color = color;
        }

        private void ShowFace(CardFace face)
        {
            _frame.color = face.FrameColor;
            SetImage(_rarityBadge, face.RarityIcon, face.RarityColor, keepWithoutSprite: true);
            _strip.color = face.StripColor;
            var ink = _skin.InkOn(face.StripColor);
            SetImage(_stripIcon, face.ElementIcon, ink, keepWithoutSprite: false);

            var hasArt = face.Art != null;
            _art.gameObject.SetActive(hasArt);
            _artPlaceholder.gameObject.SetActive(!hasArt);
            if (hasArt)
            {
                SetImage(_art, face.Art, Color.white, keepWithoutSprite: true);
                _artIcon.gameObject.SetActive(false);
            }
            else
            {
                var shade = face.FrameColor * _skin.PlaceholderBrightness;
                shade.a = 1f;
                _artPlaceholder.color = shade;
                SetImage(_artIcon, face.ElementIcon, face.FrameColor, keepWithoutSprite: false);
            }

            _basePower.SetActive(face.Power.HasValue);
            if (face.Power.HasValue)
                _basePowerText.text = face.Power.Value.ToString();

            _title.text = face.Title;
            _stats.text = face.Stats;
            _stats.color = ink;
            _body.text = face.Body;
            _rarity.text = face.Rarity;
        }

        private void OnDestroy() => _tween?.Kill();

        private static void SetImage(Image image, Sprite sprite, Color color, bool keepWithoutSprite)
        {
            image.gameObject.SetActive(sprite != null || keepWithoutSprite);
            image.sprite = sprite;
            image.color = color;
        }
    }
}
