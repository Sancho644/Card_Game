using System;
using Game.Core;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// Scores a forecast position for one player: the score, or the expected result of the battle in progress,
    /// plus own features minus the opponent's. Features: creatures still in play, abilities in hand,
    /// field cards on the table and how well waiting creatures suit their fields.
    /// </summary>
    internal sealed class PositionEvaluator
    {
        private readonly AiWeights _weights;
        private readonly CardValues _values;
        private readonly IMatchScoreValue _score;
        private readonly BattleOutlook _battle;

        public PositionEvaluator(AiWeights weights, CardValues values, IMatchScoreValue score, BattleOutlook battle)
        {
            _weights = weights;
            _values = values;
            _score = score;
            _battle = battle;
        }

        public double Evaluate(MatchEngine engine, PlayerId me)
        {
            var state = engine.State;
            if (state.Phase == MatchPhase.Ended)
            {
                var winner = state.Result?.Winner;
                return winner == null ? 0 : winner == me ? _weights.MatchWin : -_weights.MatchWin;
            }

            var value = Side(state, me) - Side(state, me.Opponent());
            return value + (state.Battle != null ? _battle.Expectation(engine, me) : _score.Value(state, me));
        }

        private double Side(MatchState state, PlayerId player)
        {
            var own = state.Player(player);
            double value = 0;
            foreach (var card in own.Hand)
                value += card.Type == CardType.Creature ? _values.Creature(card.Definition) : _values.AbilityInHand(card, own);

            foreach (var field in state.Fields)
            {
                var creature = field.OccupantOf(player);
                if (creature != null)
                {
                    value += _values.Creature(creature.Definition);
                    if (state.Battle?.Field != field)
                        value += Affinity(field, player, (CreatureCard)creature.Definition) * _weights.FieldAffinityPerPower;
                }

                if (field.Owner == player && !field.IsRemoved)
                    value += _weights.FieldCard - (field.IsLocked && !field.IsOpen ? _weights.LockedField : 0);
            }

            return value;
        }

        /// <summary>
        /// What the field may add to a waiting creature in a future battle: an open field gives its value,
        /// the owner opens a closed field only when it helps, the other player opens it when it hurts.
        /// </summary>
        private static double Affinity(FieldSlot field, PlayerId player, CreatureCard creature)
        {
            field.Definition.Modifiers.TryGetValue(creature.Element, out var value);
            if (field.IsOpen)
                return value;
            if (field.Owner == player)
                return field.IsLocked ? 0 : Math.Max(0, value);
            return Math.Min(0, value) / 2.0;
        }
    }
}
