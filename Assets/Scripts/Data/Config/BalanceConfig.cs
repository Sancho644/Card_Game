using System.Collections.Generic;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>Reference numbers by rarity (docs/rules.md §10), used to check cards.</summary>
    [CreateAssetMenu(menuName = "Game/Config/Balance", fileName = "Balance")]
    public sealed class BalanceConfig : ScriptableObject
    {
        [SerializeField] private List<RarityBalance> _rarities = new List<RarityBalance>
        {
            new RarityBalance(Rarity.Common, new IntRange(100, 200), new IntRange(30, 50), new IntRange(1, 2), new IntRange(30, 60)),
            new RarityBalance(Rarity.Epic, new IntRange(150, 250), new IntRange(50, 80), new IntRange(2, 3), new IntRange(40, 70)),
            new RarityBalance(Rarity.Legendary, new IntRange(200, 300), new IntRange(80, 120), new IntRange(3, 4), new IntRange(0, 0)),
        };

        [Tooltip("Total power a signature legendary ability adds (docs/cards.md §5).")]
        [SerializeField] private IntRange _signatureAbilityPower = new IntRange(60, 100);

        public IntRange SignatureAbilityPower => _signatureAbilityPower;

        public RarityBalance For(Rarity rarity) => _rarities.Find(entry => entry.Rarity == rarity);
    }
}
