using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>«Право хозяина»: only the host may play common and epic abilities.</summary>
    public sealed class HostOnlyAbilitiesCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity == Rarity.Legendary || player == battle.Host;
    }
}
