using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Services.Match
{
    /// <summary>
    /// Connects one player's controller to a session: passes on the events that player may see, asks for a
    /// decision when the match waits for them and offers reactions otherwise. Every event batch cancels the
    /// pending decision, which is then asked again for the new state. Used for the AI opponent and simulations.
    /// </summary>
    public sealed class SessionPlayerHost : IDisposable
    {
        private readonly IMatchSession _session;
        private readonly IPlayerController _controller;
        private readonly IActionScheduler _scheduler;
        private readonly bool _offersReactions;
        private IDisposable _pendingEvaluation;
        private bool _disposed;

        public SessionPlayerHost(IMatchSession session, IPlayerController controller, IActionScheduler scheduler, bool offersReactions)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _offersReactions = offersReactions;
            _session.EventsProduced += OnEvents;
        }

        /// <summary>The engine refused a command of this controller. Should not happen: controllers choose from legal actions.</summary>
        public event Action<GameCommand, RejectReason> CommandRejected;

        public PlayerId Player => _controller.Player;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _session.EventsProduced -= OnEvents;
            _pendingEvaluation?.Dispose();
            _controller.AbandonDecision();
        }

        private void OnEvents(IReadOnlyList<GameEvent> batch)
        {
            if (_disposed)
                return;

            _controller.CancelDecision();
            if (_controller is IMatchObserver observer)
                observer.Observe(batch.Where(gameEvent => gameEvent.IsVisibleTo(Player)).ToList());

            _pendingEvaluation?.Dispose();
            _pendingEvaluation = _scheduler.Schedule(0f, Evaluate);
        }

        private void Evaluate()
        {
            _pendingEvaluation = null;
            if (_disposed)
                return;

            var view = _session.GetView(Player);
            if (view.Phase == MatchPhase.Ended || view.AwaitingPlayer == null)
                return;

            var legal = _session.GetLegalActions(Player);
            if (view.AwaitingPlayer == Player)
                _controller.RequestDecision(view, legal, Submit);
            else if (_offersReactions && legal.Count > 0)
                _controller.OfferReaction(view, legal, Submit);
        }

        private void Submit(GameCommand command)
        {
            if (_disposed)
                return;

            var result = _session.Submit(command);
            if (result.IsAccepted)
                return;

            CommandRejected?.Invoke(command, result.Reason);
            if (!_disposed)
                _pendingEvaluation = _scheduler.Schedule(0f, Evaluate);
        }
    }
}
