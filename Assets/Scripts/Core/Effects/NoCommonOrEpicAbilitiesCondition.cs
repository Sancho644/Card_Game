using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>«Тишина»: common and epic abilities cannot be played.</summary>
    public sealed class NoCommonOrEpicAbilitiesCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity == Rarity.Legendary;
    }
}
