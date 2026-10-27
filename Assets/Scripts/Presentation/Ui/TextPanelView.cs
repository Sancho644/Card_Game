using TMPro;
using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>A panel with a block of text: the battle panel and the selection prompt.</summary>
    public sealed class TextPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        public void Show(string text)
        {
            _text.text = text;
            gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
