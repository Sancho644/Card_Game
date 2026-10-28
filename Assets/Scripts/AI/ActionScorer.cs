using System.Collections.Generic;
using Game.Core.Commands;
using Game.Core.Forecast;
using Game.Core.Randomness;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// Scores each legal action by a one-step forecast: for every guess about the hidden cards the match is
    /// rebuilt from the view, the action is applied and the position is evaluated. The score is the average
    /// gain over the position before the action. No search beyond that one step.
    /// </summary>
    internal sealed class ActionScorer
    {
        private const double RejectedInForecast = -1e6;

        private readonly ForecastBuilder _forecasts;
        private readonly PositionEvaluator _evaluator;

        public ActionScorer(ForecastBuilder forecasts, PositionEvaluator evaluator)
        {
            _forecasts = forecasts;
            _evaluator = evaluator;
        }

        public double[] Score(PlayerView view, IReadOnlyList<GameCommand> actions, IReadOnlyList<HiddenCardsGuess> guesses, IRandom random)
        {
            var me = view.Viewer;
            var scores = new double[actions.Count];
            foreach (var guess in guesses)
            {
                // Every forecast of one guess gets the same random sequence, so actions are compared fairly.
                var seed = (ulong)random.Next(int.MaxValue);
                var baseline = _evaluator.Evaluate(_forecasts.Build(view, guess, new XorShiftRandom(seed)), me);
                for (var i = 0; i < actions.Count; i++)
                {
                    var engine = _forecasts.Build(view, guess, new XorShiftRandom(seed));
                    var result = engine.Execute(actions[i]);
                    scores[i] += result.IsAccepted ? _evaluator.Evaluate(engine, me) - baseline : RejectedInForecast;
                }
            }

            for (var i = 0; i < scores.Length; i++)
                scores[i] /= guesses.Count;
            return scores;
        }
    }
}
