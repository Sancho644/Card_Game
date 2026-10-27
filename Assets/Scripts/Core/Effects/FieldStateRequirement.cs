using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
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
}
