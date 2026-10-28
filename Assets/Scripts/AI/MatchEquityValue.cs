using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// The chance to win the match from the score and the creatures each player still has, as if every later
    /// battle were a coin flip in which the winner scores and keeps its creature and the loser's creature is
    /// out. A player without creatures cannot fight, and the match goes to the leader by wins, then by creatures
    /// left (docs/rules.md §9). A sure win is worth +MatchWin, a sure loss −MatchWin. Unlike flat win points,
    /// this sees that a creature lost late can cost the match and that the last win is worth more than the first.
    /// </summary>
    internal sealed class MatchEquityValue : IMatchScoreValue
    {
        private readonly AiWeights _weights;
        private readonly GameRules _rules;
        private readonly Dictionary<(int, int, int, int), double> _chances = new Dictionary<(int, int, int, int), double>();

        public MatchEquityValue(AiWeights weights, GameRules rules)
        {
            _weights = weights;
            _rules = rules;
        }

        public double Value(MatchState state, PlayerId me, BattleConsequences after = null)
        {
            var opponent = me.Opponent();
            var chance = Chance(Wins(state, me, after), Wins(state, opponent, after), Creatures(state, me, after), Creatures(state, opponent, after));
            return _weights.MatchWin * (2 * chance - 1);
        }

        private static int Wins(MatchState state, PlayerId player, BattleConsequences after) =>
            state.Player(player).Wins + (after?.WinScoredBy == player ? 1 : 0);

        /// <summary>Creatures in hand and on the table, less the one the battle puts out of the game.</summary>
        private static int Creatures(MatchState state, PlayerId player, BattleConsequences after)
        {
            var count = state.Player(player).Hand.Count(card => card.Type == CardType.Creature) +
                        state.Fields.Count(field => field.OccupantOf(player) != null);
            return after?.FateOf(player) == CreatureFate.OutOfGame ? count - 1 : count;
        }

        private double Chance(int myWins, int opponentWins, int myCreatures, int opponentCreatures)
        {
            if (myWins >= _rules.WinsToWin)
                return 1;
            if (opponentWins >= _rules.WinsToWin)
                return 0;
            if (myCreatures <= 0 || opponentCreatures <= 0)
            {
                if (myWins != opponentWins)
                    return myWins > opponentWins ? 1 : 0;
                return myCreatures > opponentCreatures ? 1 : myCreatures < opponentCreatures ? 0 : 0.5;
            }

            var key = (myWins, opponentWins, myCreatures, opponentCreatures);
            if (_chances.TryGetValue(key, out var chance))
                return chance;

            chance = 0.5 * Chance(myWins + 1, opponentWins, myCreatures, opponentCreatures - 1) +
                     0.5 * Chance(myWins, opponentWins + 1, myCreatures - 1, opponentCreatures);
            _chances[key] = chance;
            return chance;
        }
    }
}
