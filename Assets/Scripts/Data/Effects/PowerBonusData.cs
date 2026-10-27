using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Effects;
using UnityEngine;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Сила/+N к силе своего существа")]
    public sealed class PowerBonusData : AbilityEffectData
    {
        public int Amount = 50;

        [SerializeReference]
        public List<RequirementData> Requirements = new List<RequirementData>();

        public override IAbilityEffect Create() =>
            new PowerBonusEffect(Amount, Requirements.Where(requirement => requirement != null)
                .Select(requirement => requirement.Create())
                .ToArray());
    }
}
