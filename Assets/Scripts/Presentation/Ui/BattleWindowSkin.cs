using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using Game.Presentation.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>Shared battle-window artwork, responsive layout and public card faces.</summary>
    public sealed class BattleWindowSkin : IDisposable
    {
        private readonly BattleWindowSettings _settings;
        private readonly CardFaceBuilder _faces;
        private readonly CardSkin _cards;
        private readonly Sprite[] _emblems = new Sprite[9];
        public CollectionUi Ui { get; }
        public UiTexts Texts { get; }
        public BattleWindowSettings Settings => _settings;

        public BattleWindowSkin(BattleWindowSettings settings, TMP_FontAsset font, CardFaceBuilder faces, CardSkin cards, UiTexts texts)
        {
            _settings = settings; _faces = faces; _cards = cards; Texts = texts;
            Ui = new CollectionUi(font, settings.Style);
            var atlas = settings.Emblems;
            if (atlas == null) return;
            for (var i = 0; i < 9; i++)
            {
                var width = atlas.width / 3f; var height = atlas.height / 3f;
                _emblems[i] = UnityEngine.Sprite.Create(atlas, new Rect(i % 3 * width, (2 - i / 3) * height, width, height),
                    new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            }
        }

        public void Dispose()
        {
            Ui.Dispose();
            foreach (var emblem in _emblems) if (emblem != null) UnityEngine.Object.Destroy(emblem);
        }

        public Sprite Sprite(BattleEmblem emblem) => _emblems[(int)emblem];

        public RectTransform Frame(Transform parent, string name, float x, float y, float w, float h)
        {
            var frame = Ui.Rect(parent, name, x, y, w, h);
            // The shared ornamental texture has translucent areas. A separate solid layer hides the table.
            var opaque = _settings.Opaque; opaque.a = 1;
            Ui.Panel(frame, "OpaqueBacking", .004f, .008f, .992f, .984f, opaque);
            Ui.Frame(frame, "JadeArtwork", 0, 0, 1, 1);
            return frame;
        }

        public Image Icon(Transform parent, string name, BattleEmblem emblem, float x, float y, float w, float h)
        {
            var rect = Ui.Rect(parent, name, x, y, w, h);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = Sprite(emblem); image.preserveAspect = true; image.raycastTarget = false;
            return image;
        }

        public void Label(TMP_Text text, float size, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            text.font = Ui.Font; text.color = _settings.Ink; text.fontSize = size;
            text.enableAutoSizing = true; text.fontSizeMin = 18; text.fontSizeMax = size;
            text.alignment = alignment; text.richText = false; text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        public RectTransform Window(Transform root, TMP_Text title, TMP_Text body, Button primary, Button secondary,
            BattleEmblem emblem, bool compact = false, bool opaque = false)
        {
            var scrim = root.GetComponent<Image>();
            if (scrim != null)
            {
                var color = opaque ? _settings.Opaque : _settings.Scrim;
                if (opaque) color.a = 1; // Handoff privacy must survive an accidental alpha change in the asset.
                scrim.color = color; scrim.raycastTarget = true;
            }
            var panel = (RectTransform)title.transform.parent;
            var safe = Ui.Rect(root, "WindowSafeArea", 0, 0, 1, 1);
            safe.gameObject.AddComponent<SafeAreaFitter>();
            Place(panel, safe, compact ? .19f : .12f, compact ? .27f : .10f, compact ? .62f : .76f, compact ? .46f : .80f);
            var image = panel.GetComponent<Image>(); if (image != null) image.enabled = false;
            var frame = Frame(panel, "JadeFrame", 0, 0, 1, 1); frame.SetAsFirstSibling();
            Icon(panel, "WindowEmblem", emblem, .42f, .81f, .16f, .30f);
            Place(title.rectTransform, panel, .065f, .69f, .87f, .13f);
            Label(title, _settings.TitleSize); title.color = _settings.Gold;
            if (body != null) { Place(body.rectTransform, panel, .08f, .28f, .84f, .36f); Label(body, _settings.BodySize); }
            Place((RectTransform)primary.transform, panel, secondary == null ? .29f : .08f, .06f, secondary == null ? .42f : .40f, .14f);
            Ui.Decorate(primary, _settings.BodySize);
            if (secondary != null)
            {
                Place((RectTransform)secondary.transform, panel, .52f, .06f, .40f, .14f);
                Ui.Decorate(secondary, _settings.BodySize);
            }
            var content = Ui.Rect(panel, "WindowContent", .055f, .235f, .89f, .445f);
            content.gameObject.SetActive(false);
            return content;
        }

        public static void Place(RectTransform rect, Transform parent, float x, float y, float w, float h)
        {
            rect.SetParent(parent, false); rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(x + w, y + h);
            rect.pivot = new Vector2(.5f, .5f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
        }

        public void SingleButton(Button primary, Button secondary)
        {
            var two = secondary != null && secondary.gameObject.activeSelf;
            Place((RectTransform)primary.transform, primary.transform.parent, two ? .08f : .29f, .06f, two ? .40f : .42f, .14f);
        }

        public void Clear(RectTransform content)
        {
            foreach (Transform child in content) { child.gameObject.SetActive(false); UnityEngine.Object.Destroy(child.gameObject); }
        }

        public Button Card(Transform parent, string cardId, string fallback, float x, float y, float w, float h, Action clicked = null)
        {
            var cell = Ui.Rect(parent, "CardChoice", x, y, w, h);
            var rect = Ui.Panel(cell, "Card", 0, 0, 1, 1, Color.clear);
            var fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = .70f;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            var colors = button.colors; colors.highlightedColor = new Color(.65f, 1, .92f); colors.pressedColor = new Color(.4f, .8f, .72f); button.colors = colors;
            if (clicked != null) button.onClick.AddListener(() => clicked()); else button.interactable = false;
            if (cardId == null)
            {
                Ui.Frame(rect, "UnknownChoice", 0, 0, 1, 1).GetComponent<Image>().raycastTarget = false;
                var label = Ui.Text(rect, "Label", fallback, .08f, .1f, .84f, .8f);
                Label(label, _settings.BodySize); return button;
            }
            var face = _faces.Build(cardId);
            // Entire card image is the target graphic, so selection tint is visible without a solid overlay.
            if (face.Ability != null) AbilityCardFaceView.Create(rect).Bind(face, _cards, Ui.Font);
            else if (face.IsField) FieldCardFaceView.Create(rect).Bind(face, _cards, Ui.Font);
            else CreatureCardFaceView.Create(rect).Bind(face, _cards, Ui.Font);
            var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(.15f, .75f, .65f, .6f); outline.effectDistance = new Vector2(2, -2);
            return button;
        }

        public void Gallery(RectTransform content, IReadOnlyList<string> ids, IReadOnlyList<string> labels, Action<int> chosen)
        {
            Clear(content); content.gameObject.SetActive(true);
            // At most three columns; larger public piles scroll vertically instead of shrinking the cards.
            if (ids.Count <= 3)
            {
                var columns = Math.Max(2, ids.Count); var width = 1f / columns;
                for (var i = 0; i < ids.Count; i++)
                { var index = i; Card(content, ids[i], labels[i], (1 - width * ids.Count) / 2 + i * width + .025f, 0, width - .05f, 1, () => chosen(index)); }
                return;
            }
            ScrollRect scroll;
            var list = Ui.Scroll(content, "CardChoices", 0, 0, 1, 1, out scroll);
            for (var start = 0; start < ids.Count; start += 3)
            {
                var row = Ui.Rect(list, "Row", 0, 0, 1, 1); row.gameObject.AddComponent<LayoutElement>().preferredHeight = 320;
                for (var i = start; i < Math.Min(start + 3, ids.Count); i++)
                { var index = i; Card(row, ids[i], labels[i], (i - start) / 3f + .015f, 0, .303f, 1, () => chosen(index)); }
            }
        }

        public void Scout(RectTransform content, IReadOnlyList<string> abilities, string field)
        {
            Clear(content); content.gameObject.SetActive(true);
            var heading = Ui.Text(content, "AbilitiesLabel", Texts.Get("window.scoutAbilities"), .01f, .88f, .72f, .12f);
            Label(heading, _settings.BodySize, TextAlignmentOptions.Left);
            var fieldHeading = Ui.Text(content, "FieldLabel", Texts.Get("window.scoutField"), .77f, .88f, .23f, .12f);
            Label(fieldHeading, _settings.BodySize);
            for (var i = 0; i < abilities.Count; i++)
                Card(content, abilities[i], "", i * .245f, 0, .23f, .84f);
            if (abilities.Count == 0) Label(Ui.Text(content, "Empty", Texts.Get("window.noAbilities"), 0, .3f, .7f, .2f), _settings.BodySize);
            if (field != null) Card(content, field, "", .77f, 0, .23f, .84f);
            else Label(Ui.Text(content, "NoField", Texts.Get("window.noField"), .77f, .3f, .23f, .2f), _settings.BodySize);
        }

        public void Breakdown(RectTransform content, string hostCard, string guestCard, string hostName, string guestName,
            PowerBreakdown host, PowerBreakdown guest, string details)
        {
            Clear(content); content.gameObject.SetActive(true);
            Side(content, hostCard, hostName, host, 0);
            Side(content, guestCard, guestName, guest, .52f);
            ScrollRect scroll;
            var list = Ui.Scroll(content, "BreakdownDetails", 0, 0, 1, .33f, out scroll);
            var text = Ui.Text(list, "Details", details, 0, 0, 1, 1, _settings.BodySize);
            Label(text, _settings.BodySize, TextAlignmentOptions.TopLeft); text.enableAutoSizing = false;
            text.gameObject.AddComponent<LayoutElement>();
        }

        private void Side(Transform parent, string card, string name, PowerBreakdown power, float x)
        {
            var side = Frame(parent, "PowerSide", x, .37f, .48f, .63f);
            Card(side, card, "", .025f, .08f, .30f, .80f);
            var title = Ui.Text(side, "CreatureName", name + "\n" + _faces.NameOf(card), .35f, .72f, .61f, .24f);
            Label(title, _settings.BodySize, TextAlignmentOptions.Left);
            var values = new[] { Texts.Format(UiTextKeys.BreakdownBase, power.BasePower),
                Texts.Format(UiTextKeys.BreakdownField, Signed(power.FieldValue)),
                Texts.Format("window.abilitiesPower", Signed(power.Abilities.Sum(a => a.Amount))),
                Texts.Format(UiTextKeys.BreakdownTotal, power.FinalPower) };
            for (var i = 0; i < values.Length; i++)
            {
                var row = Ui.Text(side, "PowerRow", values[i], .35f, .53f - i * .15f, .61f, .15f);
                Label(row, i == 3 ? _settings.BodySize + 3 : _settings.BodySize - 2, TextAlignmentOptions.Left);
                if (i == 3) row.color = _settings.Gold;
            }
        }

        private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();
    }
}
