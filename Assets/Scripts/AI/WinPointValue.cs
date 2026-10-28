using Game.Core.Battle;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>Every battle win is worth the same; reaching the wins needed adds the match win.</summary>
    internal sealed class WinPointValue : IMatchScoreValue
    {
        private readonly AiWeights _weights;
        private readonly GameRules _rules;

        public WinPointValue(AiWeights weights, GameRules rules)
        {
            _weights = weights;
            _rules = rules;
        }

        public double Value(MatchState state, PlayerId me, BattleConsequences after = null) =>
            Side(state, me, after) - Side(state, me.Opponent(), after);

        private double Side(MatchState state, PlayerId player, BattleConsequences after)
        {
            var wins = state.Player(player).Wins + (after?.WinScoredBy == player ? 1 : 0);
            return wins * _weights.WinPoint + (wins >= _rules.WinsToWin ? _weights.MatchWin : 0);
        }
    }
}
