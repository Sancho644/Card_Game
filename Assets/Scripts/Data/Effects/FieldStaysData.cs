using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Последствия/Нерушимая: карта остаётся открытой")]
    public sealed class FieldStaysData : ConditionData
    {
        public override FieldCondition Create() => new FieldStaysCondition();
    }
}
