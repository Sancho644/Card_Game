using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>
    /// A special battle rule printed on a field card. Active only while the field is open.
    /// Override just the hooks the condition needs.
    /// </summary>
    public abstract class FieldCondition
    {
        /// <summary>Validation hook: may forbid an ability from being played.</summary>
        public virtual bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) => true;

        /// <summary>
        /// Resolution hook: may change numbers, the comparison rule or the consequences.
        /// Runs after the abilities and only if none of them cancelled the condition.
        /// </summary>
        public virtual void Apply(BattleCalculation calculation)
        {
        }
    }
}
