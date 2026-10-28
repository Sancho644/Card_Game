using System.Collections.Generic;
using System.Linq;
using Game.Core.Effects;
using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>
    /// The power count of docs/rules.md §6, «Подсчёт силы»: field values, then other abilities in play
    /// order, then conditions that change the comparison. A cancelled condition acts at no step. Used both
    /// for the final result and for the live power shown during the battle.
    /// </summary>
    internal sealed class BattlePowerCounter
    {
        private static readonly IReadOnlyList<FieldCondition> NoConditions = new FieldCondition[0];

        public BattleCalculation Count(BattleState battle)
        {
            var calculation = new BattleCalculation(battle);
            var conditions = calculation.FieldConditionActive ? battle.Field.Definition.Conditions : NoConditions;

            // Step 1: field values, changed by conditions and then by abilities.
            foreach (var condition in conditions)
                condition.ModifyFieldValues(calculation);
            ApplyAbilities(battle, calculation, BattleStep.FieldValues);

            // Step 2: the other abilities in play order.
            ApplyAbilities(battle, calculation, BattleStep.Power);

            // Step 3: conditions that change the comparison and the consequences.
            foreach (var condition in conditions)
                condition.ModifyOutcome(calculation);

            return calculation;
        }

        public BattlePower Power(BattleState battle)
        {
            var calculation = Count(battle);
            return new BattlePower(
                battle.Field.Id,
                battle.Host,
                calculation.BreakdownOf(battle.Host),
                calculation.BreakdownOf(battle.Guest),
                calculation.Comparison.Rule,
                calculation.Comparison.WinnerOnEqualPower(calculation));
        }

        private static void ApplyAbilities(BattleState battle, BattleCalculation calculation, BattleStep step)
        {
            foreach (var played in battle.ActiveAbilities)
            {
                calculation.CurrentSource = played.Instance;
                foreach (var effect in played.Card.Effects.OfType<IBattleEffect>().Where(effect => effect.Step == step))
                    effect.Apply(calculation, played.Player);
            }

            calculation.CurrentSource = null;
        }
    }
}
