using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Отменить условие")]
    public sealed class CancelFieldConditionData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new CancelFieldConditionEffect();
    }
}
