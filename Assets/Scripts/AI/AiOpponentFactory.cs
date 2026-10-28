using System;
using Game.Core.Cards;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;
using Game.Services.Match;
using Game.Services.Matchmaking;

namespace Game.AI
{
    /// <summary>Connects an AI player to a session; it acts with its think delay and reacts out of turn.</summary>
    public sealed class AiOpponentFactory : IOpponentFactory
    {
        private readonly CardCatalog _catalog;
        private readonly GameRules _rules;
        private readonly Func<string, AiProfile> _profiles;
        private readonly IActionScheduler _scheduler;

        public AiOpponentFactory(CardCatalog catalog, GameRules rules, Func<string, AiProfile> profiles, IActionScheduler scheduler)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public IDisposable Attach(IMatchSession session, PlayerId player, string difficulty, ulong seed)
        {
            var profile = _profiles(difficulty);
            var brain = new AiBrain(player, profile, _catalog, _rules, new XorShiftRandom(seed));
            var controller = new AiController(brain, profile, _scheduler, new XorShiftRandom(seed ^ 0x5DEECE66DUL));
            return new SessionPlayerHost(session, controller, _scheduler, offersReactions: true);
        }
    }
}
