using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Effects;
using Game.Core.Events;
using Game.Core.Forecast;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// Picks one of the legal actions. Sees only the player's view, the events its player may see and the
    /// public card data; it never checks rules itself and never chooses outside the offered list.
    /// All randomness comes from the given generator, so a match can be replayed.
    /// </summary>
    public sealed class AiBrain
    {
        private readonly AiProfile _profile;
        private readonly CardCatalog _catalog;
        private readonly IRandom _random;
        private readonly AiMemory _memory;
        private readonly DeckSlots _slots;
        private readonly ActionScorer _scorer;

        public AiBrain(PlayerId player, AiProfile profile, CardCatalog catalog, GameRules rules, IRandom random)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            Player = player;
            _memory = new AiMemory(player);
            _slots = new DeckSlots(catalog, rules.Deck);
            var values = new CardValues(profile.Weights);
            var evaluator = new PositionEvaluator(profile.Weights, values, new BattleOutlook(profile, rules, values));
            _scorer = new ActionScorer(new ForecastBuilder(catalog, rules), evaluator);
        }

        public PlayerId Player { get; }

        public void Observe(IEnumerable<GameEvent> visibleEvents) => _memory.Observe(visibleEvents);

        /// <summary>The action to take when the match waits for this player.</summary>
        public GameCommand Decide(PlayerView view, IReadOnlyList<GameCommand> legalActions)
        {
            if (legalActions == null || legalActions.Count == 0)
                throw new ArgumentException("There is no legal action to choose from.", nameof(legalActions));
            if (legalActions.Count == 1)
                return legalActions[0];

            var turnOrder = legalActions.OfType<ChooseTurnOrder>().FirstOrDefault(order => order.GoFirst == _profile.PrefersFirstTurn);
            if (turnOrder != null)
                return turnOrder;

            var scores = Score(view, legalActions);
            return legalActions[IndexOfBest(scores)];
        }

        /// <summary>An optional action on the opponent's turn, or null to let it pass.</summary>
        public GameCommand React(PlayerView view, IReadOnlyList<GameCommand> legalActions)
        {
            if (!_profile.UsesReactions || legalActions == null || legalActions.Count == 0)
                return null;

            var scores = Score(view, legalActions);
            var best = IndexOfBest(scores);
            return scores[best] > _profile.ReactionThreshold ? legalActions[best] : null;
        }

        private double[] Score(PlayerView view, IReadOnlyList<GameCommand> actions)
        {
            _memory.ObserveView(view);
            var model = new OpponentModel(view, _memory, _catalog, _slots);
            var guesses = Enumerable.Range(0, Math.Max(1, _profile.Samples)).Select(_ => model.Sample(_random)).ToList();
            var scores = _scorer.Score(view, actions, guesses, _random);

            for (var i = 0; i < actions.Count; i++)
            {
                scores[i] += InformationValue(view, actions[i], model);
                if (_profile.Noise > 0)
                    scores[i] += _profile.Noise * (_random.Next(2001) / 1000.0 - 1);
            }

            return scores;
        }

        /// <summary>A forecast cannot show what scouting teaches, so the AI values each unknown opponent card it would see.</summary>
        private double InformationValue(PlayerView view, GameCommand action, OpponentModel model)
        {
            if (!(action is PlayAbility play))
                return 0;
            var card = view.Hand.FirstOrDefault(hand => hand.InstanceId == play.AbilityId);
            var reveals = card != null &&
                          _catalog.Get(card.CardId) is AbilityCard ability &&
                          ability.Effects.OfType<ScoutEffect>().Any();
            return reveals ? model.UnknownCardCount * _profile.Weights.InformationPerCard : 0;
        }

        private static int IndexOfBest(IReadOnlyList<double> scores)
        {
            var best = 0;
            for (var i = 1; i < scores.Count; i++)
            {
                if (scores[i] > scores[best])
                    best = i;
            }

            return best;
        }
    }
}
