using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Сила/Изменить силу существа соперника")]
    public sealed class OpponentPowerData : AbilityEffectData
    {
        public int Amount = -20;

        public override IAbilityEffect Create() => new OpponentPowerEffect(Amount);
    }
}
