using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Битва/Первый ход в битве делаешь ты")]
    public sealed class TakeFirstBattleTurnData : AbilityEffectData
    {
        public override IAbilityEffect Create() => new TakeFirstBattleTurnEffect();
    }
}
