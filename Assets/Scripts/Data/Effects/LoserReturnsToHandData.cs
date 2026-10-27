using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Последствия/Милосердие: проигравший в руку")]
    public sealed class LoserReturnsToHandData : ConditionData
    {
        public override FieldCondition Create() => new LoserReturnsToHandCondition();
    }
}
