using System;
using Game.Core.Battle;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Если ты хозяин или гость")]
    public sealed class BattleRoleRequirementData : RequirementData
    {
        public BattleRole Role = BattleRole.Host;

        public override IEffectRequirement Create() => new BattleRoleRequirement(Role);
    }
}
