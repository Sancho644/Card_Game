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
        }

        public void Hide()
        {
            if (this != null)
                gameObject.SetActive(false);
        }
    }
}
