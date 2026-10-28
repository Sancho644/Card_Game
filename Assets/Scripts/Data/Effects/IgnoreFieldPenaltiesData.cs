using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Штрафы не действуют на твоё существо")]
    public sealed class IgnoreFieldPenaltiesData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new IgnoreFieldPenaltiesEffect();
    }
}
