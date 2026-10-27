using System;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>Battle consequences from docs/rules.md §6, before any card effects.</summary>
    internal sealed class ConsequenceTable
    {
        private readonly DrawRule _drawRule;

        public ConsequenceTable(DrawRule drawRule)
        {
            _drawRule = drawRule;
        }

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

        /// <summary>The draw rule (temporary, docs/rules.md §6). This is the only place draws are settled.</summary>
        private void ApplyDrawRule(BattleConsequences consequences)
        {
            switch (_drawRule)
            {
                case DrawRule.BothReturnToHand:
                    consequences.WinScoredBy = null;
                    consequences.SetFate(consequences.Host, CreatureFate.ReturnsToHand);
                    consequences.SetFate(consequences.Guest, CreatureFate.ReturnsToHand);
                    consequences.FieldRemoved = consequences.FieldWasOpen;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_drawRule), _drawRule, "Unknown draw rule.");
            }
        }
    }
}
