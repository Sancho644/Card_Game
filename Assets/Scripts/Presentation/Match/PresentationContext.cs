using System;
using System.Collections.Generic;
using Game.Core.Events;
using Game.Core.State;
using Game.Presentation.Animation;

namespace Game.Presentation.Match
{
    /// <summary>What an event presenter sees while one batch of events is turned into animation steps.</summary>
    public sealed class PresentationContext
    {
        private readonly AnimationQueue _queue;
        private readonly Func<IAnimationStep> _createSync;
        private bool _lastWasSync;

        public PresentationContext(
            IReadOnlyList<GameEvent> batch,
            PlayerView previousView,
            PlayerView view,
            AnimationQueue queue,
            Func<IAnimationStep> createSync)
        {
            Batch = batch;
            PreviousView = previousView;
            View = view;
            _queue = queue;
            _createSync = createSync;
        }

        public IReadOnlyList<GameEvent> Batch { get; }

        /// <summary>Position of the event being presented in <see cref="Batch"/>.</summary>
        public int Index { get; set; }

        /// <summary>The viewer's view before this batch; null at the start of the match.</summary>
        public PlayerView PreviousView { get; }

        /// <summary>The viewer's view after this batch.</summary>
        public PlayerView View { get; }

        public void Enqueue(IAnimationStep step)
        {
            _lastWasSync = false;
            _queue.Enqueue(step);
        }

        public void Enqueue(Action action) => Enqueue(new ActionStep(action));

        /// <summary>Moves the cards to their places; repeated requests in a row become one.</summary>
        public void EnqueueSync()
        {
            if (_lastWasSync)
                return;
            _queue.Enqueue(_createSync());
            _lastWasSync = true;
        }
    }
}
