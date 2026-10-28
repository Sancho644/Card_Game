using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Cards
{
    /// <summary>
    /// A balance variant to try in simulations: card assets that replace the cards of the main database with
    /// the same id. The main card assets stay untouched; the overrides are separate copies kept outside the
    /// database. A variant can build on another one, whose overrides apply first.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Balance Variant", fileName = "BalanceVariant")]
    public sealed class BalanceVariantDef : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, Tooltip("Applied first; this variant's overrides go on top")] private BalanceVariantDef _basedOn;
        [SerializeField, Tooltip("Copies of cards with changed numbers; the id must match a card in the database")]
        private List<CardDef> _overrides = new List<CardDef>();

        public string Id => _id;
        public string DisplayName => _displayName;
        public BalanceVariantDef BasedOn => _basedOn;
        public IReadOnlyList<CardDef> Overrides => _overrides;

        /// <summary>Engine data of the database with this variant's cards in place of the originals.</summary>
        public CardCatalog ToCatalog(CardDatabase database) =>
            new CardCatalog(database.Cards.Where(card => card != null).Select(card => Resolve(card.Id) ?? card).Select(card => card.ToCore()));

        /// <summary>Problems that would make the variant misleading; empty when it is fine.</summary>
        public IEnumerable<string> Validate(CardDatabase database)
        {
            var visited = new HashSet<BalanceVariantDef>();
            for (var variant = this; variant != null; variant = variant._basedOn)
            {
                if (!visited.Add(variant))
                {
                    yield return "the chain of base variants loops";
                    yield break;
                }

                foreach (var card in variant._overrides)
                {
                    if (card == null)
                    {
                        yield return $"{variant.name}: the list has an empty slot";
                        continue;
                    }

                    var original = database.Find(card.Id);
                    if (original == null)
                        yield return $"{variant.name}: {card.name} has id '{card.Id}' that is not in the database";
                    else if (original.Type != card.Type || original.Rarity != card.Rarity)
                        yield return $"{variant.name}: {card.name} changes the type or rarity of '{card.Id}'";
                    else if (ReferenceEquals(original, card))
                        yield return $"{variant.name}: {card.name} is the database card itself, not a copy";
                }
            }
        }

        /// <summary>This variant's card with the id, else the base variant's; null if none overrides it.</summary>
        private CardDef Resolve(string id) =>
            _overrides.LastOrDefault(card => card != null && card.Id == id) ?? _basedOn?.Resolve(id);
    }
}
