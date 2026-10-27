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

        /// <summary>Checked at the power count, not when the ability was played (docs/rules.md §6).</summary>
        public bool IsMet(BattleCalculation calculation, PlayerId caster) => calculation.FieldOpen == _mustBeOpen;
    }

    /// <summary>The field is open and gives the caster's creature a bonus.</summary>
    public sealed class FieldGivesBonusRequirement : IEffectRequirement
    {
        public bool IsMet(BattleCalculation calculation, PlayerId caster) =>
            calculation.FieldOpen && calculation.FieldModifierFor(caster) > 0;
    }

    /// <summary>
    /// The caster's creature is weaker than the opponent's, counting the effects applied before
    /// this one in play order.
    /// </summary>
    public sealed class WeakerThanOpponentRequirement : IEffectRequirement
    {
        public bool IsMet(BattleCalculation calculation, PlayerId caster) =>
            calculation.PowerOf(caster) < calculation.PowerOf(caster.Opponent());
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
