using System;
using System.Collections.Generic;

namespace Game.Presentation.Animation
{
    /// <summary>
    /// Plays presentation steps strictly one after another. While it is busy the table ignores input.
    /// </summary>
    public sealed class AnimationQueue
    {
        private readonly Queue<IAnimationStep> _steps = new Queue<IAnimationStep>();
        private IAnimationStep _current;
        private bool _pumping;

        public event Action Idle;

        public bool IsBusy => _current != null || _steps.Count > 0;

        public void Enqueue(IAnimationStep step)
        {
            _steps.Enqueue(step ?? throw new ArgumentNullException(nameof(step)));
            Pump();
        }

        public void Enqueue(Action action) => Enqueue(new ActionStep(action));

        /// <summary>Drops waiting steps and finishes the current one at once.</summary>
        public void Clear()
        {
            _steps.Clear();
            var current = _current;
            _current = null;
            current?.Complete();
        }

        private void Pump()
        {
            if (_pumping)
                return;

            _pumping = true;
            while (_current == null && _steps.Count > 0)
            {
                var step = _steps.Dequeue();
                _current = step;
                step.Play(() => OnStepDone(step));
            }

            _pumping = false;
            if (!IsBusy)
                Idle?.Invoke();
        }

        private void OnStepDone(IAnimationStep step)
        {
            if (_current != step)
                return;

            _current = null;
            Pump();
        }
    }
}
