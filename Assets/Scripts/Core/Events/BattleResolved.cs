using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class BattleResolved : GameEvent
    {
        public BattleResolved(
            int fieldId,
            PowerBreakdown host,
            PowerBreakdown guest,
            ComparisonRule rule,
            TieBreak tieBreak,
            PlayerId? winner)
        {
            FieldId = fieldId;
            Host = host;
            Guest = guest;
            Rule = rule;
            TieBreak = tieBreak;
            Winner = winner;
        }

        public int FieldId { get; }

        /// <summary>The host's power by the steps of the count.</summary>
        public PowerBreakdown Host { get; }

        public PowerBreakdown Guest { get; }
        public int HostPower => Host.FinalPower;
        public int GuestPower => Guest.FinalPower;
        public ComparisonRule Rule { get; }

        /// <summary>How equal final power was decided: by placement order or by «Перевес».</summary>
        public TieBreak TieBreak { get; }

        /// <summary>Null on a draw.</summary>
        public PlayerId? Winner { get; }
    }
}
