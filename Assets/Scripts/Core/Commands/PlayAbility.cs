using Game.Core.State;

namespace Game.Core.Commands
{
    public sealed class PlayAbility : GameCommand
    {
        public PlayAbility(PlayerId player, int abilityId, AbilityTargets targets = null)
            : base(player)
        {
            AbilityId = abilityId;
            Targets = targets ?? AbilityTargets.None;
        }

        public int AbilityId { get; }
        public AbilityTargets Targets { get; }
    }
}
