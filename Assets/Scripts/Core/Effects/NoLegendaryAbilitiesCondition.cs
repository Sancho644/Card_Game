using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>«Печать»: legendary abilities cannot be played.</summary>
    public sealed class NoLegendaryAbilitiesCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity != Rarity.Legendary;
    }
}
