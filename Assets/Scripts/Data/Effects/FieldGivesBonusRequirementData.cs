using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Если карта открыта и даёт твоему существу бонус")]
    public sealed class FieldGivesBonusRequirementData : RequirementData
    {
        public override IEffectRequirement Create() => new FieldGivesBonusRequirement();
    }
}
