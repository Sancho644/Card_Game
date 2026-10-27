using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Cards
{
    /// <summary>All cards of the game, looked up by stable id.</summary>
    [CreateAssetMenu(menuName = "Game/Card Database", fileName = "CardDatabase")]
    public sealed class CardDatabase : ScriptableObject
    {
        [SerializeField] private List<CardDef> _cards = new List<CardDef>();

        private Dictionary<string, CardDef> _byId;

        public IReadOnlyList<CardDef> Cards => _cards;

        public CardDef Find(string id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, CardDef>();
                foreach (var card in _cards.Where(card => card != null))
                    _byId[card.Id] = card;
            }

            return _byId.TryGetValue(id, out var found) ? found : null;
        }

        /// <summary>Plain engine data for a match.</summary>
        public CardCatalog ToCatalog() => new CardCatalog(_cards.Select(card => card.ToCore()));

        /// <summary>Problems across the whole database; empty when everything is fine.</summary>
        public IEnumerable<string> Validate()
        {
            if (_cards.Any(card => card == null))
                yield return "the list has an empty slot";

            var cards = _cards.Where(card => card != null).ToList();
            foreach (var duplicate in cards.GroupBy(card => card.Id).Where(group => group.Count() > 1))
                yield return $"id '{duplicate.Key}' is used by {string.Join(", ", duplicate.Select(card => card.name))}";
            foreach (var card in cards)
            {
                foreach (var problem in card.Validate())
                    yield return $"{card.name}: {problem}";
            }

            foreach (var ability in cards.OfType<AbilityDef>().Where(ability => ability.BoundCreature != null))
            {
                if (!cards.Contains(ability.BoundCreature))
                    yield return $"{ability.name}: its creature {ability.BoundCreature.name} is not in the database";
            }
        }

        private void OnValidate() => _byId = null;
    }
}
