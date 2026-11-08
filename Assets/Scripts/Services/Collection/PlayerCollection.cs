using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Rules;

namespace Game.Services.Collection
{
    /// <summary>Three persistent local deck drafts. Match selection always goes through deck validation.</summary>
    public sealed class PlayerCollection
    {
        public const int PresetCount = 3;
        public const int NameLimit = 32;
        private readonly ICollectionStore _store;
        private readonly CardCatalog _catalog;
        private readonly DeckValidator _validator;
        private readonly DeckPreset[] _presets = new DeckPreset[PresetCount];
        private int _selected;

        public PlayerCollection(ICollectionStore store, CardCatalog catalog, GameRules rules, IReadOnlyList<DeckPreset> defaults)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _catalog = catalog;
            _validator = new DeckValidator(catalog, rules.Deck);
            if (defaults == null || defaults.Count == 0) throw new ArgumentException("Default decks required.");
            // TODO(rules): All cards are owned in the prototype until acquisition is defined (docs/decisions.md).
            OwnedCards = Array.AsReadOnly(catalog.All.Select(c => c.Id).ToArray());
            for (var i = 0; i < PresetCount; i++)
            {
                var fallback = defaults[i % defaults.Count];
                var saved = store.Read(Key(i, "cards"));
                var cards = saved == null ? fallback.Cards.ToArray() : saved.Split('\n');
                if (cards.Length == 0 || cards.Any(id => string.IsNullOrEmpty(id) || !catalog.TryGet(id, out _)))
                    cards = fallback.Cards.ToArray();
                var name = CleanName(store.Read(Key(i, "name")));
                _presets[i] = new DeckPreset(name.Length == 0 ? fallback.Name : name, cards);
            }
            if (!int.TryParse(store.Read("collection.v1.selected"), out _selected) || _selected < 0 || _selected >= PresetCount)
                _selected = 0;
        }

        public IReadOnlyList<string> OwnedCards { get; }
        public int SelectedIndex => _selected;
        public DeckPreset Selected => Get(_selected);
        public DeckPreset Get(int index) { CheckIndex(index); return _presets[index]; }
        public DeckValidationResult Validate(int index) => _validator.Validate(Get(index).ToDeckList());

        public void Rename(int index, string name)
        {
            name = CleanName(name);
            if (name.Length == 0) throw new ArgumentException("Name required.");
            _presets[index] = new DeckPreset(name, Get(index).Cards);
            Persist(index);
        }

        public void Replace(int index, string cardId)
        {
            var card = _catalog.Get(cardId);
            if (!OwnedCards.Contains(cardId)) throw new ArgumentException("Card not owned.");
            var preset = Get(index);
            var cards = preset.Cards.Where(id => _catalog.Get(id).Type != card.Type || _catalog.Get(id).Rarity != card.Rarity).ToList();
            cards.Add(cardId);
            _presets[index] = new DeckPreset(preset.Name, cards);
            Persist(index);
        }

        public bool Select(int index)
        {
            if (!Validate(index).IsValid) return false;
            _selected = index;
            _store.Write("collection.v1.selected", index.ToString(System.Globalization.CultureInfo.InvariantCulture));
            _store.Save();
            return true;
        }

        private void Persist(int index)
        {
            _store.Write(Key(index, "name"), _presets[index].Name);
            _store.Write(Key(index, "cards"), string.Join("\n", _presets[index].Cards));
            _store.Save();
        }
        private static string Key(int index, string suffix) => "collection.v1.deck." + index + "." + suffix;
        private static string CleanName(string name)
        {
            var clean = new string((name ?? "").Where(c => !char.IsControl(c)).ToArray()).Trim();
            return clean.Length > NameLimit ? clean.Substring(0, NameLimit) : clean;
        }
        private static void CheckIndex(int index)
        {
            if (index < 0 || index >= PresetCount) throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
