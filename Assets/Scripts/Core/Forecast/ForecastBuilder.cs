using System;
using Game.Core.Cards;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.Core.Forecast
{
    /// <summary>
    /// Builds a separate engine for "what if" questions from what one player sees plus a guess about
    /// the rest. The real match state is never read, so a forecast cannot leak hidden information.
    /// </summary>
    public sealed class ForecastBuilder
    {
        private readonly CardCatalog _catalog;
        private readonly GameRules _rules;

        public ForecastBuilder(CardCatalog catalog, GameRules rules)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public MatchEngine Build(PlayerView view, HiddenCardsGuess guess, IRandom random)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (guess == null) throw new ArgumentNullException(nameof(guess));
            if (view.Phase == MatchPhase.NotStarted)
                throw new ArgumentException("A match that has not started cannot be forecast.", nameof(view));

            var state = new ViewStateRestorer(_catalog, view, guess).Restore();
            return new MatchEngine(state, _rules, random);
        }
    }
}
