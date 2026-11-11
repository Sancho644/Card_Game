using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data.Presentation;
using Game.Presentation.App;
using Game.Presentation.Cards;
using TMPro;
using UnityEngine;
namespace Game.Presentation.Menu
{
    /// <summary>Live altar menu and modal panels; deck contents come from the player's saved selection.</summary>
    public sealed class MainMenuLayout : IDisposable
    {
        public readonly TMP_Text Title;
        public readonly UnityEngine.UI.Button VersusAi, HotSeat, Collection, Difficulty, FastAnimations;
        private readonly CollectionUi _ui;
        private readonly CollectionCardView _cards;
        private readonly AppServices _app;
        private readonly CardFaceBuilder _faces;
        private readonly Action _chooseDeck;
        private readonly RectTransform _gallery, _settings, _details;
        private readonly TMP_Text _deckName, _detailText;
        private readonly RectTransform _detailCard;
        private readonly List<GameObject> _rendered = new List<GameObject>();
        private readonly List<GameObject> _detailRendered = new List<GameObject>();
        public MainMenuLayout(Transform parent, TMP_FontAsset font, CollectionSettings style, MainMenuSettings settings, AppServices app, Action chooseDeck)
        {
            _ui = new CollectionUi(font, style); _app = app; _chooseDeck = chooseDeck;
            _faces = new CardFaceBuilder(app.Database, style.CardVisuals, app.Texts);
            _cards = new CollectionCardView(_ui, style, _faces);
            var root = _ui.Panel(parent, "AltarMenu", 0, 0, 1, 1, style.Background);
            if (settings != null && settings.Background != null) {
                var image = _ui.Rect(root, "Backdrop", 0, 0, 1, 1).gameObject.AddComponent<UnityEngine.UI.Image>();
                image.sprite = settings.Background; image.raycastTarget = false;
                var fit = image.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
            }
            var title = _ui.Frame(root, "TitlePlate", .24f, .88f, .52f, .10f);
            Title = _ui.Text(title, "Title", "", .04f, .05f, .92f, .9f, 52);
            Title.alignment = TextAlignmentOptions.Midline; Title.enableAutoSizing = true; Title.fontSizeMin = 28; Title.fontSizeMax = 52;
            var deck = _ui.Button(root, "SelectedDeck", "", .25f, .775f, .50f, .075f, chooseDeck);
            _deckName = deck.GetComponentInChildren<TMP_Text>(); _deckName.fontSizeMax = 30;
            _gallery = _ui.Rect(root, "DeckCards", .13f, .52f, .74f, .24f);
            VersusAi = Button(root, "VersusAi", .29f, .36f, .42f, .115f, 42);
            _ui.Select(VersusAi, true);
            HotSeat = Button(root, "HotSeat", .29f, .255f, .42f, .08f, 27);
            Collection = Button(root, "Collection", .29f, .15f, .42f, .08f, 32);
            var settingsButton = _ui.Button(root, "Settings", app.Texts.Get("menu.settings"), .40f, .045f, .20f, .065f, OpenSettings);
            settingsButton.GetComponentInChildren<TMP_Text>().fontSizeMax = 26;
            _settings = _ui.Panel(root, "SettingsModal", 0, 0, 1, 1, new Color(0, 0, 0, .82f));
            var panel = _ui.Frame(_settings, "SettingsPanel", .25f, .24f, .50f, .52f);
            var caption = _ui.Text(panel, "Title", app.Texts.Get("menu.settings"), .08f, .77f, .84f, .14f, 34);
            caption.alignment = TextAlignmentOptions.Midline;
            Difficulty = Button(panel, "Difficulty", .08f, .51f, .84f, .16f, 26);
            FastAnimations = Button(panel, "FastAnimations", .08f, .29f, .84f, .16f, 26);
            _ui.Button(panel, "CloseSettings", app.Texts.Get("library.close"), .30f, .07f, .40f, .13f, () => _settings.gameObject.SetActive(false));
            _settings.gameObject.SetActive(false);
            _details = _ui.Panel(root, "CardDetails", 0, 0, 1, 1, new Color(0, 0, 0, .88f));
            var detailPanel = _ui.Frame(_details, "Panel", .12f, .07f, .76f, .86f);
            _detailCard = _ui.Rect(detailPanel, "Card", .03f, .06f, .40f, .88f);
            _detailText = _ui.Text(detailPanel, "Description", "", .47f, .20f, .47f, .65f, 26);
            _detailText.enableAutoSizing = true; _detailText.fontSizeMin = 16; _detailText.fontSizeMax = 26;
            _detailText.alignment = TextAlignmentOptions.TopLeft;
            _ui.Button(detailPanel, "CloseDetails", app.Texts.Get("library.close"), .64f, .045f, .30f, .09f, () => _details.gameObject.SetActive(false));
            _details.gameObject.SetActive(false);
        }
        public void RefreshDeck()
        {
            CollectionDeckGrid.Clear(_rendered);
            var deck = _app.Collection.Selected;
            _deckName.text = (string.IsNullOrEmpty(deck.Name) ? _app.Texts.Get("collection.emptyPreset") : deck.Name) + " · " + deck.Cards.Count + " / 9";
            var cards = deck.Cards.Select(_app.Database.Find).OrderBy(c => c.Type).ThenBy(c => c.Rarity).ToList();
            for (var i = 0; i < 9; i++) {
                if (i < cards.Count) {
                    var card = cards[i];
                    var button = _cards.Create(_gallery, card, i, false, false, "", () => Inspect(card.Id), 9, 1);
                    _rendered.Add(button.transform.parent.parent.parent.gameObject);
                } else {
                    var empty = _ui.Button(_gallery, "EmptySlot" + i, "+", i / 9f + .006f, .055f, .099f, .89f, _chooseDeck);
                    _rendered.Add(empty.gameObject);
                }
            }
        }
        public void OpenSettings() { _settings.gameObject.SetActive(true); _settings.SetAsLastSibling(); }
        public void CloseModals() { _settings.gameObject.SetActive(false); _details.gameObject.SetActive(false); }
        public void Dispose() { _ui.Dispose(); }
        private UnityEngine.UI.Button Button(Transform parent, string name, float x, float y, float w, float h, float size)
        {
            var button = _ui.Button(parent, name, "", x, y, w, h, () => { });
            button.GetComponentInChildren<TMP_Text>().fontSizeMax = size; return button;
        }
        private void Inspect(string id)
        {
            CollectionDeckGrid.Clear(_detailRendered);
            var button = _cards.Create(_detailCard, _app.Database.Find(id), 0, false, false, "", () => { }, 1, 1);
            _detailRendered.Add(button.transform.parent.parent.parent.gameObject);
            var face = _faces.Build(id);
            _detailText.text = face.Title + "\n\n" + face.TypeLabel + " · " + face.Rarity + "\n\n" + face.Stats + "\n\n" + face.Body;
            _details.gameObject.SetActive(true); _details.SetAsLastSibling();
        }
    }
}
