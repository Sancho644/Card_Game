using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>Forbids other abilities while the ability carrying it is in force in the battle.</summary>
    public interface IAbilityRestriction : IAbilityEffect
    {
        bool Forbids(AbilityCard ability, PlayerId player, PlayerId caster);
    }
}
