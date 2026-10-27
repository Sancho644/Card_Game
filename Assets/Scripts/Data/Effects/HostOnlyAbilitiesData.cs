using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Право хозяина: обычные и эпические только у хозяина")]
    public sealed class HostOnlyAbilitiesData : ConditionData
    {
        public override FieldCondition Create() => new HostOnlyAbilitiesCondition();
    }
}
