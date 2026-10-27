using System.Collections.Generic;
using System.Linq;
using Game.Core.Decks;
using Game.Data.Cards;
using UnityEngine;

namespace Game.Data.Decks
{
    /// <summary>A ready-made deck (test decks, AI decks). The engine receives only card ids.</summary>
    [CreateAssetMenu(menuName = "Game/Deck", fileName = "Deck")]
    public sealed class DeckDef : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private List<CardDef> _cards = new List<CardDef>();

        public string Id => _id;
        public string DisplayName => _displayName;
        public IReadOnlyList<CardDef> Cards => _cards;

        public DeckList ToDeckList() => new DeckList(_cards.Where(card => card != null).Select(card => card.Id));
    }
}
