using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>One effect of an ability card, applied during battle resolution at its <see cref="Step"/>.</summary>
    public interface IAbilityEffect
    {
        BattleStep Step { get; }

        void Apply(BattleCalculation calculation, PlayerId caster);
    }
}
