using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Вернуть способность из сброса")]
    public sealed class ReturnAbilityFromDiscardData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ReturnAbilityFromDiscardEffect();
    }
}
