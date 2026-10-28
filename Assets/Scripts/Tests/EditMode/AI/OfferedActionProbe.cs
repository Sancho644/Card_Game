using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;
using Game.Services.Match;

namespace Game.AI.Tests
{
    /// <summary>Wraps a controller and records every submitted command that was not one of the offered actions.</summary>
    internal sealed class OfferedActionProbe : IPlayerController, IMatchObserver
    {
        private readonly IPlayerController _inner;

        public OfferedActionProbe(IPlayerController inner)
        {
            _inner = inner;
        }

        public List<string> Violations { get; } = new List<string>();
        public int Submitted { get; private set; }
        public PlayerId Player => _inner.Player;

        public void Observe(IReadOnlyList<GameEvent> visibleEvents) => (_inner as IMatchObserver)?.Observe(visibleEvents);

        public void RequestDecision(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit) =>
            _inner.RequestDecision(view, legalActions, Checked(legalActions, submit));

        public void OfferReaction(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit) =>
            _inner.OfferReaction(view, legalActions, Checked(legalActions, submit));

        public void CancelDecision() => _inner.CancelDecision();

        public void AbandonDecision() => _inner.AbandonDecision();

        private Action<GameCommand> Checked(IReadOnlyList<GameCommand> offered, Action<GameCommand> submit) => command =>
        {
            Submitted++;
            if (!offered.Any(action => ReferenceEquals(action, command)))
                Violations.Add($"{command.GetType().Name} by {command.Player} was not offered");
            submit(command);
        };
    }
}
