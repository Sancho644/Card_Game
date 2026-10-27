using System.Collections.Generic;
using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>A player's field cards are put face down on the table.</summary>
    public sealed class FieldsDealt : GameEvent
    {
        public FieldsDealt(PlayerId owner, IReadOnlyList<int> fieldIds)
        {
            Owner = owner;
            FieldIds = fieldIds;
        }

        public PlayerId Owner { get; }
        public IReadOnlyList<int> FieldIds { get; }
    }
}
