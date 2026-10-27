using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Если карта поля открыта или закрыта")]
    public sealed class FieldStateRequirementData : RequirementData
    {
        public bool MustBeOpen;

        public override IEffectRequirement Create() => new FieldStateRequirement(MustBeOpen);
    }
}
