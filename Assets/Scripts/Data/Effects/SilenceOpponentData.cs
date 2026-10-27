using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Соперник не играет обычные и эпические до конца битвы")]
    public sealed class SilenceOpponentData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new SilenceOpponentEffect();
    }
}
