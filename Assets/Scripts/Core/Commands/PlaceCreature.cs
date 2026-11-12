using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>
    /// Main action: put a creature from hand on any field where you have no creature yet.
    /// Signature abilities are played separately, after the battle starts.
    /// </summary>
    public sealed class PlaceCreature : GameCommand
    {
        public PlaceCreature(PlayerId player, int creatureId, int fieldId, int? signatureAbilityId = null)
            : base(player)
        {
            CreatureId = creatureId;
            FieldId = fieldId;
            SignatureAbilityId = signatureAbilityId;
        }

        public int CreatureId { get; }
        public int FieldId { get; }
        /// <summary>Legacy command payload; a non-null value is rejected under current rules.</summary>
        public int? SignatureAbilityId { get; }
    }
}
