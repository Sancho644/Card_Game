using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Последствия/Жертва: победитель тоже выбывает")]
    public sealed class WinnerOutOfGameData : ConditionData
    {
        public override FieldCondition Create() => new WinnerOutOfGameCondition();
    }
}
