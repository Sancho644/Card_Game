using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Сравнение/Узкая грань")]
    public sealed class NarrowMarginDrawData : ConditionData
    {
        public int Margin = 20;

        public override FieldCondition Create() => new NarrowMarginDrawCondition(Margin);
    }
}
