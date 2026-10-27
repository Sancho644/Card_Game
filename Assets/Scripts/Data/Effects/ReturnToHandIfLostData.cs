using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Исход/Проиграв, существо возвращается в руку")]
    public sealed class ReturnToHandIfLostData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ReturnToHandIfLostEffect();
    }
}
