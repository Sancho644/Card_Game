using System;
using Game.Core.Decks;
using Game.Core.State;

namespace Game.Core
{
    public sealed class MatchSetup
    {
        public MatchSetup(DeckList player1Deck, DeckList player2Deck)
        {
            Player1Deck = player1Deck ?? throw new ArgumentNullException(nameof(player1Deck));
            Player2Deck = player2Deck ?? throw new ArgumentNullException(nameof(player2Deck));
        }

        public DeckList Player1Deck { get; }
        public DeckList Player2Deck { get; }

        public DeckList DeckOf(PlayerId player) => player == PlayerId.Player1 ? Player1Deck : Player2Deck;
    }
}
