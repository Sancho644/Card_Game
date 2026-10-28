using System;
using System.Collections.Generic;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.State;
using Game.Services.Match;

namespace Game.AI
{
    /// <summary>
    /// The AI as a player controller: decides at once, then submits after a short "thinking" delay so a
    /// human can follow its moves. The delay has its own random generator and never changes the decisions.
    /// </summary>
    public sealed class AiController : IPlayerController, IMatchObserver
    {
        private readonly AiBrain _brain;
        private readonly AiProfile _profile;
        private readonly IActionScheduler _scheduler;
        private readonly IRandom _delayRandom;
        private IDisposable _pending;

        public AiController(AiBrain brain, AiProfile profile, IActionScheduler scheduler, IRandom delayRandom)
        {
            _brain = brain ?? throw new ArgumentNullException(nameof(brain));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _delayRandom = delayRandom ?? throw new ArgumentNullException(nameof(delayRandom));
        }

        public PlayerId Player => _brain.Player;

        public void Observe(IReadOnlyList<GameEvent> visibleEvents) => _brain.Observe(visibleEvents);

        public void RequestDecision(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit) =>
            SubmitLater(_brain.Decide(view, legalActions), submit);

        public void OfferReaction(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit)
        {
            var reaction = _brain.React(view, legalActions);
            if (reaction != null)
                SubmitLater(reaction, submit);
        }

        public void CancelDecision() => DropPending();

        public void AbandonDecision() => DropPending();

        private void SubmitLater(GameCommand command, Action<GameCommand> submit)
        {
            DropPending();
            _pending = _scheduler.Schedule(ThinkDelay(), () =>
            {
                _pending = null;
                submit(command);
            });
        }

        private float ThinkDelay()
        {
            var spread = Math.Max(0f, _profile.ThinkDelayMax - _profile.ThinkDelayMin);
            return _profile.ThinkDelayMin + spread * _delayRandom.Next(1001) / 1000f;
        }

        private void DropPending()
        {
            _pending?.Dispose();
            _pending = null;
        }
    }
}
