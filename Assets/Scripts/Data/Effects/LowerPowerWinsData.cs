using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Сравнение/Закон слабейшего")]
    public sealed class LowerPowerWinsData : ConditionData
    {
        public override FieldCondition Create() => new LowerPowerWinsCondition();
    }
}
