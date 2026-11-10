using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.App;
using Game.Presentation.Cards;
using Game.Services.Collection;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Menu
{
    /// <summary>Browse owned cards, edit three saved drafts and select a valid deck for play.</summary>
    public sealed class CollectionScreen : IDisposable
    {
        private const int PageSize = 6;
        private readonly AppServices _app;
        private readonly CollectionSettings _style;
        private readonly CollectionUi _ui;
        private readonly CollectionCardView _cardView;
        private readonly UnityEngine.UI.Button _collectionTab;
        private readonly UnityEngine.UI.Button _deckTab;
        private readonly UnityEngine.UI.Button _allTypes;
        private readonly Dictionary<CardType, UnityEngine.UI.Button> _typeTabs = new Dictionary<CardType, UnityEngine.UI.Button>();
        private readonly TMP_Text _emptyMessage;
        private readonly UnityEngine.UI.Button _remove;
        private readonly UnityEngine.UI.Button _delete;
        private readonly RectTransform _root;
        private readonly RectTransform _grid;
        private readonly RectTransform _deck;
        private readonly TMP_InputField _name;
        private readonly TMP_Text _status;
        private readonly TMP_Text _deckTitle;
        private readonly TMP_Text _pageLabel;
        private readonly TMP_Text _filterLabel;
        private readonly TMP_Text _detailName;
        private readonly TMP_Text _detailText;
        private readonly UnityEngine.UI.Image _art;
        private readonly UnityEngine.UI.Button _add;
        private readonly UnityEngine.UI.Button _use;
        private readonly UnityEngine.UI.Button _play;
        private readonly UnityEngine.UI.Button _previous;
        private readonly UnityEngine.UI.Button _next;
        private readonly UnityEngine.UI.Button[] _presets;
        private readonly List<GameObject> _cards = new List<GameObject>();
        private readonly List<GameObject> _slots = new List<GameObject>();
        private readonly Action _closed;
        private readonly Action _playAi;
        private bool _browseCollection;
        private int _editing;
        private int _page;
        private CardType? _type;
        private Rarity? _rarity;
        private CardDef _focused;

        public CollectionScreen(Transform parent, TMP_FontAsset font, CollectionSettings style, AppServices app, Action closed, Action playAi)
        {
            _app = app; _style = style; _closed = closed; _playAi = playAi;
            _ui = new CollectionUi(font, style);
            _cardView = new CollectionCardView(_ui, style, new CardFaceBuilder(app.Database, style.CardVisuals, app.Texts));
            _root = _ui.Panel(parent, "Collection", 0, 0, 1, 1, style.Background);
            _ui.Text(_root, "Title", T("title"), .025f, .92f, .65f, .06f, style.TitleSize);
            _ui.Button(_root, "Close", T("back"), .83f, .925f, .145f, .055f, Close);
            _presets = new UnityEngine.UI.Button[PlayerCollection.PresetCount];
            for (var i = 0; i < _presets.Length; i++)
            {
                var index = i;
                _presets[i] = _ui.Button(_root, "Preset" + i, "", .025f + i * .19f, .84f, .18f, .06f, () => EditPreset(index));
            }
            _name = _ui.Input(_root, "DeckName", .62f, .84f, .25f, .06f, T("name"), Rename);
            _delete = _ui.Button(_root, "DeletePreset", T("deletePreset"), .88f, .84f, .095f, .06f, DeletePreset);
            _delete.GetComponentInChildren<TMP_Text>().fontSize = style.SmallSize;
            _name.characterLimit = PlayerCollection.NameLimit;
            _deckTitle = _ui.Text(_root, "DeckTitle", T("deck"), .025f, .77f, .25f, .05f);
            _filterLabel = _ui.Text(_root, "Filter", "", .30f, .77f, .18f, .05f, style.SmallSize);
            _deckTab = _ui.Button(_root, "DeckCards", T("deckCards"), .49f, .775f, .10f, .045f, () => Browse(false));
            _collectionTab = _ui.Button(_root, "CollectionCards", T("browseCollection"), .60f, .775f, .10f, .045f, () => Browse(true));
            _deckTab.GetComponentInChildren<TMP_Text>().fontSize = style.SmallSize;
            _collectionTab.GetComponentInChildren<TMP_Text>().fontSize = style.SmallSize;
            var typeFilters = _ui.Rect(_root, "TypeFilters", .30f, .718f, .40f, .044f);
            _allTypes = TypeButton(typeFilters, "FilterAll", T("filter.All"), 0, () => FilterType(null));
            var typeIndex = 1;
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
            {
                var selectedType = type;
                _typeTabs.Add(type, TypeButton(typeFilters, "Filter" + type, T("filter." + type), typeIndex++, () => FilterType(selectedType)));
            }
            _deck = _ui.Rect(_root, "DeckSlots", .025f, .17f, .255f, .59f);
            _grid = _ui.Rect(_root, "CardGrid", .30f, .22f, .40f, .485f);
            _emptyMessage = _ui.Text(_grid, "EmptyDeck", T("emptyDeck"), .05f, .2f, .9f, .6f);
            _emptyMessage.alignment = TextAlignmentOptions.Center;
            _previous = _ui.Button(_root, "Previous", T("previous"), .30f, .16f, .1f, .045f, () => { _page--; RefreshCards(); });
            _pageLabel = _ui.Text(_root, "Page", "", .41f, .16f, .18f, .045f, style.SmallSize);
            _pageLabel.alignment = TextAlignmentOptions.Center;
            _next = _ui.Button(_root, "Next", T("next"), .60f, .16f, .1f, .045f, () => { _page++; RefreshCards(); });
            var details = _ui.Panel(_root, "Details", .72f, .17f, .255f, .64f, style.Panel);
            _detailName = _ui.Text(details, "CardName", T("inspect"), .06f, .82f, .88f, .16f);
            var imageRect = _ui.Rect(details, "Art", .1f, .55f, .8f, .26f);
            _art = imageRect.gameObject.AddComponent<UnityEngine.UI.Image>();
            _art.preserveAspect = true; _art.raycastTarget = false;
            _detailText = _ui.Text(details, "CardDescription", "", .06f, .16f, .88f, .38f, style.SmallSize);
            _detailText.alignment = TextAlignmentOptions.TopLeft;
            _detailText.enableAutoSizing = true;
            _detailText.fontSizeMin = 15; _detailText.fontSizeMax = style.SmallSize;
            _add = _ui.Button(details, "AddToDeck", T("replace"), .06f, .025f, .88f, .08f, Add);
            _remove = _ui.Button(details, "RemoveFromDeck", T("remove"), .06f, .115f, .88f, .08f, Remove);
            _status = _ui.Text(_root, "Status", "", .025f, .015f, .54f, .12f, style.SmallSize);
            _use = _ui.Button(_root, "UseDeck", T("use"), .59f, .045f, .18f, .075f, Use);
            _play = _ui.Button(_root, "PlayAi", T("play"), .79f, .045f, .185f, .075f, Play);
            _root.gameObject.SetActive(false);
        }

        public void Show()
        {
            _root.gameObject.SetActive(true);
            EditPreset(_app.Collection.SelectedIndex);
        }
        public void Dispose() { if (_root != null) UnityEngine.Object.Destroy(_root.gameObject); }
        private string T(string key) => _app.Texts.Get("collection." + key);
        private string TypeName(CardType type) => T("type." + type);
        private string RarityName(Rarity rarity) => _app.Texts.Get(UiTextKeys.RarityPrefix + rarity);
        private void Close() { Rename(_name.text); _root.gameObject.SetActive(false); _closed(); }
        private void EditPreset(int index)
        {
            _browseCollection = false;
            _editing = index; _page = 0; _focused = null; _type = null; _rarity = null;
            _name.SetTextWithoutNotify(_app.Collection.Get(index).Name);
            Refresh();
        }
        private void Rename(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) { _name.SetTextWithoutNotify(_app.Collection.Get(_editing).Name); return; }
            _app.Collection.Rename(_editing, value);
            _name.SetTextWithoutNotify(_app.Collection.Get(_editing).Name);
            RefreshStatus();
        }
        private void Refresh()
        {
            RefreshDeck(); RefreshCards(); RefreshDetails(); RefreshStatus();
        }
        private void RefreshStatus()
        {
            for (var i = 0; i < _presets.Length; i++)
            {
                _presets[i].GetComponentInChildren<TMP_Text>().text = (i + 1) + ". " + (string.IsNullOrEmpty(_app.Collection.Get(i).Name) ? T("emptyPreset") : _app.Collection.Get(i).Name);
                _presets[i].GetComponent<UnityEngine.UI.Image>().color = i == _editing ? _style.Selected : _style.Button;
            }
            _delete.interactable = _app.Collection.Get(_editing).Cards.Count > 0 || !string.IsNullOrEmpty(_app.Collection.Get(_editing).Name);
            _deckTitle.text = _app.Texts.Format("collection.deckCount", _app.Collection.Get(_editing).Cards.Count);
            var result = _app.Collection.Validate(_editing);
            _use.interactable = _play.interactable = result.IsValid;
            _status.text = result.IsValid ? _app.Texts.Format("collection.saved", (string.IsNullOrEmpty(_app.Collection.Selected.Name) ? T("emptyPreset") : _app.Collection.Selected.Name)) : string.Join("\n", result.Errors.Select(ErrorText).Distinct());
        }
        private string ErrorText(DeckError error)
        {
            if (error.Code == DeckErrorCode.SignatureCreatureMissing)
            {
                var ability = _app.Catalog.Get(error.CardId) as AbilityCard;
                return _app.Texts.Format("collection.signature", _app.Database.Find(error.CardId).DisplayName, _app.Database.Find(ability.BoundCreatureId).DisplayName);
            }
            return T("invalid");
        }
        private void RefreshDeck()
        {
            Clear(_slots);
            var preset = _app.Collection.Get(_editing);
            var index = 0;
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
            foreach (var rarity in _app.Rules.Deck.RequiredRarities(type))
            {
                var slotType = type; var slotRarity = rarity;
                var card = preset.Cards.Select(_app.Database.Find).FirstOrDefault(c => c.Type == type && c.Rarity == rarity);
                var label = TypeName(type) + " · " + RarityName(rarity) + "\n" + (card?.DisplayName ?? T("empty"));
                var button = _ui.Button(_deck, "Slot" + index, label, 0, 1 - (index + 1) / 9f, 1, .102f, () =>
                {
                    _browseCollection = true;
                    _type = slotType; _rarity = slotRarity; _page = 0; _focused = card;
                    RefreshCards(); RefreshDetails();
                });
                button.GetComponentInChildren<TMP_Text>().fontSize = _style.SmallSize;
                _slots.Add(button.gameObject); index++;
            }
        }
        private void Browse(bool collection)
        {
            _browseCollection = collection; _type = null; _rarity = null; _page = 0;
            _focused = null; RefreshCards(); RefreshDetails();
        }
        private UnityEngine.UI.Button TypeButton(Transform parent, string name, string label, int index, Action clicked)
        {
            var button = _ui.Button(parent, name, label, index * .25f, 0, .235f, 1, clicked);
            var text = button.GetComponentInChildren<TMP_Text>();
            text.enableAutoSizing = true; text.fontSizeMin = _style.SmallSize * .8f; text.fontSizeMax = _style.SmallSize;
            return button;
        }
        private void FilterType(CardType? type)
        {
            _type = type; _rarity = null; _page = 0; _focused = null;
            RefreshCards(); RefreshDetails();
        }
        private void RefreshCards()
        {
            Clear(_cards);
            var source = _browseCollection ? _app.Collection.OwnedCards : _app.Collection.Get(_editing).Cards;
            var list = source.Select(_app.Database.Find)
                .Where(c => (!_type.HasValue || c.Type == _type) && (!_rarity.HasValue || c.Rarity == _rarity))
                .OrderBy(c => c.Type).ThenBy(c => c.Rarity).ThenBy(c => c.DisplayName).ToList();
            _emptyMessage.gameObject.SetActive(list.Count == 0);
            _emptyMessage.text = T(_type.HasValue ? "noMatchingCards" : "emptyDeck");
            _deckTab.GetComponent<UnityEngine.UI.Image>().color = _browseCollection ? _style.Button : _style.Selected;
            _collectionTab.GetComponent<UnityEngine.UI.Image>().color = _browseCollection ? _style.Selected : _style.Button;
            _allTypes.GetComponent<UnityEngine.UI.Image>().color = !_type.HasValue ? _style.Selected : _style.Button;
            foreach (var tab in _typeTabs)
                tab.Value.GetComponent<UnityEngine.UI.Image>().color = _type == tab.Key ? _style.Selected : _style.Button;
            var pages = Math.Max(1, (list.Count + PageSize - 1) / PageSize);
            _page = Mathf.Clamp(_page, 0, pages - 1);
            _pageLabel.text = _app.Texts.Format("collection.page", _page + 1, pages, list.Count);
            _filterLabel.text = _type.HasValue
                ? T("filter." + _type.Value) + (_rarity.HasValue ? " · " + RarityName(_rarity.Value) : "")
                : T(_browseCollection ? "all" : "deckCards");
            _previous.interactable = _page > 0; _next.interactable = _page + 1 < pages;
            var selected = _app.Collection.Get(_editing).Cards;
            var visible = list.Skip(_page * PageSize).Take(PageSize).ToList();
            for (var i = 0; i < visible.Count; i++)
            {
                var card = visible[i];
                var button = _cardView.Create(_grid, card, i, selected.Contains(card.Id), _focused == card,
                    T("inDeck"), () => { _focused = card; RefreshDetails(); RefreshCards(); });
                _cards.Add(button.transform.parent.parent.parent.gameObject);
            }
        }
        private void RefreshDetails()
        {
            _remove.interactable = _focused != null && _app.Collection.Get(_editing).Cards.Contains(_focused.Id);
            _add.interactable = _focused != null && !_app.Collection.Get(_editing).Cards.Contains(_focused.Id);
            _detailName.text = _focused == null ? T("inspect") : _focused.DisplayName;
            _art.sprite = _focused?.Art;
            _art.enabled = _art.sprite != null;
            _detailText.rectTransform.anchorMin = new Vector2(.06f, .22f);
            _detailText.rectTransform.anchorMax = new Vector2(.94f, _art.enabled ? .54f : .81f);
            _detailText.rectTransform.offsetMin = _detailText.rectTransform.offsetMax = Vector2.zero;
            _detailText.text = _focused == null ? T("hint") : TypeName(_focused.Type) + " · " + RarityName(_focused.Rarity) + "\n\n" + Stats(_focused) + _focused.Description;
        }
        private string Stats(CardDef card)
        {
            if (card is CreatureDef creature)
                return _app.Texts.Get(UiTextKeys.ElementPrefix + creature.Element) + " · " + _app.Texts.Format("collection.power", creature.Power) + "\n\n";
            if (card is FieldDef field)
                return string.Join("\n", field.Modifiers.Select(m => _app.Texts.Get(UiTextKeys.ElementPrefix + m.Element) + " " + (m.Value > 0 ? "+" : "") + m.Value)) + "\n\n";
            return "";
        }
        private void Add() { if (_focused == null) return; _app.Collection.Replace(_editing, _focused.Id); Refresh(); }
        private void Remove() { if (_focused == null) return; _app.Collection.Remove(_editing, _focused.Id); Refresh(); }
        private void DeletePreset()
        {
            _app.Collection.Delete(_editing);
            _name.SetTextWithoutNotify(""); _focused = null; Refresh();
        }
        private void Use() { Rename(_name.text); if (_app.Collection.Select(_editing)) Close(); }
        private void Play() { Rename(_name.text); if (!_app.Collection.Select(_editing)) return; _root.gameObject.SetActive(false); _playAi(); }
        private static void Clear(List<GameObject> objects)
        {
            foreach (var item in objects) { item.SetActive(false); UnityEngine.Object.Destroy(item); }
            objects.Clear();
        }
    }
}
