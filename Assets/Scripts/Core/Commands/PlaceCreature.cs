using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>
    /// Main action: put a creature from hand on any field where you have no creature yet.
    /// If this starts a battle, the creature's signature ability may be played with it, before the battle.
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
        public int? SignatureAbilityId { get; }
    }
}
