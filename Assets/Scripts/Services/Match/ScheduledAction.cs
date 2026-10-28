using System;

namespace Game.Services.Match
{
    /// <summary>An action waiting in a <see cref="ManualScheduler"/>; disposing cancels it.</summary>
    public sealed class ScheduledAction : IDisposable
    {
        private Action _action;

        public ScheduledAction(Action action)
        {
            _action = action;
        }

        public void Dispose() => _action = null;

        /// <summary>Runs the action once, unless it was cancelled. False if there was nothing to run.</summary>
        public bool TryRun()
        {
            var action = _action;
            _action = null;
            action?.Invoke();
            return action != null;
        }
    }
}
