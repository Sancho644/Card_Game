using System.Linq;
using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>Returns one of your used abilities, other than this one, from the discard pile. Target: CardId.</summary>
    public sealed class ReturnAbilityFromDiscardEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context)
        {
            var id = context.Targets.CardId;
            return id != null && id != context.Ability.Id &&
                   context.State.Player(context.Caster).Discard.Any(card => card.Id == id)
                ? RejectReason.None
                : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context) => context.ReturnAbilityFromDiscard(context.Targets.CardId.Value);
    }
}
