using System;
using System.Linq;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.App;
using Game.Presentation.Cards;
using Game.Presentation.Ui;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Menu
{
    /// <summary>Two duel panels, real selected-deck preview and an unknown opponent.</summary>
    public sealed class SearchingLayout : IDisposable
    {
        public readonly TMP_Text Title, Hint, Timer;
        public readonly UnityEngine.UI.Button Cancel;

        private readonly CollectionUi _ui;
        private readonly AppServices _app;
        private readonly SearchingSettings _settings;
        private readonly CardFaceBuilder _faces;
        private readonly CardSkin _skin;
        private readonly CreatureCardFaceView _hero;
        private readonly TMP_Text _deckName;
        private readonly RectTransform _opponent;
        private readonly SearchPulseGraphic _pulse;
        private string _previewId;

        public SearchingLayout(Transform parent, TMP_FontAsset font, CollectionSettings style, SearchingSettings settings, AppServices app)
        {
            _ui = new CollectionUi(font, style); _app = app; _settings = settings;
            _faces = new CardFaceBuilder(app.Database, style.CardVisuals, app.Texts);
            _skin = new CardSkin(style.CardVisuals);
            var root = _ui.Panel(parent, "DuelSearch", 0, 0, 1, 1, style.Background);
            var backdrop = _ui.Rect(root, "Background", 0, 0, 1, 1).gameObject.AddComponent<UnityEngine.UI.Image>();
            backdrop.sprite = settings.Background; backdrop.raycastTarget = false;
            var cover = backdrop.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            cover.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
            cover.aspectRatio = settings.Background.rect.width / settings.Background.rect.height;
            var safe = _ui.Rect(root, "SafeArea", 0, 0, 1, 1);
            safe.gameObject.AddComponent<SafeAreaFitter>();
            Title = Text(safe, "Title", "", .20f, .89f, .60f, .08f, 58);
            var rule = _ui.Panel(safe, "TitleRule", .21f, .875f, .58f, .0015f, new Color(.7f, .47f, .22f));
            rule.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var mode = _ui.Plaque(safe, "Mode", .415f, .79f, .17f, .06f);
            Text(mode, "Label", app.Texts.Get(UiTextKeys.SearchModeAi), .03f, .08f, .94f, .84f, 27);
            var own = Panel(safe, "OwnPanel", .13f);
            var other = Panel(safe, "OpponentPanel", .57f);
            Header(own, app.Texts.Get(UiTextKeys.PlayerYou));
            Header(other, app.Texts.Get(UiTextKeys.PlayerOpponent));
            var heroBounds = CardBounds(own, "SelectedCreature");
            _hero = CreatureCardFaceView.Create(heroBounds);
            var deckPlate = _ui.Plaque(own, "DeckNamePlate", .025f, .015f, .95f, .105f);
            _deckName = Text(deckPlate, "DeckName", "", .03f, .09f, .94f, .82f, 24);
            var unknownBounds = CardBounds(other, "UnknownBounds");
            _opponent = _ui.Rect(unknownBounds, "UnknownOpponent", 0, 0, 1, 1);
            var back = _opponent.gameObject.AddComponent<UnityEngine.UI.Image>();
            back.sprite = settings.UnknownCard; back.preserveAspect = true; back.raycastTarget = false;
            var question = Text(_opponent, "Question", app.Texts.Get(UiTextKeys.SearchUnknown), .2f, .32f, .6f, .40f, 148);
            question.fontStyle = FontStyles.Bold;
            var crossed = _ui.Rect(safe, "CrossedCards", .445f, .525f, .11f, .14f);
            foreach (var side in new[] { -1, 1 })
            {
                var rect = _ui.Rect(crossed, "Card", side < 0 ? .17f : .49f, .05f, .34f, .90f);
                var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
                image.sprite = settings.UnknownCard; image.preserveAspect = true; image.raycastTarget = false;
                rect.localRotation = Quaternion.Euler(0, 0, side * 18f);
            }
            Text(safe, "Versus", app.Texts.Get(UiTextKeys.SearchVersus), .40f, .463f, .20f, .055f, 38);
            _pulse = _ui.Rect(safe, "SearchLights", .425f, .39f, .15f, .065f).gameObject.AddComponent<SearchPulseGraphic>();
            _pulse.color = settings.Accent; _pulse.raycastTarget = false;
            Hint = Text(safe, "Status", "", .25f, .22f, .50f, .065f, 40);
            Timer = Text(safe, "Elapsed", "", .35f, .17f, .30f, .04f, 26);
            Cancel = _ui.Button(safe, "CancelSearch", "", .345f, .055f, .31f, .075f, () => { });
            _ui.Decorate(Cancel, 32);
        }

        public void RefreshDeck()
        {
            var deck = _app.Collection.Selected;
            var name = string.IsNullOrEmpty(deck.Name) ? _app.Texts.Get("collection.emptyPreset") : deck.Name;
            _deckName.text = _app.Texts.Format(UiTextKeys.SearchDeck, name, deck.Cards.Count);
            var creature = deck.Cards.Select(_app.Database.Find).OfType<CreatureDef>().OrderBy(c => c.Rarity).FirstOrDefault();
            _hero.gameObject.SetActive(creature != null);
            if (creature == null || creature.Id == _previewId) return;
            _previewId = creature.Id;
            _hero.Bind(_faces.Build(creature.Id), _skin, _ui.Font);
        }

        public void Animate(float seconds)
        {
            var wave = Mathf.Sin(seconds * Mathf.PI * 2 / _settings.FloatSeconds);
            _opponent.anchoredPosition = new Vector2(0, wave * _settings.FloatPixels);
            _opponent.localRotation = Quaternion.Euler(0, 0, wave * _settings.SwayDegrees);
            _pulse.Show(seconds / _settings.PulseSeconds);
        }

        public void Dispose() => _ui.Dispose();

        private RectTransform Panel(Transform parent, string name, float x)
        {
            var area = _ui.Rect(parent, name + "Area", x, .29f, .30f, .51f);
            var bounds = _ui.Rect(area, "Bounds", 0, 0, 1, 1);
            var fit = bounds.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = .82f;
            var frame = _ui.Frame(bounds, name, 0, 0, 1, 1);
            frame.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            return frame;
        }

        private RectTransform CardBounds(Transform parent, string name)
        {
            var area = _ui.Rect(parent, name, .08f, .125f, .84f, .74f);
            var bounds = _ui.Rect(area, "Portrait", 0, 0, 1, 1);
            var fit = bounds.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = .70f;
            return bounds;
        }

        private void Header(Transform parent, string title)
        {
            var plate = _ui.Plaque(parent, "Header", .015f, .90f, .97f, .14f);
            Text(plate, "Label", title, .03f, .08f, .94f, .84f, 30);
        }

        private TMP_Text Text(Transform parent, string name, string value, float x, float y, float w, float h, float size)
        {
            var text = _ui.Text(parent, name, value, x, y, w, h, size);
            text.alignment = TextAlignmentOptions.Midline;
            text.enableAutoSizing = true; text.fontSizeMin = size * .60f; text.fontSizeMax = size;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Truncate;
            return text;
        }
    }
}
