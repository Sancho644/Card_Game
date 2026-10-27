using Game.Core.Battle;
using Game.Core.Cards;
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

    /// <summary>The battle's field card is open (or closed).</summary>
    public sealed class FieldStateRequirement : IEffectRequirement
    {
        private readonly bool _mustBeOpen;

        public FieldStateRequirement(bool mustBeOpen)
        {
            _mustBeOpen = mustBeOpen;
        }

        // TODO(rules): checked against the field state at resolution, not at the moment the ability
        // was played. See docs/decisions.md.
        public bool IsMet(BattleCalculation calculation, PlayerId caster) => calculation.FieldOpen == _mustBeOpen;
    }

    /// <summary>The caster's creature has the given element.</summary>
    public sealed class CreatureElementRequirement : IEffectRequirement
    {
        private readonly Element _element;

        public CreatureElementRequirement(Element element)
        {
            _element = element;
        }

        public bool IsMet(BattleCalculation calculation, PlayerId caster) =>
            calculation.CreatureOf(caster).Element == _element;
    }
}
