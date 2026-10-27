using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Карта поля/Хозяин сразу открывает карту")]
    public sealed class OpenBattleFieldData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new OpenBattleFieldEffect();
    }

    [Serializable, DataLabel("Способности/Отменить последнюю обычную или эпическую способность соперника")]
    public sealed class CancelLastOpponentAbilityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new CancelLastOpponentAbilityEffect();
    }

    [Serializable, DataLabel("Способности/Соперник не играет обычные и эпические до конца битвы")]
    public sealed class SilenceOpponentData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new SilenceOpponentEffect();
    }

    [Serializable, DataLabel("Способности/Сбросить свою способность из руки")]
    public sealed class DiscardOwnAbilityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new DiscardOwnAbilityEffect();
    }

    [Serializable, DataLabel("Способности/Соперник сбрасывает случайную способность")]
    public sealed class DiscardRandomOpponentAbilityData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new DiscardRandomOpponentAbilityEffect();
    }

    [Serializable, DataLabel("Способности/Вернуть способность из сброса")]
    public sealed class ReturnAbilityFromDiscardData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ReturnAbilityFromDiscardEffect();
    }

    [Serializable, DataLabel("Битва/Первый ход в битве делаешь ты")]
    public sealed class TakeFirstBattleTurnData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new TakeFirstBattleTurnEffect();
    }

    [Serializable, DataLabel("Битва/Отступление")]
    public sealed class RetreatData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new RetreatEffect();
    }

    [Serializable, DataLabel("Существа/Переместить существо")]
    public sealed class MoveCreatureData : AbilityEffectData
    {
        /// <summary>True: your creature («Манёвр»); false: the opponent's («Смещение»).</summary>
        public bool OwnCreature = true;

        public override IAbilityEffect Create() => new MoveCreatureEffect(OwnCreature);
    }

    [Serializable, DataLabel("Существа/Вернуть выбывшее существо в руку")]
    public sealed class ReturnCreatureFromOutOfGameData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ReturnCreatureFromOutOfGameEffect();
    }

    [Serializable, DataLabel("Информация/Разведка")]
    public sealed class ScoutData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new ScoutEffect();
    }

    [Serializable, DataLabel("Карта поля/Запереть закрытую карту соперника")]
    public sealed class LockFieldData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new LockFieldEffect();
    }
}
