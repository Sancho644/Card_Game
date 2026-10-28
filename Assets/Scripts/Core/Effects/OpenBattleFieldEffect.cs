using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>The host opens the battle's field at once if it is closed (and not locked).</summary>
    public sealed class OpenBattleFieldEffect : IPlayEffect
    {
        public AbilityTargetKinds Targets => AbilityTargetKinds.None;

        public RejectReason Validate(PlayContext context) => RejectReason.None;

        public void Apply(PlayContext context) => context.OpenBattleField();
    }
}
