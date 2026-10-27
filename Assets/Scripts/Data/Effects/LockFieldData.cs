using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Запереть закрытую карту соперника")]
    public sealed class LockFieldData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new LockFieldEffect();
    }
}
