using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>
    /// A title, a body and up to two buttons. Used for the hand-off screen, turn order, the power
    /// breakdown and the end of the match.
    /// </summary>
    public sealed class DialogView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private Button _primary;
        [SerializeField] private Button _secondary;

        public void Show(string title, string body, string primaryLabel, Action primary, string secondaryLabel = null, Action secondary = null)
        {
            _title.text = title;
            _body.text = body ?? "";
            _body.gameObject.SetActive(!string.IsNullOrEmpty(body));
            UiButtons.SetLabel(_primary, primaryLabel);
            UiButtons.Bind(_primary, primary == null ? (Action)null : () => { Hide(); primary(); });
            UiButtons.SetLabel(_secondary, secondaryLabel);
            UiButtons.Bind(_secondary, secondary == null ? (Action)null : () => { Hide(); secondary(); });
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
