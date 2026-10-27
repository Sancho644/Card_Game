using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>
    /// A special battle rule printed on a field card. Active only while the field is open and
    /// not cancelled by an ability. Override just the hooks the condition needs.
    /// </summary>
    public abstract class FieldCondition
    {
        /// <summary>Validation hook: may forbid an ability from being played.</summary>
        public virtual bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) => true;

        /// <summary>Step 1 of the power count: change the field card's values, e.g. double them.</summary>
        public virtual void ModifyFieldValues(BattleCalculation calculation)
        {
        }

        /// <summary>Step 3 of the power count: change the comparison rule or the consequences.</summary>
        public virtual void ModifyOutcome(BattleCalculation calculation)
        {
        }
    }
}
