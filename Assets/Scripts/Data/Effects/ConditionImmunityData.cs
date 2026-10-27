using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Условие не действует на твоё существо")]
    public sealed class ConditionImmunityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ConditionImmunityEffect();
    }
}
