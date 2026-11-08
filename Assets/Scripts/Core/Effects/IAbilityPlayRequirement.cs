using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>A stable requirement checked before spending an ability, including during placement.</summary>
    public interface IAbilityPlayRequirement
    {
        bool AllowsPlay(PlayerId caster, BattleState battle, CardDefinition creature);
    }
}
