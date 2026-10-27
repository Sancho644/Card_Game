using System;
using System.Collections.Generic;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Config
{
    [Serializable]
    public struct IntRange
    {
        public int Min;
        public int Max;

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int value) => value >= Min && value <= Max;

        public override string ToString() => Min == Max ? Min.ToString() : $"{Min}–{Max}";
    }

    [Serializable]
    public sealed class RarityBalance
    {
        public Rarity Rarity;
        public IntRange CreaturePower;

        [Tooltip("Each bonus (positive value) of a field card.")]
        public IntRange FieldBonus;

        [Tooltip("How many elements a field card gives a bonus to.")]
        public IntRange FieldBonusElements;

        [Tooltip("Total power an ability adds to its own creature.")]
        public IntRange AbilityPower;

        public RarityBalance(Rarity rarity, IntRange creaturePower, IntRange fieldBonus, IntRange fieldBonusElements, IntRange abilityPower)
        {
            Rarity = rarity;
            CreaturePower = creaturePower;
            FieldBonus = fieldBonus;
            FieldBonusElements = fieldBonusElements;
            AbilityPower = abilityPower;
        }
    }

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
