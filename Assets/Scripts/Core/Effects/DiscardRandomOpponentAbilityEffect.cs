using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>The opponent discards a random ability from hand.</summary>
    public sealed class DiscardRandomOpponentAbilityEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context) => RejectReason.None;

        public void Apply(PlayContext context) => context.DiscardRandomAbility(context.Opponent);
    }
}
