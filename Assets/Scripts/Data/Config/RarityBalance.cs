using System;
using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Config
{
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
}
