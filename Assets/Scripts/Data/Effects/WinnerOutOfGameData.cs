using System;
using Game.Core.Effects;
using UnityEngine;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Последствия/Жертва: победитель тоже выбывает")]
    public sealed class WinnerOutOfGameData : ConditionData
    {
        [Tooltip("Only a winning guest goes out; a winning host stays")]
        public bool OnlyGuest;

        public override FieldCondition Create() => new WinnerOutOfGameCondition(OnlyGuest);
    }
}
