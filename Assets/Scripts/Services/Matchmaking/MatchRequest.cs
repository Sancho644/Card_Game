using System;
using Game.Core.Decks;

namespace Game.Services.Matchmaking
{
    /// <summary>What the player asks the matchmaking for: their deck and, for a local opponent, its difficulty.</summary>
    public sealed class MatchRequest
    {
        public MatchRequest(DeckList deck, string opponentDifficulty)
        {
            Deck = deck ?? throw new ArgumentNullException(nameof(deck));
            OpponentDifficulty = opponentDifficulty;
        }

        public DeckList Deck { get; }

        /// <summary>AI profile id for a local opponent; a network service ignores it.</summary>
        public string OpponentDifficulty { get; }
    }
}
