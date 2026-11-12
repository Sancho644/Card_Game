using TMPro;
using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>A short message across the middle of the screen, e.g. that a turn was skipped and why.</summary>
    public sealed class BannerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _text;
        private BattleWindowSkin _skin;
        private UnityEngine.UI.Image _emblem;

        public void Configure(BattleWindowSkin skin)
        {
            _skin = skin;
            var panel = (RectTransform)transform;
            BattleWindowSkin.Place(panel, panel.parent, .23f, .40f, .54f, .15f);
            var image = GetComponent<UnityEngine.UI.Image>(); if (image != null) image.enabled = false;
            var frame = skin.Frame(panel, "BannerFrame", 0, 0, 1, 1); frame.SetAsFirstSibling();
            foreach (var graphic in frame.GetComponentsInChildren<UnityEngine.UI.Graphic>()) graphic.raycastTarget = false;
            _emblem = skin.Icon(panel, "BannerEmblem", BattleEmblem.Coin, .015f, .10f, .12f, .80f);
            skin.Label(_title, skin.Settings.TitleSize - 4);
            skin.Label(_text, skin.Settings.BodySize - 3);
        }

        public void Show(string title, string text)
        {
            _title.text = title ?? "";
            _title.gameObject.SetActive(!string.IsNullOrEmpty(title));
            _text.text = text ?? "";
            gameObject.SetActive(true);
            if (_skin != null)
            {
                var panelRect = (RectTransform)transform;
                panelRect.anchorMin = new Vector2(.23f, .48f); panelRect.anchorMax = new Vector2(.77f, .48f);
                panelRect.sizeDelta = new Vector2(0, string.IsNullOrEmpty(text) ? 96 : 150);
                panelRect.anchoredPosition = Vector2.zero;
                BattleWindowSkin.Place(_title.rectTransform, panelRect, .14f, string.IsNullOrEmpty(text) ? .12f : .57f, .82f, string.IsNullOrEmpty(text) ? .76f : .32f);
                BattleWindowSkin.Place(_text.rectTransform, panelRect, .14f, .1f, .82f, .44f);
                _text.overflowMode = TextOverflowModes.Ellipsis;
                _emblem.sprite = _skin.Sprite(title == _skin.Texts.Get(Game.Data.Presentation.UiTextKeys.YourTurn)
                    || title == _skin.Texts.Get(Game.Data.Presentation.UiTextKeys.OpponentTurn) ? BattleEmblem.Coin : BattleEmblem.Battle);
                _title.color = title == _skin.Texts.Get(Game.Data.Presentation.UiTextKeys.YourTurn) ? _skin.Settings.Teal : _skin.Settings.Gold;
                return;
            }
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
