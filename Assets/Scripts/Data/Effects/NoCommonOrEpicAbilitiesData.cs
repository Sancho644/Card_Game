using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Тишина: нельзя обычные и эпические")]
    public sealed class NoCommonOrEpicAbilitiesData : ConditionData
    {
        public override FieldCondition Create() => new NoCommonOrEpicAbilitiesCondition();
    }
}
