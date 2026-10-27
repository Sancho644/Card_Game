using System;
using System.Collections.Generic;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.Services.Match
{
    /// <summary>Runs the rules engine in the same process: hot-seat and play against the AI.</summary>
    public sealed class LocalMatchSession : IMatchSession
    {
        private readonly CardCatalog _catalog;
        private readonly GameRules _rules;
        private MatchEngine _engine;

        public LocalMatchSession(CardCatalog catalog, GameRules rules)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public event Action<IReadOnlyList<GameEvent>> EventsProduced;

        private MatchEngine Engine => _engine ?? throw new InvalidOperationException("The match has not started.");

        public void Start(MatchSetup setup, ulong seed)
        {
            _engine = new MatchEngine(setup, _catalog, _rules, new XorShiftRandom(seed));
            EventsProduced?.Invoke(_engine.Start());
        }

        public CommandResult Submit(GameCommand command)
        {
            var result = Engine.Execute(command);
            if (result.IsAccepted)
                EventsProduced?.Invoke(result.Events);
            return result;
        }

        public PlayerView GetView(PlayerId viewer) => Engine.GetView(viewer);

        public IReadOnlyList<GameCommand> GetLegalActions(PlayerId player) => Engine.GetLegalActions(player);
    }
}
