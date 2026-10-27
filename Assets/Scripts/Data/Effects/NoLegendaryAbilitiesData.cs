using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Печать: нельзя legendary")]
    public sealed class NoLegendaryAbilitiesData : ConditionData
    {
        public override FieldCondition Create() => new NoLegendaryAbilitiesCondition();
    }
}
