using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>Battle consequences from docs/rules.md §6, before any card effects.</summary>
    internal sealed class ConsequenceTable
    {
        public BattleConsequences Build(PlayerId host, PlayerId? winner, bool fieldOpen)
        {
            var consequences = new BattleConsequences(host, winner, fieldOpen);
            if (winner == null)
            {
                ApplyDrawRule(consequences);
                return consequences;
            }

            var winnerId = winner.Value;
            var loserId = winnerId.Opponent();
            var hostKeepsClosedField = !fieldOpen && winnerId == host;

            consequences.WinScoredBy = winnerId;
            consequences.SetFate(loserId, CreatureFate.OutOfGame);
            consequences.SetFate(winnerId, hostKeepsClosedField ? CreatureFate.StaysOnField : CreatureFate.ReturnsToHand);
            consequences.FieldRemoved = !hostKeepsClosedField;
            return consequences;
        }

        /// <summary>
        /// Temporary draw rule (docs/rules.md §6, open question 1). This is the only place it is defined.
        /// No win; both creatures return to hand; an open field is removed, a closed one stays.
        /// </summary>
        private static void ApplyDrawRule(BattleConsequences consequences)
        {
            consequences.WinScoredBy = null;
            consequences.SetFate(consequences.Host, CreatureFate.ReturnsToHand);
            consequences.SetFate(consequences.Guest, CreatureFate.ReturnsToHand);
            consequences.FieldRemoved = consequences.FieldWasOpen;
        }
    }
}
