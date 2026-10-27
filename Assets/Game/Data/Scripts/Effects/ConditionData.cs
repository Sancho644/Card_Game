using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Способности/Тишина: нельзя обычные и эпические")]
    public sealed class NoCommonOrEpicAbilitiesData : ConditionData
    {
        public override FieldCondition Create() => new NoCommonOrEpicAbilitiesCondition();
    }

    [Serializable, DataLabel("Способности/Право хозяина: обычные и эпические только у хозяина")]
    public sealed class HostOnlyAbilitiesData : ConditionData
    {
        public override FieldCondition Create() => new HostOnlyAbilitiesCondition();
    }

    [Serializable, DataLabel("Способности/Скупая битва: одна обычная или эпическая на игрока")]
    public sealed class OneAbilityPerPlayerData : ConditionData
    {
        public override FieldCondition Create() => new OneAbilityPerPlayerCondition();
    }

    [Serializable, DataLabel("Способности/Печать: нельзя legendary")]
    public sealed class NoLegendaryAbilitiesData : ConditionData
    {
        public override FieldCondition Create() => new NoLegendaryAbilitiesCondition();
    }

    [Serializable, DataLabel("Числа/Удвоение бонусов и штрафов")]
    public sealed class DoubleFieldValuesData : ConditionData
    {
        public override FieldCondition Create() => new DoubleFieldValuesCondition();
    }

    [Serializable, DataLabel("Числа/Дом хозяина: штрафы только для гостя")]
    public sealed class PenaltiesOnlyForGuestData : ConditionData
    {
        public override FieldCondition Create() => new PenaltiesOnlyForGuestCondition();
    }

    [Serializable, DataLabel("Сравнение/Закон слабейшего")]
    public sealed class LowerPowerWinsData : ConditionData
    {
        public override FieldCondition Create() => new LowerPowerWinsCondition();
    }

    [Serializable, DataLabel("Сравнение/Узкая грань")]
    public sealed class NarrowMarginDrawData : ConditionData
    {
        public int Margin = 20;

        public override FieldCondition Create() => new NarrowMarginDrawCondition(Margin);
    }

    [Serializable, DataLabel("Последствия/Милосердие: проигравший в руку")]
    public sealed class LoserReturnsToHandData : ConditionData
    {
        public override FieldCondition Create() => new LoserReturnsToHandCondition();
    }

    [Serializable, DataLabel("Последствия/Нерушимая: карта остаётся открытой")]
    public sealed class FieldStaysData : ConditionData
    {
        public override FieldCondition Create() => new FieldStaysCondition();
    }

    [Serializable, DataLabel("Последствия/Жертва: победитель тоже выбывает")]
    public sealed class WinnerOutOfGameData : ConditionData
    {
        public override FieldCondition Create() => new WinnerOutOfGameCondition();
    }

    [Serializable, DataLabel("Последствия/Трофей: победитель берёт способность из сброса")]
    public sealed class WinnerTakesAbilityData : ConditionData
    {
        public override FieldCondition Create() => new WinnerTakesAbilityCondition();
    }
}
