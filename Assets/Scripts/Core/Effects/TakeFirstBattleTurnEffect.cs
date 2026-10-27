using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>You take the first turn in the upcoming battle. Only before the battle starts.</summary>
    public sealed class TakeFirstBattleTurnEffect : IPlayEffect
    {
        public AbilityTargetKinds Targets => AbilityTargetKinds.None;

        public RejectReason Validate(PlayContext context) =>
            context.IsBeforeBattle ? RejectReason.None : RejectReason.TimingNotAllowed;

        public void Apply(PlayContext context) => context.TakeFirstBattleTurn();
    }
}
