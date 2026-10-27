using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>One effect of an ability card, applied during battle resolution in play order.</summary>
    public interface IAbilityEffect
    {
        void Apply(BattleCalculation calculation, PlayerId caster);
    }
}
