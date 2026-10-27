using Game.Core.Cards;
using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>Cancels the last common or epic ability the opponent played in this battle.</summary>
    public sealed class CancelLastOpponentAbilityEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context) => RejectReason.None;

        public void Apply(PlayContext context) =>
            context.CancelLastOpponentAbility(ability => ability.Rarity != Rarity.Legendary);
    }
}
