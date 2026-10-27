using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Скупая битва: одна обычная или эпическая на игрока")]
    public sealed class OneAbilityPerPlayerData : ConditionData
    {
        public override FieldCondition Create() => new OneAbilityPerPlayerCondition();
    }
}
