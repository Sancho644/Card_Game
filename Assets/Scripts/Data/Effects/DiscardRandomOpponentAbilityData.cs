using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Соперник сбрасывает случайную способность")]
    public sealed class DiscardRandomOpponentAbilityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new DiscardRandomOpponentAbilityEffect();
    }
}
