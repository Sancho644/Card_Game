using System.Collections.Generic;
using Game.Core.State;
using Game.Presentation.Cards;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Names of card instances the current viewer has seen. Events carry instance ids; the names come
    /// from what the viewer's own <see cref="PlayerView"/> showed, so hidden cards stay hidden.
    /// </summary>
    public sealed class CardNameLookup
    {
        private readonly CardFaceBuilder _faces;
        private readonly Dictionary<int, string> _cardIds = new Dictionary<int, string>();

        public CardNameLookup(CardFaceBuilder faces)
        {
            _faces = faces;
        }

        public void Remember(CardRef card)
        {
            if (card?.CardId != null)
                _cardIds[card.InstanceId] = card.CardId;
        }

        public void Remember(PlayerView view)
        {
            foreach (var card in view.Hand)
                Remember(card);
            foreach (var field in view.Fields)
            {
                Remember(new CardRef(field.Id, field.CardId));
                foreach (var owner in PlayerIds.All)
                    Remember(field.Occupant(owner));
            }

            foreach (var owner in PlayerIds.All)
            {
                foreach (var card in view.Discard(owner))
                    Remember(card);
                foreach (var card in view.OutOfGame(owner))
                    Remember(card);
            }
        }

        public void Clear() => _cardIds.Clear();

        public string CardIdOf(int instanceId) => _cardIds.TryGetValue(instanceId, out var cardId) ? cardId : null;

        public string NameOf(int instanceId) => _faces.NameOf(CardIdOf(instanceId));
    }
}
