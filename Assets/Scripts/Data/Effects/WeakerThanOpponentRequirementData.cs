using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Если твоё существо слабее")]
    public sealed class WeakerThanOpponentRequirementData : RequirementData
    {
        public override IEffectRequirement Create() => new WeakerThanOpponentRequirement();
    }
}
