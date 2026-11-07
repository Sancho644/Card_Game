using System.Linq;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>Returns the spent creatures only when their owner has none in hand or on the table.</summary>
    internal static class CreatureCycleFlow
    {
        public static void ReturnExhaustedPlayers(MatchState state, EventLog events)
        {
            // TODO(rules): Return immediately after the action, but never after the match ends.
            if (state.Phase == MatchPhase.Ended) return;
            foreach (var id in PlayerIds.All)
            {
                var player = state.Player(id);
                if (player.HasCreatureInHand || state.Fields.Any(f => f.OccupantOf(id) != null)) continue;
                foreach (var creature in player.OutOfGame.ToList())
                {
                    player.MoveOutOfGameToHand(creature);
                    events.Emit(new CreatureReturnedToHand(id, creature.Id));
                }
            }
        }
    }
}
