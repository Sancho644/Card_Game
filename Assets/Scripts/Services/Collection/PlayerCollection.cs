using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Rules;

namespace Game.Services.Collection
{
    /// <summary>Persistent local deck drafts, retaining legacy slot indexes when the library grows.</summary>
    public sealed class PlayerCollection
    {
        public const int NameLimit = 32;
        private const int InitialPresetCount = 3;
        private readonly ICollectionStore _store;
        private readonly CardCatalog _catalog;
        private readonly DeckValidator _validator;
        private readonly List<DeckPreset> _presets = new List<DeckPreset>();
        private int _selected;

        public PlayerCollection(ICollectionStore store, CardCatalog catalog, GameRules rules, IReadOnlyList<DeckPreset> defaults)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _catalog = catalog;
            _validator = new DeckValidator(catalog, rules.Deck);
            if (defaults == null || defaults.Count == 0) throw new ArgumentException("Default decks required.");
            // TODO(rules): All cards are owned in the prototype until acquisition is defined (docs/decisions.md).
            OwnedCards = Array.AsReadOnly(catalog.All.Select(c => c.Id).ToArray());
            var count = int.TryParse(store.Read("collection.v1.count"), out var storedCount)
                ? Math.Max(InitialPresetCount, storedCount) : InitialPresetCount;
            for (var i = 0; i < count; i++)
            {
                var fallback = i < InitialPresetCount ? defaults[i % defaults.Count] : new DeckPreset("", Array.Empty<string>());
                var saved = store.Read(Key(i, "cards"));
                var cards = saved == null ? fallback.Cards.ToArray() : saved.Length == 0 ? Array.Empty<string>() : saved.Split('\n');
                if (cards.Any(id => string.IsNullOrEmpty(id) || !catalog.TryGet(id, out _)))
                    cards = fallback.Cards.ToArray();
                var name = CleanName(store.Read(Key(i, "name")));
                _presets.Add(new DeckPreset(saved == null && name.Length == 0 ? fallback.Name : name, cards));
            }
            if (!int.TryParse(store.Read("collection.v1.selected"), out _selected) || _selected < 0 || _selected >= Count)
                _selected = 0;
        }

        public IReadOnlyList<string> OwnedCards { get; }
        public int Count => _presets.Count;
        public int SelectedIndex => _selected;
        public DeckPreset Selected => Get(_selected);
        public DeckPreset Get(int index) { CheckIndex(index); return _presets[index]; }
        public DeckValidationResult Validate(int index) => _validator.Validate(Get(index).ToDeckList());

        public int Create(string name)
        {
            name = CleanName(name);
            if (name.Length == 0) throw new ArgumentException("Name required.");
            var index = Count;
            _presets.Add(new DeckPreset(name, Array.Empty<string>()));
            Persist(index);
            return index;
        }

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

        public void Remove(int index, string cardId)
        {
            var preset = Get(index);
            _presets[index] = new DeckPreset(preset.Name, preset.Cards.Where(id => id != cardId));
            Persist(index);
        }

        public void Delete(int index)
        {
            CheckIndex(index);
            _presets[index] = new DeckPreset("", Array.Empty<string>());
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
            _store.Write("collection.v1.count", Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
        private void CheckIndex(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
