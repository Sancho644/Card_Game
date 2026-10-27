using Game.Core.State;

namespace Game.Core.Battle
{
    public sealed class BattleResolution
    {
        internal BattleResolution(
            PowerBreakdown host,
            PowerBreakdown guest,
            ComparisonRule rule,
            bool tieBroken,
            PlayerId? winner,
            BattleConsequences consequences)
        {
            Host = host;
            Guest = guest;
            Rule = rule;
            TieBroken = tieBroken;
            Winner = winner;
            Consequences = consequences;
        }

        public PowerBreakdown Host { get; }
        public PowerBreakdown Guest { get; }
        public ComparisonRule Rule { get; }

        /// <summary>Powers were equal and an effect («Перевес») gave the win to one side.</summary>
        public bool TieBroken { get; }

        public PlayerId? Winner { get; }
        public BattleConsequences Consequences { get; }
    }
}
