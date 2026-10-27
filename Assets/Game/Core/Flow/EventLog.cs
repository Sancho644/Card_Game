using System.Collections.Generic;
using Game.Core.Events;

namespace Game.Core.Flow
{
    /// <summary>Collects events produced while one command (or the match start) is processed.</summary>
    internal sealed class EventLog
    {
        private readonly List<GameEvent> _pending = new List<GameEvent>();

        public void Emit(GameEvent gameEvent) => _pending.Add(gameEvent);

        public IReadOnlyList<GameEvent> Drain()
        {
            var events = _pending.ToArray();
            _pending.Clear();
            return events;
        }
    }
}
