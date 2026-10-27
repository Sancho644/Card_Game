using System.Collections.Generic;
using System.Linq;
using Game.Core.Effects;
using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>
    /// Turns a finished battle into final powers, a winner and consequences. Does not change state.
    /// Order from docs/rules.md §6, «Подсчёт силы»: field values, then other abilities in play order,
    /// then comparison and consequences. A cancelled condition acts at no step.
    /// </summary>
    internal sealed class BattleResolver
    {
        private static readonly IReadOnlyList<FieldCondition> NoConditions = new FieldCondition[0];

        private readonly ConsequenceTable _consequenceTable;

        public BattleResolver(ConsequenceTable consequenceTable)
        {
            _consequenceTable = consequenceTable;
        }

        public BattleResolution Resolve(BattleState battle)
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

            var host = calculation.BreakdownOf(battle.Host);
            var guest = calculation.BreakdownOf(battle.Guest);
            var winner = calculation.Comparison.DecideWinner(calculation);
            var tieBroken = winner == null && host.FinalPower == guest.FinalPower && calculation.TieWinner != null;
            if (tieBroken)
                winner = calculation.TieWinner;

            var consequences = _consequenceTable.Build(battle.Host, winner, battle.Field.IsOpen);
            foreach (var change in calculation.ConsequenceChanges)
                change(consequences);

            return new BattleResolution(host, guest, calculation.Comparison.Rule, tieBroken, winner, consequences);
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
