using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Битва/Отступление")]
    public sealed class RetreatData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new RetreatEffect();
    }
}
