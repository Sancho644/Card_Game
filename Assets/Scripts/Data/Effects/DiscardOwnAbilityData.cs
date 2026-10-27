using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Сбросить свою способность из руки")]
    public sealed class DiscardOwnAbilityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new DiscardOwnAbilityEffect();
    }
}
