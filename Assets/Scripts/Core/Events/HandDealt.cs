using System.Collections.Generic;
using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>A player's starting hand. Private to that player.</summary>
    public sealed class HandDealt : GameEvent
    {
        public HandDealt(PlayerId owner, IReadOnlyList<CardRef> cards)
            : base(owner)
        {
            Owner = owner;
            Cards = cards;
        }

        public PlayerId Owner { get; }
        public IReadOnlyList<CardRef> Cards { get; }
    }
}
