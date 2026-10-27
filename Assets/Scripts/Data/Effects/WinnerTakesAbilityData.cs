using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Последствия/Трофей: победитель берёт способность из сброса")]
    public sealed class WinnerTakesAbilityData : ConditionData
    {
        public override FieldCondition Create() => new WinnerTakesAbilityCondition();
    }
}
