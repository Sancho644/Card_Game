using System.Collections.Generic;
using System.Linq;
using Game.Core.Effects;
using Game.Core.State;

namespace Game.Core.Battle
{
    public sealed class BattleResolution
    {
        internal BattleResolution(int hostPower, int guestPower, PlayerId? winner, BattleConsequences consequences)
        {
            HostPower = hostPower;
            GuestPower = guestPower;
            Winner = winner;
            Consequences = consequences;
        }

        public int HostPower { get; }
        public int GuestPower { get; }
        public PlayerId? Winner { get; }
        public BattleConsequences Consequences { get; }
    }

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

            var hostPower = calculation.PowerOf(battle.Host);
            var guestPower = calculation.PowerOf(battle.Guest);
            var winner = calculation.Comparison.DecideWinner(calculation);
            if (winner == null && hostPower == guestPower)
                winner = calculation.TieWinner;

            var consequences = _consequenceTable.Build(battle.Host, winner, battle.Field.IsOpen);
            foreach (var change in calculation.ConsequenceChanges)
                change(consequences);

            return new BattleResolution(hostPower, guestPower, winner, consequences);
        }

        private static void ApplyAbilities(BattleState battle, BattleCalculation calculation, BattleStep step)
        {
            foreach (var played in battle.ActiveAbilities)
            {
                foreach (var effect in played.Card.Effects.OfType<IBattleEffect>().Where(effect => effect.Step == step))
                    effect.Apply(calculation, played.Player);
            }
        }
    }
}
