using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>
    /// Turns a finished battle into final powers, a winner and consequences. Does not change state.
    /// The powers come from <see cref="BattlePowerCounter"/>, then comparison and consequences.
    /// </summary>
    internal sealed class BattleResolver
    {
        private readonly BattlePowerCounter _counter;
        private readonly ConsequenceTable _consequenceTable;

        public BattleResolver(BattlePowerCounter counter, ConsequenceTable consequenceTable)
        {
            _counter = counter;
            _consequenceTable = consequenceTable;
        }

        public BattleResolution Resolve(BattleState battle)
        {
            var calculation = _counter.Count(battle);
            var host = calculation.BreakdownOf(battle.Host);
            var guest = calculation.BreakdownOf(battle.Guest);
            var winner = calculation.Comparison.DecideWinner(calculation);
            var tieBreak = winner == null || host.FinalPower != guest.FinalPower ? TieBreak.None
                : winner == calculation.TieWinner ? TieBreak.Ability
                : TieBreak.EarlierPlacement;

            var consequences = BuildConsequences(battle, calculation, winner);
            return new BattleResolution(host, guest, calculation.Comparison.Rule, tieBreak, winner, consequences);
        }

        /// <summary>What would happen if the battle ended now with this winner (null: a draw).</summary>
        public BattleConsequences ConsequencesFor(BattleState battle, PlayerId? winner) =>
            BuildConsequences(battle, _counter.Count(battle), winner);

        private BattleConsequences BuildConsequences(BattleState battle, BattleCalculation calculation, PlayerId? winner)
        {
            var consequences = _consequenceTable.Build(battle.Host, winner, battle.Field.IsOpen);
            foreach (var change in calculation.ConsequenceChanges)
                change(consequences);
            if (battle.FieldStays)
                consequences.FieldRemoved = false;
            return consequences;
        }
    }
}
