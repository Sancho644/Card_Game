using Game.Core.State;

namespace Game.Core.Battle
{
    public sealed class BattleResolution
    {
        internal BattleResolution(
            PowerBreakdown host,
            PowerBreakdown guest,
            ComparisonRule rule,
            TieBreak tieBreak,
            PlayerId? winner,
            BattleConsequences consequences)
        {
            Host = host;
            Guest = guest;
            Rule = rule;
            TieBreak = tieBreak;
            Winner = winner;
            Consequences = consequences;
        }

        public PowerBreakdown Host { get; }
        public PowerBreakdown Guest { get; }
        public ComparisonRule Rule { get; }

        /// <summary>How equal final power was decided, if it was.</summary>
        public TieBreak TieBreak { get; }

        public PlayerId? Winner { get; }
        public BattleConsequences Consequences { get; }
    }
}
