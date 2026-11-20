using System.Collections.Generic;
using System.Linq;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Presentation.Match
{
    /// <summary>Confirmed arrivals in the public discard piles for one batch.</summary>
    public static class DiscardAudio
    {
        public static HashSet<int> NewCards(IReadOnlyList<GameEvent> batch, PlayerView previous, PlayerView current)
        {
            var arrivals = new HashSet<int>();
            if (previous == null)
                return arrivals;

            foreach (var gameEvent in batch)
            {
                switch (gameEvent)
                {
                    case AbilityPlayed played:
                        AddArrival(played.Player, played.Ability);
                        break;
                    case AbilityDiscarded discarded:
                        AddArrival(discarded.Owner, discarded.Ability);
                        break;
                }
            }

            return arrivals;

            void AddArrival(PlayerId owner, CardRef card)
            {
                if (current.Discard(owner).Any(c => c.InstanceId == card.InstanceId) &&
                    !previous.Discard(owner).Any(c => c.InstanceId == card.InstanceId))
                    arrivals.Add(card.InstanceId);
            }
        }
    }
}
