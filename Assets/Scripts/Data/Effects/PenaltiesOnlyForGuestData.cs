using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Числа/Дом хозяина: штрафы только для гостя")]
    public sealed class PenaltiesOnlyForGuestData : ConditionData
    {
        public override FieldCondition Create() => new PenaltiesOnlyForGuestCondition();
    }
}
