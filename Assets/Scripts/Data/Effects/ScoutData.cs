using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Информация/Разведка")]
    public sealed class ScoutData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ScoutEffect();
    }
}
