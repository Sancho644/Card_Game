using System.Collections.Generic;

namespace Game.Core.State
{
    /// <summary>Seat at the table. Not the turn order: who moves first is decided by the coin toss.</summary>
    public enum PlayerId
    {
        Player1 = 0,
        Player2 = 1,
    }

    public static class PlayerIds
    {
        public static readonly IReadOnlyList<PlayerId> All = new[] { PlayerId.Player1, PlayerId.Player2 };

        public static PlayerId Opponent(this PlayerId player) =>
            player == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
    }
}
