using System;
using Game.Data.Presentation;
using Game.Presentation.App;
using Game.Presentation.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Menu
{
    /// <summary>Selected deck versus an unknown opponent, with an elapsed timer and cancellable search.</summary>
    public sealed class SearchingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private Button _cancel;
        private SearchingLayout _layout;
        private AppServices _app;
        private float _startedAt;
        private int _shownSeconds;

        public void Initialize(SearchingSettings settings, CollectionSettings style, AppServices app)
        {
            var font = _title.font;
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            var image = GetComponent<UnityEngine.UI.Image>();
            if (image != null) image.enabled = false;
            _app = app;
            // TMP applies its initial defaults in Awake; build under an active parent before styling text.
            gameObject.SetActive(true);
            _layout = new SearchingLayout(transform, font, style, settings, app);
            _title = _layout.Title; _hint = _layout.Hint; _cancel = _layout.Cancel;
            gameObject.SetActive(false);
        }

        public void Show(string title, string hint, string cancelLabel, Action cancel)
        {
            _title.text = title;
            _hint.text = hint;
            UiButtons.SetLabel(_cancel, cancelLabel);
            UiButtons.Bind(_cancel, cancel);
            gameObject.SetActive(true);
            _layout.RefreshDeck();
            _startedAt = Time.unscaledTime;
            _shownSeconds = -1;
            transform.SetAsLastSibling();
            Update();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_layout == null) return;
            var elapsed = Mathf.Max(0, Time.unscaledTime - _startedAt);
            _layout.Animate(elapsed);
            var seconds = Mathf.FloorToInt(elapsed);
            if (seconds == _shownSeconds) return;
            _shownSeconds = seconds;
            _layout.Timer.text = _app.Texts.Format(UiTextKeys.SearchTimer, seconds / 60, seconds % 60);
        }

        private void OnDestroy() => _layout?.Dispose();
    }
}
