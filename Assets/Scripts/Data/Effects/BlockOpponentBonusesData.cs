using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Бонусы не действуют на соперника")]
    public sealed class BlockOpponentBonusesData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new BlockOpponentBonusesEffect();
    }
}
