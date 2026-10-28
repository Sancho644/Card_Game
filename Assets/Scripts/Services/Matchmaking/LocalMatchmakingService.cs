using System;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;
using Game.Services.Match;

namespace Game.Services.Matchmaking
{
    /// <summary>
    /// Matchmaking stub: after a short pretend search it creates a local match against an opponent from the
    /// factory, with a random valid deck. The player takes the first seat; the coin decides who goes first.
    /// </summary>
    public sealed class LocalMatchmakingService : IMatchmakingService
    {
        private const PlayerId LocalSeat = PlayerId.Player1;

        private readonly CardCatalog _catalog;
        private readonly GameRules _rules;
        private readonly IOpponentFactory _opponents;
        private readonly IActionScheduler _scheduler;
        private readonly IRandom _random;
        private readonly RandomDeckBuilder _decks;
        private readonly float _searchSecondsMin;
        private readonly float _searchSecondsMax;
        private IDisposable _search;

        public LocalMatchmakingService(
            CardCatalog catalog,
            GameRules rules,
            IOpponentFactory opponents,
            IActionScheduler scheduler,
            IRandom random,
            float searchSecondsMin,
            float searchSecondsMax)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _opponents = opponents ?? throw new ArgumentNullException(nameof(opponents));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _decks = new RandomDeckBuilder(catalog, rules.Deck);
            _searchSecondsMin = searchSecondsMin;
            _searchSecondsMax = Math.Max(searchSecondsMin, searchSecondsMax);
        }

        public void FindMatch(MatchRequest request, Action<MatchFound> found)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (found == null) throw new ArgumentNullException(nameof(found));

            Cancel();
            var delay = _searchSecondsMin + (_searchSecondsMax - _searchSecondsMin) * _random.Next(1001) / 1000f;
            _search = _scheduler.Schedule(delay, () =>
            {
                _search = null;
                found(CreateMatch(request));
            });
        }

        public void Cancel()
        {
            _search?.Dispose();
            _search = null;
        }

        private MatchFound CreateMatch(MatchRequest request)
        {
            var setup = new MatchSetup(request.Deck, _decks.Build(_random));
            var session = new LocalMatchSession(_catalog, _rules);
            var opponent = _opponents.Attach(session, LocalSeat.Opponent(), request.OpponentDifficulty, NextSeed());
            return new MatchFound(session, LocalSeat, setup, NextSeed(), opponent);
        }

        private ulong NextSeed() => ((ulong)(uint)_random.Next(int.MaxValue) << 31) ^ (uint)_random.Next(int.MaxValue);
    }
}
