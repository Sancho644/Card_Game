using System.Collections.Generic;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Cards
{
    /// <summary>
    /// A card as the designer edits it. The engine never sees this asset: <see cref="ToCore"/> turns it
    /// into plain engine data, and everything refers to the card by its stable <see cref="Id"/>.
    /// </summary>
    public abstract class CardDef : ScriptableObject
    {
        [Tooltip("Stable id shared by client and server, e.g. field.forge. Never change it after release.")]
        [SerializeField] private string _id;

        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 6)] private string _description;
        [SerializeField] private Rarity _rarity;

        [Header("Art")]
        [SerializeField] private Sprite _art;
        [SerializeField] private Sprite _frame;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Rarity Rarity => _rarity;
        public Sprite Art => _art;
        public Sprite Frame => _frame;
        public abstract CardType Type { get; }

        public abstract CardDefinition ToCore();

        /// <summary>Problems that would make the card unusable; empty when the card is fine.</summary>
        public virtual IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(_id))
                yield return "id is empty";
            if (string.IsNullOrWhiteSpace(_displayName))
                yield return "name is empty";
        }
    }
}
