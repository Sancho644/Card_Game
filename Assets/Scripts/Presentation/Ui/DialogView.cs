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
        private BattleWindowSkin _skin;
        private RectTransform _content;

        public void Configure(BattleWindowSkin skin, BattleEmblem emblem, bool compact = false, bool opaque = false)
        {
            _skin = skin;
            _content = skin.Window(transform, _title, _body, _primary, _secondary, emblem, compact, opaque);
        }

        public void Show(string title, string body, string primaryLabel, Action primary, string secondaryLabel = null, Action secondary = null)
        {
            if (_content != null) { _skin.Clear(_content); _content.gameObject.SetActive(false); }
            _title.text = title;
            _body.text = body ?? "";
            _body.gameObject.SetActive(!string.IsNullOrEmpty(body));
            UiButtons.SetLabel(_primary, primaryLabel);
            UiButtons.Bind(_primary, primary == null ? (Action)null : () => { Hide(); primary(); });
            UiButtons.SetLabel(_secondary, secondaryLabel);
            UiButtons.Bind(_secondary, secondary == null ? (Action)null : () => { Hide(); secondary(); });
            _skin?.SingleButton(_primary, _secondary);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void ShowScout(string title, string body, string label, Action done,
            System.Collections.Generic.IReadOnlyList<string> abilities, string field)
        {
            Show(title, body, label, done);
            if (_skin == null) return;
            _body.gameObject.SetActive(false);
            _title.transform.parent.Find("WindowEmblem").GetComponent<Image>().sprite = _skin.Sprite(BattleEmblem.Scout);
            BattleWindowSkin.Place(_content, _content.parent, .055f, .235f, .89f, .43f);
            _skin.Scout(_content, abilities, field);
        }

        public void ShowBreakdown(string title, string body, string label, Action done,
            string hostCard, string guestCard, string hostName, string guestName,
            Game.Core.Battle.PowerBreakdown host, Game.Core.Battle.PowerBreakdown guest)
        {
            Show(title, body, label, done);
            if (_skin == null) return;
            _body.gameObject.SetActive(false);
            _title.transform.parent.Find("WindowEmblem").GetComponent<Image>().sprite = _skin.Sprite(BattleEmblem.Battle);
            BattleWindowSkin.Place(_content, _content.parent, .055f, .235f, .89f, .43f);
            _skin.Breakdown(_content, hostCard, guestCard, hostName, guestName, host, guest, body);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
