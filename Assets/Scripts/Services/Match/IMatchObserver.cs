using System.Collections.Generic;
using Game.Core.Events;

namespace Game.Services.Match
{
    /// <summary>Receives the events of a match that its player is allowed to see, in order.</summary>
    public interface IMatchObserver
    {
        void Observe(IReadOnlyList<GameEvent> visibleEvents);
    }
}
