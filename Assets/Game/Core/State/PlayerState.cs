using System;
using System.Collections.Generic;
using Game.Core.Cards;

namespace Game.Core.State
{
    public sealed class PlayerState
    {
        private readonly List<CardInstance> _hand = new List<CardInstance>();
        private readonly List<CardInstance> _discard = new List<CardInstance>();
        private readonly List<CardInstance> _outOfGame = new List<CardInstance>();

        internal PlayerState(PlayerId id)
        {
            Id = id;
        }

        public PlayerId Id { get; }
        public IReadOnlyList<CardInstance> Hand => _hand;

        /// <summary>Used abilities.</summary>
        public IReadOnlyList<CardInstance> Discard => _discard;

        /// <summary>Defeated creatures.</summary>
        public IReadOnlyList<CardInstance> OutOfGame => _outOfGame;

        public int Wins { get; internal set; }
        public int LegendariesPlayedThisTurn { get; internal set; }

        public bool HasCreatureInHand => _hand.Exists(card => card.Type == CardType.Creature);

        public CardInstance FindInHand(int instanceId) => _hand.Find(card => card.Id == instanceId);

        internal void AddToHand(CardInstance card) => _hand.Add(card);

        internal void RemoveFromHand(CardInstance card)
        {
            if (!_hand.Remove(card))
                throw new InvalidOperationException($"{card} is not in the hand of {Id}.");
        }

        internal void AddToDiscard(CardInstance card) => _discard.Add(card);

        internal void AddToOutOfGame(CardInstance card) => _outOfGame.Add(card);
    }
}
