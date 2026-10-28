using System;

namespace Game.Services.Matchmaking
{
    /// <summary>
    /// Finds an opponent and prepares a match. The local stub makes a match against the AI at once; a network
    /// service will queue the player on a server behind the same interface.
    /// </summary>
    public interface IMatchmakingService
    {
        /// <summary>Starts searching; <paramref name="found"/> is called once, later, unless the search is cancelled.</summary>
        void FindMatch(MatchRequest request, Action<MatchFound> found);

        void Cancel();
    }
}
