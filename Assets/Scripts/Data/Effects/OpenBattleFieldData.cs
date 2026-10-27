using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Хозяин сразу открывает карту")]
    public sealed class OpenBattleFieldData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new OpenBattleFieldEffect();
    }
}
