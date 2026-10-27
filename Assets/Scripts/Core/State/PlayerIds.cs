using System.Collections.Generic;

namespace Game.Core.State
{
    public static class PlayerIds
    {
        public static readonly IReadOnlyList<PlayerId> All = new[] { PlayerId.Player1, PlayerId.Player2 };

        public static PlayerId Opponent(this PlayerId player) =>
            player == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
    }
}
