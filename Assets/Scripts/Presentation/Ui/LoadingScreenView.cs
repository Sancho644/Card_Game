using System.Collections;
using Game.Data.Presentation;
using Game.Presentation.Menu;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>Persistent overlay, above both scenes while they are replaced.</summary>
    public sealed class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private UnityEngine.UI.Image _background;
        [SerializeField] private UnityEngine.UI.Image _seal;
        [SerializeField] private RectTransform _sealGroup;
        [SerializeField] private LoadingRingGraphic _ring;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _percentage;
        [SerializeField] private TMP_Text _tip;

        private LoadingScreenSettings _settings;
        private UiTexts _texts;
        private CollectionUi _ui;
        private float _progress;
        private float _openedAt;
        private float _nextTipAt;
        private int _nextTip;
        private int _shownPercent = -1;

        public bool IsVisible => gameObject.activeSelf && _group.alpha > 0;
        public bool ShowOnTransitions => _settings.ShowOnTransitions;
        public float Progress => _progress;

        public void Initialize(LoadingScreenSettings settings, UiTexts texts)
        {
            _settings = settings;
            _texts = texts;
            _ui = new CollectionUi(_title.font, settings.Style);
            _ui.Plaque(_tip.transform.parent, "TipFrame", 0, 0, 1, 1).SetAsFirstSibling();
            _background.sprite = settings.Background;
            _background.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio = settings.Background.rect.width / settings.Background.rect.height;
            _seal.sprite = settings.Seal;
            _ring.color = settings.Accent;
            _title.text = texts.Get(UiTextKeys.MenuTitle);
            _label.text = texts.Get(UiTextKeys.LoadingTitle);
            HideImmediately();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _group.alpha = 1;
            _group.blocksRaycasts = true;
            _progress = 0;
            _shownPercent = -1;
            _openedAt = Time.unscaledTime;
            _ring.Show(0, 0);
            _sealGroup.localScale = Vector3.one;
            NextTip();
            SetProgress(0);
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        public void SetProgress(float value)
        {
            _progress = Mathf.Max(_progress, Mathf.Clamp01(value));
            var percent = Mathf.FloorToInt(_progress * 100f);
            if (percent == _shownPercent) return;
            _shownPercent = percent;
            _percentage.text = _texts.Format(UiTextKeys.LoadingPercent, percent);
        }

        public IEnumerator FadeOut()
        {
            var elapsed = 0f;
            while (elapsed < _settings.FadeOutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                _group.alpha = 1 - Mathf.Clamp01(elapsed / _settings.FadeOutSeconds);
                yield return null;
            }
            HideImmediately();
        }

        public void HideImmediately()
        {
            _group.alpha = 0;
            _group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_settings == null) return;
            var elapsed = Time.unscaledTime - _openedAt;
            _ring.Show(_progress, elapsed * _settings.CometTurnsPerSecond);
            _sealGroup.localScale = Vector3.one * (1 + Mathf.Sin(elapsed * Mathf.PI * 2 / _settings.PulseSeconds) * _settings.SealPulse);
            if (Time.unscaledTime >= _nextTipAt) NextTip();
        }

        private void NextTip()
        {
            _tip.text = _settings.Tips.Count == 0 ? "" : _texts.Get(_settings.Tips[_nextTip++ % _settings.Tips.Count]);
            _nextTipAt = Time.unscaledTime + _settings.TipSeconds;
        }

        private void OnDestroy() => _ui?.Dispose();
    }
}
