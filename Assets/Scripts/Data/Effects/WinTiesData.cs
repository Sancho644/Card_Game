using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Исход/При равенстве побеждает твоё существо")]
    public sealed class WinTiesData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new WinTiesEffect();
    }
}
