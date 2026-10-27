using System.Collections.Generic;
using Game.Core.Cards;
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
    /// Order from docs/rules.md §6, «Подсчёт силы»:
    /// condition cancellation, then field values, then other abilities in play order, then comparison and consequences.
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
            var field = battle.Field;
            var calculation = new BattleCalculation(
                battle.Host,
                (CreatureCard)field.OccupantOf(battle.Host).Definition,
                (CreatureCard)field.OccupantOf(battle.Guest).Definition,
                field.Definition,
                field.IsOpen);

            // A cancelled condition does not act at any step, whenever the cancelling ability was played.
            ApplyAbilities(battle, calculation, BattleStep.ConditionControl);
            var conditions = calculation.FieldOpen && calculation.FieldConditionActive
                ? field.Definition.Conditions
                : NoConditions;

            // Step 1: field values, changed by conditions and then by abilities.
            foreach (var condition in conditions)
                condition.ModifyFieldValues(calculation);
            ApplyAbilities(battle, calculation, BattleStep.FieldValues);

            // Step 2: the other abilities in play order.
            ApplyAbilities(battle, calculation, BattleStep.Power);

            // Step 3: conditions that change the comparison and the consequences.
            foreach (var condition in conditions)
                condition.ModifyOutcome(calculation);

            var winner = calculation.Comparison.DecideWinner(calculation);
            var consequences = _consequenceTable.Build(battle.Host, winner, field.IsOpen);
            foreach (var modifier in calculation.ConsequenceModifiers)
                modifier.Modify(consequences);

            return new BattleResolution(
                calculation.PowerOf(battle.Host),
                calculation.PowerOf(battle.Guest),
                winner,
                consequences);
        }

        private static void ApplyAbilities(BattleState battle, BattleCalculation calculation, BattleStep step)
        {
            foreach (var played in battle.PlayedAbilities)
            {
                foreach (var effect in played.Card.Effects)
                {
                    if (effect.Step == step)
                        effect.Apply(calculation, played.Player);
                }
            }
        }
    }
}
