using System;
using Game.Data.Presentation;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Menu
{
    /// <summary>Anchored controls using the menu's canvas and font. No additional input system.</summary>
    public sealed class CollectionUi
    {
        private readonly TMP_FontAsset _font;
        private readonly CollectionSettings _style;
        public CollectionUi(TMP_FontAsset font, CollectionSettings style) { _font = font; _style = style; }
        public RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(x + w, y + h);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
        public RectTransform Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rect = Rect(parent, name, x, y, w, h);
            rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = color;
            return rect;
        }
        public TMP_Text Text(Transform parent, string name, string value, float x, float y, float w, float h, float size = 0)
        {
            var rect = Rect(parent, name, x, y, w, h);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _font; text.fontSize = size == 0 ? _style.FontSize : size;
            text.color = _style.Text; text.text = value; text.richText = false;
            text.raycastTarget = false; text.alignment = TextAlignmentOptions.MidlineLeft;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }
        public UnityEngine.UI.Button Button(Transform parent, string name, string label, float x, float y, float w, float h, Action clicked)
        {
            var rect = Panel(parent, name, x, y, w, h, _style.Button);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            var text = Text(rect, "Label", label, .04f, .04f, .92f, .92f);
            text.alignment = TextAlignmentOptions.Center;
            button.onClick.AddListener(() => clicked());
            return button;
        }
        public TMP_InputField Input(Transform parent, string name, float x, float y, float w, float h, string placeholder, Action<string> changed)
        {
            var rect = Panel(parent, name, x, y, w, h, _style.Panel);
            var viewport = Rect(rect, "Viewport", .03f, .05f, .94f, .9f);
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var text = Text(viewport, "Text", "", 0, 0, 1, 1);
            var hint = Text(viewport, "Placeholder", placeholder, 0, 0, 1, 1);
            hint.color = _style.Muted;
            var input = rect.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = viewport; input.textComponent = (TextMeshProUGUI)text;
            input.placeholder = hint; input.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.customCaretColor = true; input.caretColor = _style.Text;
            input.onEndEdit.AddListener(value => changed(value));
            return input;
        }
    }
}
