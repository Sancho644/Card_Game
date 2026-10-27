using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>Your creature leaves the battle for your hand; the battle ends without a winner.</summary>
    public sealed class RetreatEffect : IPlayEffect
    {
        public AbilityTargetKinds Targets => AbilityTargetKinds.None;

        public RejectReason Validate(PlayContext context) =>
            context.Battle != null && !context.IsBeforeBattle ? RejectReason.None : RejectReason.TimingNotAllowed;

        public void Apply(PlayContext context) => context.Retreat();
    }
}
