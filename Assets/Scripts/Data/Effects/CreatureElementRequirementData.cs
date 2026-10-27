using System;
using Game.Core.Cards;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Если твоё существо этой стихии")]
    public sealed class CreatureElementRequirementData : RequirementData
    {
        public Element Element = Element.Fire;

        public override IEffectRequirement Create() => new CreatureElementRequirement(Element);
    }
}
