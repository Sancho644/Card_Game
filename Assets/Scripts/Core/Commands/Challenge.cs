using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>Main action with no creatures in hand: move your creature to a field with an opponent's creature.</summary>
    public sealed class Challenge : GameCommand
    {
        public Challenge(PlayerId player, int creatureId, int targetFieldId, int? signatureAbilityId = null)
            : base(player)
        {
            CreatureId = creatureId;
            TargetFieldId = targetFieldId;
            SignatureAbilityId = signatureAbilityId;
        }

        public int CreatureId { get; }
        public int TargetFieldId { get; }

        /// <summary>The moved creature's signature ability, played before the battle.</summary>
        public int? SignatureAbilityId { get; }
    }
}
