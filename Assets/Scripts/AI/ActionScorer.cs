using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Forecast;
using Game.Core.Randomness;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// Scores each legal action by a forecast: for every guess about the hidden cards the match is rebuilt
    /// from the view, the action is applied and the position is evaluated. The score is the average gain
    /// over the position before the action. If the profile considers replies and the action leaves the
    /// opponent to act (in a battle, or on its next turn once the AI's main action is done), the opponent's
    /// answer worst for the AI is applied too; the opponent knows its own hand in the guess.
    /// </summary>
    internal sealed class ActionScorer
    {
        private const double RejectedInForecast = -1e6;

        private readonly ForecastBuilder _forecasts;
        private readonly PositionEvaluator _evaluator;
        private readonly bool _considersReplies;

        public ActionScorer(ForecastBuilder forecasts, PositionEvaluator evaluator, bool considersReplies)
        {
            _forecasts = forecasts;
            _evaluator = evaluator;
            _considersReplies = considersReplies;
        }

        public double[] Score(PlayerView view, IReadOnlyList<GameCommand> actions, IReadOnlyList<HiddenCardsGuess> guesses, IRandom random)
        {
            var me = view.Viewer;
            var scores = new double[actions.Count];
            foreach (var guess in guesses)
            {
                // Every forecast of one guess gets the same random sequence, so actions are compared fairly.
                var seed = (ulong)random.Next(int.MaxValue);
                MatchEngine Build() => _forecasts.Build(view, guess, new XorShiftRandom(seed));

                var baseline = _evaluator.Evaluate(Build(), me);
                for (var i = 0; i < actions.Count; i++)
                {
                    var outcome = Outcome(Build, actions[i], me);
                    scores[i] += outcome.HasValue ? outcome.Value - baseline : RejectedInForecast;
                }
            }

            for (var i = 0; i < scores.Length; i++)
                scores[i] /= guesses.Count;
            return scores;
        }

        /// <summary>The value of the position after the action and, if considered, the opponent's answer; null if rejected.</summary>
        private double? Outcome(Func<MatchEngine> build, GameCommand action, PlayerId me)
        {
            var engine = build();
            if (!engine.Execute(action).IsAccepted)
                return null;
            if (!_considersReplies)
                return _evaluator.Evaluate(engine, me);

            // Once the main action is done the turn passes: the end of the turn is applied to see the opponent's answer.
            var endsTurn = IsMainActionDone(engine, me) && engine.Execute(new EndTurn(me)).IsAccepted;
            var replies = Replies(engine, me);
            if (replies.Count == 0)
                return _evaluator.Evaluate(engine, me);

            double? worst = null;
            foreach (var reply in replies)
            {
                var next = build();
                next.Execute(action);
                if (endsTurn)
                    next.Execute(new EndTurn(me));
                if (!next.Execute(reply).IsAccepted)
                    continue;
                var value = _evaluator.Evaluate(next, me);
                if (worst == null || value < worst)
                    worst = value;
            }

            return worst ?? _evaluator.Evaluate(engine, me);
        }

        /// <summary>
        /// The opponent's answers worth checking: anything it may do on its turn in a battle, or the start of its
        /// next turn: its main action (placing a creature or challenging) or an ability before it, such as moving
        /// a creature into a battle.
        /// </summary>
        private static IReadOnlyList<GameCommand> Replies(MatchEngine engine, PlayerId me)
        {
            var state = engine.State;
            var opponent = me.Opponent();
            if (state.Phase == MatchPhase.Battle && state.Battle.CurrentActor == opponent)
                return engine.GetLegalActions(opponent);
            if (state.Phase == MatchPhase.Turn && state.ActivePlayer == opponent && !state.MainActionDone)
                return engine.GetLegalActions(opponent).Where(reply => !(reply is EndTurn)).ToList();
            return new GameCommand[0];
        }

        private static bool IsMainActionDone(MatchEngine engine, PlayerId me) =>
            engine.State.Phase == MatchPhase.Turn && engine.State.ActivePlayer == me && engine.State.MainActionDone;
    }
}
