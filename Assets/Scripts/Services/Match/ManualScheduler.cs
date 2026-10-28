using System;
using System.Collections.Generic;

namespace Game.Services.Match
{
    /// <summary>Queues actions and ignores delays; <see cref="RunPending"/> runs them in order. For simulations and tests.</summary>
    public sealed class ManualScheduler : IActionScheduler
    {
        private readonly Queue<ScheduledAction> _queue = new Queue<ScheduledAction>();

        public bool HasPending => _queue.Count > 0;

        public IDisposable Schedule(float delaySeconds, Action action)
        {
            var scheduled = new ScheduledAction(action ?? throw new ArgumentNullException(nameof(action)));
            _queue.Enqueue(scheduled);
            return scheduled;
        }

        /// <summary>Runs queued actions, including ones they schedule, until the queue is empty or the limit is hit.</summary>
        /// <returns>How many actions ran.</returns>
        public int RunPending(int limit = int.MaxValue)
        {
            var ran = 0;
            while (_queue.Count > 0 && ran < limit)
            {
                if (_queue.Dequeue().TryRun())
                    ran++;
            }

            return ran;
        }
    }
}
