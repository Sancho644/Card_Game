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

    [Serializable, DataLabel("Сила/Изменить силу существа соперника")]
    public sealed class OpponentPowerData : AbilityEffectData
    {
        public int Amount = -20;

        public override IAbilityEffect Create() => new OpponentPowerEffect(Amount);
    }

    [Serializable, DataLabel("Исход/При равенстве побеждает твоё существо")]
    public sealed class WinTiesData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new WinTiesEffect();
    }

    [Serializable, DataLabel("Исход/Проиграв, существо возвращается в руку")]
    public sealed class ReturnToHandIfLostData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ReturnToHandIfLostEffect();
    }

    [Serializable, DataLabel("Карта поля/Отменить условие")]
    public sealed class CancelFieldConditionData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new CancelFieldConditionEffect();
    }

    [Serializable, DataLabel("Карта поля/Условие не действует на твоё существо")]
    public sealed class ConditionImmunityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ConditionImmunityEffect();
    }

    [Serializable, DataLabel("Карта поля/Поменять местами бонусы и штрафы")]
    public sealed class InvertFieldValuesData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new InvertFieldValuesEffect();
    }

    [Serializable, DataLabel("Карта поля/Бонусы не действуют на соперника")]
    public sealed class BlockOpponentBonusesData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new BlockOpponentBonusesEffect();
    }
}
