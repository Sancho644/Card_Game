using Game.Core.Cards;
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

    /// <summary>Turns a finished battle into final powers, a winner and consequences. Does not change state.</summary>
    internal sealed class BattleResolver
    {
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

            foreach (var played in battle.PlayedAbilities)
            {
                foreach (var effect in played.Card.Effects)
                    effect.Apply(calculation, played.Player);
            }

            // TODO(rules): conditions run after abilities so a later ability can cancel them.
            // See docs/decisions.md.
            if (calculation.FieldOpen && calculation.FieldConditionActive)
            {
                foreach (var condition in field.Definition.Conditions)
                    condition.Apply(calculation);
            }

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
    }
}
