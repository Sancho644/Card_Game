using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Числа/Удвоение бонусов и штрафов")]
    public sealed class DoubleFieldValuesData : ConditionData
    {
        public override FieldCondition Create() => new DoubleFieldValuesCondition();
    }
}
