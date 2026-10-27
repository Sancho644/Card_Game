using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Существа/Вернуть выбывшее существо в руку")]
    public sealed class ReturnCreatureFromOutOfGameData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ReturnCreatureFromOutOfGameEffect();
    }
}
