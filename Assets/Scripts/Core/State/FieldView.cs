using System.Linq;

namespace Game.Core.State
{
    public sealed class FieldView
    {
        private readonly CardRef[] _occupants;

        internal FieldView(FieldSlot field, PlayerId viewer)
        {
            Id = field.Id;
            Owner = field.Owner;
            IsOpen = field.IsOpen;
            IsRemoved = field.IsRemoved;
            IsLocked = field.IsLocked;
            // A closed field removed after a battle stays hidden from the opponent (docs/rules.md §6).
            CardId = field.IsOpen || field.Owner == viewer ? field.Definition.Id : null;
            _occupants = PlayerIds.All.Select(id => CardRef.From(field.OccupantOf(id))).ToArray();
        }

        public int Id { get; }
        public PlayerId Owner { get; }

        /// <summary>Card id, or null when the viewer may not see it.</summary>
        public string CardId { get; }

        public bool IsOpen { get; }
        public bool IsRemoved { get; }
        public bool IsLocked { get; }

        /// <summary>Creatures stand face up, so both players see them.</summary>
        public CardRef Occupant(PlayerId player) => _occupants[(int)player];
    }
}
