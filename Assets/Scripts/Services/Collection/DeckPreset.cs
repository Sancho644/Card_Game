using System.Collections.Generic;
using System.Linq;
using Game.Core.Decks;
namespace Game.Services.Collection
{
    public sealed class DeckPreset
    {
        public DeckPreset(string name, IEnumerable<string> cards)
        {
            Name = name;
            Cards = System.Array.AsReadOnly(cards.ToArray());
        }
        public string Name { get; }
        public IReadOnlyList<string> Cards { get; }
        public DeckList ToDeckList() => new DeckList(Cards);
    }
}
