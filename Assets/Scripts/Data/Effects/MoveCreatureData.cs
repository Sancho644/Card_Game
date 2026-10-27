using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Существа/Переместить существо")]
    public sealed class MoveCreatureData : AbilityEffectData
    {
        /// <summary>True: your creature («Манёвр»); false: the opponent's («Смещение»).</summary>
        public bool OwnCreature = true;

        public override IAbilityEffect Create() => new MoveCreatureEffect(OwnCreature);
    }
}
