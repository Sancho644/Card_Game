using Game.Core.State;

namespace Game.Presentation.Match
{
    /// <summary>How a match is played at this screen: two people taking turns, or one person against an opponent elsewhere.</summary>
    public sealed class MatchMode
    {
        private MatchMode(bool showsHandoff, bool offersReactions, string player1NameKey, string player2NameKey)
        {
            ShowsHandoff = showsHandoff;
            OffersReactions = offersReactions;
            Player1NameKey = player1NameKey;
            Player2NameKey = player2NameKey;
        }

        /// <summary>A hand-off screen hides the table whenever the deciding player changes.</summary>
        public bool ShowsHandoff { get; }

        /// <summary>
        /// The local player may act while the opponent decides, if the rules allow it. Off in hot-seat: the second
        /// player cannot see their hand on the other's turn (docs/rules.md §12).
        /// </summary>
        public bool OffersReactions { get; }

        public string Player1NameKey { get; }
        public string Player2NameKey { get; }

        public static MatchMode HotSeat(string player1NameKey, string player2NameKey) =>
            new MatchMode(true, false, player1NameKey, player2NameKey);

        public static MatchMode VersusOpponent(PlayerId localPlayer, string localNameKey, string opponentNameKey) =>
            localPlayer == PlayerId.Player1
                ? new MatchMode(false, true, localNameKey, opponentNameKey)
                : new MatchMode(false, true, opponentNameKey, localNameKey);
    }
}
