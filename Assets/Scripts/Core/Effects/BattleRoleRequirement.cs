using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>The caster is the host or the guest of the battle.</summary>
    public sealed class BattleRoleRequirement : IEffectRequirement
    {
        private readonly BattleRole _role;

        public BattleRoleRequirement(BattleRole role)
        {
            _role = role;
        }

        public bool IsMet(BattleCalculation calculation, PlayerId caster) => calculation.RoleOf(caster) == _role;
    }
}
