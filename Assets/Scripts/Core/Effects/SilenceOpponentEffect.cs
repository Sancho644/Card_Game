using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>Until the end of this battle the opponent cannot play common and epic abilities.</summary>
    public sealed class SilenceOpponentEffect : IAbilityRestriction
    {
        public bool Forbids(AbilityCard ability, PlayerId player, PlayerId caster) =>
            player != caster && ability.Rarity != Rarity.Legendary;
    }
}
