using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Последствия/Плацдарм: победивший гость остаётся на поле")]
    public sealed class GuestWinnerStaysData : ConditionData
    {
        public override FieldCondition Create() => new GuestWinnerStaysCondition();
    }
}