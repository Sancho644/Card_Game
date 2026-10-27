using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>«Скупая битва»: each player may play at most one common or epic ability in the whole battle.</summary>
    public sealed class OneAbilityPerPlayerCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity == Rarity.Legendary ||
            battle.CountPlayed(player, played => played.Rarity != Rarity.Legendary) == 0;
    }
}
