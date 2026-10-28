using System;
using System.Collections.Generic;

namespace Game.Core.Forecast
{
    /// <summary>
    /// An assumption about what a player cannot see: the cards in the opponent's hand and the
    /// content of the field cards still hidden from them. Used to rebuild a match from a player view.
    /// </summary>
    public sealed class HiddenCardsGuess
    {
        public HiddenCardsGuess(IReadOnlyList<string> opponentHand, IReadOnlyDictionary<int, string> hiddenFields)
        {
            OpponentHand = opponentHand ?? throw new ArgumentNullException(nameof(opponentHand));
            HiddenFields = hiddenFields ?? throw new ArgumentNullException(nameof(hiddenFields));
        }

        /// <summary>Card ids, as many as the opponent holds.</summary>
        public IReadOnlyList<string> OpponentHand { get; }

        /// <summary>Card id for every field the viewer cannot see, by field id.</summary>
        public IReadOnlyDictionary<int, string> HiddenFields { get; }
    }
}
