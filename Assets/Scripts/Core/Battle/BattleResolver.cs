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
            var tieBroken = winner == null && host.FinalPower == guest.FinalPower && calculation.TieWinner != null;
            if (tieBroken)
                winner = calculation.TieWinner;

            var consequences = _consequenceTable.Build(battle.Host, winner, battle.Field.IsOpen);
            foreach (var change in calculation.ConsequenceChanges)
                change(consequences);
            if (battle.FieldStays)
                consequences.FieldRemoved = false;

            return new BattleResolution(host, guest, calculation.Comparison.Rule, tieBroken, winner, consequences);
        }
    }
}
