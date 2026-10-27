using System.Linq;
using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>
    /// You discard one other ability from your hand; with none left, nothing happens.
    /// Target: CardId, required while you hold another ability.
    /// </summary>
    public sealed class DiscardOwnAbilityEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context)
        {
            var others = context.AbilitiesInHand(context.Caster).Where(card => card.Id != context.Ability.Id).ToList();
            if (others.Count == 0)
                return RejectReason.None;
            return others.Any(card => card.Id == context.Targets.CardId) ? RejectReason.None : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context)
        {
            var id = context.Targets.CardId;
            if (id != null && context.State.Player(context.Caster).FindInHand(id.Value) != null)
                context.DiscardFromHand(context.Caster, id.Value);
        }
    }
}
