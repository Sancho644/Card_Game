using System;
using Game.Core.Cards;

namespace Game.Core.Decks
{
    public sealed class DeckError
    {
        internal DeckError(DeckErrorCode code, string cardId = null, CardType? type = null, Rarity? rarity = null)
        {
            Code = code;
            CardId = cardId;
            Type = type;
            Rarity = rarity;
        }

        public DeckErrorCode Code { get; }
        public string CardId { get; }
        public CardType? Type { get; }
        public Rarity? Rarity { get; }

        public override string ToString() =>
            Type == null ? $"{Code} {CardId}" : $"{Code} {Type} {Rarity}";
    }
}
