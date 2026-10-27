using System;
using Game.Core.Cards;

namespace Game.Core.State
{
    /// <summary>A field card on the table and the creatures standing on it (at most one per player).</summary>
    public sealed class FieldSlot
    {
        private readonly CardInstance[] _occupants = new CardInstance[PlayerIds.All.Count];
        private readonly int[] _placementOrder = new int[PlayerIds.All.Count];

        internal FieldSlot(CardInstance card)
        {
            Card = card;
            Definition = (FieldCard)card.Definition;
        }

        public CardInstance Card { get; }
        public FieldCard Definition { get; }
        public int Id => Card.Id;
        public PlayerId Owner => Card.Owner;
        public bool IsOpen { get; internal set; }
        public bool IsRemoved { get; internal set; }

        public bool IsContested => _occupants[0] != null && _occupants[1] != null;

        /// <summary>The player whose creature has stood on this field longer. Valid when contested.</summary>
        public PlayerId EarlierOccupant =>
            _placementOrder[(int)PlayerId.Player1] < _placementOrder[(int)PlayerId.Player2]
                ? PlayerId.Player1
                : PlayerId.Player2;

        public CardInstance OccupantOf(PlayerId player) => _occupants[(int)player];

        internal void Occupy(PlayerId player, CardInstance creature, int placementOrder)
        {
            if (_occupants[(int)player] != null)
                throw new InvalidOperationException($"{player} already has a creature on field #{Id}.");

            _occupants[(int)player] = creature;
            _placementOrder[(int)player] = placementOrder;
        }

        internal CardInstance Vacate(PlayerId player)
        {
            var creature = _occupants[(int)player];
            _occupants[(int)player] = null;
            return creature;
        }
    }
}
