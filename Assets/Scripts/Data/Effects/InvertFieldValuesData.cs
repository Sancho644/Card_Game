using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Поменять местами бонусы и штрафы")]
    public sealed class InvertFieldValuesData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new InvertFieldValuesEffect();
    }
}
