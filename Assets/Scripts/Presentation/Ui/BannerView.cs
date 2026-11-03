using TMPro;
using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>A short message across the middle of the screen, e.g. that a turn was skipped and why.</summary>
    public sealed class BannerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _text;

        public void Show(string title, string text)
        {
            _title.text = title ?? "";
            _title.gameObject.SetActive(!string.IsNullOrEmpty(title));
            _text.text = text ?? "";
            gameObject.SetActive(true);
            // Outcome messages may include both a special rule and card consequences.
            var body = _text.rectTransform;
            var height = Mathf.Max(52f, _text.GetPreferredValues(_text.text, body.rect.width, 0f).y);
            body.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            var panel = (RectTransform)transform;
            var panelHeight = Mathf.Max(170f, height + 100f);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            _title.rectTransform.anchoredPosition = new Vector2(0f, panelHeight / 2f - 42f);
            body.anchoredPosition = new Vector2(0f, -22f);
        }

        public void Hide()
        {
            if (this != null)
                gameObject.SetActive(false);
        }
    }
}
