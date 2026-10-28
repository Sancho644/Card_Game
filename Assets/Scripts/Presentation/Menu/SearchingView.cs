using System;
using DG.Tweening;
using Game.Presentation.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Menu
{
    /// <summary>"Searching for an opponent": a turning spinner, a pulsing hint and a cancel button.</summary>
    public sealed class SearchingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private RectTransform _spinner;
        [SerializeField] private Button _cancel;
        [SerializeField, Min(0.1f)] private float _turnSeconds = 1.2f;

        private Sequence _animation;

        public void Show(string title, string hint, string cancelLabel, Action cancel)
        {
            _title.text = title;
            _hint.text = hint;
            UiButtons.SetLabel(_cancel, cancelLabel);
            UiButtons.Bind(_cancel, cancel);
            gameObject.SetActive(true);
            Animate();
        }

        public void Hide()
        {
            Stop();
            gameObject.SetActive(false);
        }

        private void OnDestroy() => Stop();

        private void Animate()
        {
            Stop();
            _spinner.localRotation = Quaternion.identity;
            _hint.alpha = 1f;
            _animation = DOTween.Sequence()
                .Join(_spinner.DOLocalRotate(new Vector3(0, 0, -360), _turnSeconds, RotateMode.FastBeyond360).SetEase(Ease.Linear))
                .Join(_hint.DOFade(0.35f, _turnSeconds / 2).SetLoops(2, LoopType.Yoyo))
                .SetLoops(-1)
                .SetUpdate(true);
        }

        private void Stop()
        {
            if (_animation != null && _animation.IsActive())
                _animation.Kill();
            _animation = null;
        }
    }
}
