using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>A title and a list of buttons: deck selection, a card from a pile, a signature ability.</summary>
    public sealed class OptionListView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private RectTransform _optionsRoot;
        [SerializeField] private Button _optionTemplate;
        [SerializeField] private Button _cancel;

        private readonly List<Button> _options = new List<Button>();

        public void Show(string title, IReadOnlyList<string> labels, Action<int> chosen, string cancelLabel = null, Action cancel = null)
        {
            _title.text = title;
            Clear();
            for (var i = 0; i < labels.Count; i++)
            {
                var index = i;
                var button = Instantiate(_optionTemplate, _optionsRoot);
                UiButtons.SetLabel(button, labels[i]);
                UiButtons.Bind(button, () => { Hide(); chosen(index); });
                _options.Add(button);
            }

            UiButtons.SetLabel(_cancel, cancelLabel);
            UiButtons.Bind(_cancel, cancel == null ? (Action)null : () => { Hide(); cancel(); });
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Clear();
            gameObject.SetActive(false);
        }

        private void Clear()
        {
            foreach (var button in _options)
            {
                // Destroy happens at the end of the frame; hide at once so old buttons cannot be pressed.
                button.gameObject.SetActive(false);
                Destroy(button.gameObject);
            }

            _options.Clear();
        }
    }
}
