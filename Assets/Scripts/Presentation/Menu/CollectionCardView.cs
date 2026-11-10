using System;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Menu
{
    /// <summary>Portrait card face for the collection, using the same data and colors as table cards.</summary>
    public sealed class CollectionCardView
    {
        private readonly CollectionUi _ui;
        private readonly CollectionSettings _style;
        private readonly CardFaceBuilder _faces;
        public CollectionCardView(CollectionUi ui, CollectionSettings style, CardFaceBuilder faces)
        { _ui = ui; _style = style; _faces = faces; }

        public UnityEngine.UI.Button Create(Transform parent, CardDef card, int index, bool inDeck, bool selected, string inDeckText, Action clicked)
        {
            var face = _faces.Build(card.Id);
            var cell = _ui.Rect(parent, "CardCell", index % 3 / 3f, 1 - (index / 3 + 1) / 2f, 1f / 3, .5f);
            var padding = _ui.Rect(cell, "Padding", .05f, .055f, .90f, .89f);
            var holder = _ui.Rect(padding, "CardBounds", 0, 0, 1, 1);
            var fit = holder.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = .70f;
            var button = _ui.Button(holder, "Card_" + card.Id, "", 0, 0, 1, 1, clicked);
            button.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);
            button.GetComponent<UnityEngine.UI.Image>().color = selected ? _style.CardVisuals.Selected : face.RarityColor;
            if(face.Power.HasValue && _style.CardVisuals.CreatureStyle!=null)
            {
                var view=CreatureCardFaceView.Create(button.transform);
                view.Bind(face,new CardSkin(_style.CardVisuals),_ui.Font);
                return button;
            }
            var body = Panel(button.transform, "Face", .025f, .018f, .95f, .964f, _style.CardVisuals.CardBody);
            Panel(body, "ElementAccent", 0, .975f, 1, .025f, face.FrameColor);
            var title = Text(body, "Name", face.Title, .055f, .855f, .89f, .105f, _style.SmallSize);
            title.alignment = TextAlignmentOptions.Center;
            var art = Panel(body, "ArtWindow", .055f, .44f, .89f, .40f,
                face.StripColor * new Color(.32f, .32f, .32f, 1));
            if (face.Art != null) Icon(art, "Art", face.Art, Color.white, 0, 0, 1, 1);
            else if (face.ElementIcon != null) Icon(art, "Element", face.ElementIcon, face.StripColor, .20f, .16f, .60f, .68f);
            else
            {
                var bounds = _ui.Rect(art, "SigilBounds", .30f, .20f, .40f, .60f);
                var sigil = Panel(bounds, "Sigil", 0, 0, 1, 1, face.RarityColor);
                var square = sigil.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                square.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
                square.aspectRatio = 1;
                sigil.localRotation = Quaternion.Euler(0, 0, 45);
                Panel(sigil, "Inset", .15f, .15f, .70f, .70f, face.RarityColor * new Color(.5f, .5f, .5f, 1));
            }
            var strip = Panel(body, "StatsStrip", .055f, .35f, .89f, .085f, face.StripColor);
            var stats = Text(strip, "Stats", face.Stats, .025f, .04f, .95f, .92f, _style.SmallSize * .8f);
            stats.color = _style.CardVisuals.StripIconColor; stats.alignment = TextAlignmentOptions.Center;
            if (face.Power.HasValue)
            {
                var badge = Panel(art, "PowerBadge", .61f, .04f, .35f, .27f, _style.CardVisuals.PowerBadge);
                Text(badge, "Power", face.Power.Value.ToString(), .04f, .03f, .92f, .94f, _style.FontSize).alignment = TextAlignmentOptions.Center;
            }
            var rules = Text(body, "Rules", face.Body, .075f, .075f, .85f, .25f, _style.SmallSize * .82f);
            rules.alignment = TextAlignmentOptions.TopLeft;
            Text(body, "Rarity", face.Rarity, .06f, .012f, .88f, .05f, _style.SmallSize * .65f).color = face.RarityColor;
            if (inDeck)
            {
                var tag = Panel(body, "InDeck", .08f, .71f, .84f, .09f, _style.Selected);
                Text(tag, "Label", inDeckText, .02f, 0, .96f, 1, _style.SmallSize * .8f).alignment = TextAlignmentOptions.Center;
            }
            return button;
        }
        private RectTransform Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rect = _ui.Panel(parent, name, x, y, w, h, color);
            rect.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            return rect;
        }
        private TMP_Text Text(Transform parent, string name, string value, float x, float y, float w, float h, float size)
        {
            var text = _ui.Text(parent, name, value, x, y, w, h, size);
            text.enableAutoSizing = true; text.fontSizeMin = size * .75f; text.fontSizeMax = size;
            return text;
        }
        private void Icon(Transform parent, string name, Sprite sprite, Color color, float x, float y, float w, float h)
        {
            var image = _ui.Rect(parent, name, x, y, w, h).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite; image.color = color; image.preserveAspect = true; image.raycastTarget = false;
        }
    }
}
