using TMPro;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>Shared field face with live bonus values and condition text in every card surface.</summary>
    public sealed class FieldCardFaceView : CardFaceView
    {
        private CardFace _face;
        private CardSkin _skin;
        private TMP_FontAsset _font;
        private TMP_Text _conditionTitle;
        private TMP_Text _rules;
        private UnityEngine.UI.Image _stateIcon;

        public static FieldCardFaceView Create(Transform parent) => CreateFace<FieldCardFaceView>(parent, "FieldFace");

        public void Bind(CardFace face, CardSkin skin, TMP_FontAsset font)
        {
            gameObject.SetActive(true);
            if (_face == face && _skin == skin && _font == font) return;
            ResetFace(font);
            _face = face;
            _skin = skin;
            _font = font;
            var style = skin.FieldStyle;

            DrawArt(style.Art, face.Art, style.WorldSize.x / style.WorldSize.y, ElementBackdrop(face.FrameColor), .88f);
            if (face.Art == null)
                SpriteNode("ArtPlaceholder", new Rect(.34f, .58f, .32f, .22f), face.ElementIcon, Color.white);
            SpriteNode("Frame", new Rect(0, 0, 1, 1), style.Frame, Color.white, preserveAspect: false);
            Panel("InformationPlate", new Rect(.055f, .078f, .89f, .37f), new Color(.035f, .105f, .10f));
            Panel("TitlePlate", new Rect(.055f, .359f, .89f, .089f), new Color(.14f, .085f, .055f));
            Panel("TitleTrim", new Rect(.055f, .44f, .89f, .008f), new Color(.71f, .47f, .20f));
            Label("Title", style.Title, face.Title, style.Ink, 32, 54, wrap: true);
            DrawBonuses(face);

            var body = face.Body ?? "";
            var colon = body.IndexOf(':');
            var hasTitle = colon > 0 && colon < 45;
            _conditionTitle = null;
            _rules = null;
            if (hasTitle)
            {
                Panel("ConditionRule", new Rect(.085f, .237f, .83f, .003f), new Color(.45f, .36f, .23f));
                _conditionTitle = Label("ConditionTitle", style.ConditionTitle, body.Substring(0, colon),
                    new Color(1, .82f, .52f), 28, 42, wrap: true);
            }
            if (body.Length > 0)
            {
                var area = style.Rules;
                if (!hasTitle) area.yMax = style.ConditionTitle.yMax;
                _rules = Label("Rules", area, hasTitle ? body.Substring(colon + 1).TrimStart() : body, style.Ink,
                    25, 42, wrap: true, alignment: TextAlignmentOptions.MidlineLeft);
            }
            Label("Type", style.Type, face.TypeLabel, style.Ink, 23, 32);
            DrawRarity(style.Rarity, face, style.Ink);

            // Keep the status bright while the closed field itself is dimmed.
            _stateIcon = Node("FieldState", style.StateBadge).gameObject.AddComponent<UnityEngine.UI.Image>();
            _stateIcon.raycastTarget = false;
            _stateIcon.preserveAspect = true;
            _stateIcon.gameObject.SetActive(false);
        }

        private void DrawBonuses(CardFace face)
        {
            var bonuses = face.FieldBonuses;
            if (bonuses.Count == 0) return;
            var rail = Node("Bonuses", _skin.FieldStyle.BonusRail);
            var width = 1f / bonuses.Count;
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                var panel = Node("Bonus" + i, new Rect(i * width + .012f * width, 0, width * .976f, 1), rail);
                var accent = bonus.Value < 0 ? new Color(1, .62f, .53f) :
                    bonus.Value > 0 ? new Color(.67f, 1f, .82f) : _skin.FieldStyle.Ink;
                Panel("Panel", new Rect(0, 0, 1, 1), new Color(.055f, .15f, .14f), panel);
                Panel("Trim", new Rect(0, .96f, 1, .04f), accent, panel);
                SpriteNode("Icon", new Rect(.06f, .34f, .32f, .58f), bonus.Icon, Color.white, panel);
                Label("Value", new Rect(.37f, .32f, .59f, .58f), (bonus.Value > 0 ? "+" : "") + bonus.Value,
                    accent, 26, 48, panel);
                Label("Element", new Rect(.05f, .022f, .90f, .30f), bonus.Element, _skin.FieldStyle.Ink, 17, 25, panel);
            }
        }

        public void SetState(bool closed, bool locked)
        {
            if (_stateIcon == null) return;
            _stateIcon.sprite = locked ? _skin.FieldStyle.LockedIcon : closed ? _skin.FieldStyle.ClosedIcon : null;
            _stateIcon.gameObject.SetActive(_stateIcon.sprite != null);
        }

        public void SetCancelled(bool cancelled)
        {
            SetStrike(_conditionTitle, cancelled);
            SetStrike(_rules, cancelled);
        }

        private static void SetStrike(TMP_Text label, bool cancelled)
        {
            if (label == null) return;
            label.fontStyle = cancelled ? label.fontStyle | FontStyles.Strikethrough : label.fontStyle & ~FontStyles.Strikethrough;
        }
    }
}
