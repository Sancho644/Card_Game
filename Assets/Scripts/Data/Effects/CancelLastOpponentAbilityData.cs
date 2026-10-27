using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Отменить последнюю обычную или эпическую способность соперника")]
    public sealed class CancelLastOpponentAbilityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new CancelLastOpponentAbilityEffect();
    }
}
