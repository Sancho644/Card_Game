namespace Game.Simulation
{
    /// <summary>One card across a simulation: counted per deck that contained it ("deck-games").</summary>
    public sealed class CardStats
    {
        internal CardStats(string cardId)
        {
            CardId = cardId;
        }

        public string CardId { get; }

        /// <summary>Completed matches in which a deck held this card (a match with it in both decks counts twice).</summary>
        public int DeckGames { get; internal set; }

        /// <summary>Of those, how many times the deck used the card at least once.</summary>
        public int GamesUsed { get; internal set; }

        public int TotalUses { get; internal set; }

        /// <summary>Of those deck-games, how many the deck won.</summary>
        public int DeckWins { get; internal set; }

        /// <summary>Deck-games of matches with a winner; drawn matches are left out of the win share.</summary>
        public int DecidedDeckGames { get; internal set; }

        public double UsedShare => DeckGames == 0 ? 0 : (double)GamesUsed / DeckGames;
        public double UsesPerGame => DeckGames == 0 ? 0 : (double)TotalUses / DeckGames;

        /// <summary>Share of deck-games won, among matches with a winner.</summary>
        public ShareEstimate WinShare => new ShareEstimate(DeckWins, DecidedDeckGames);
    }
}
