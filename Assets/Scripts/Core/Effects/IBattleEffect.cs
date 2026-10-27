using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>Applied during the power count at its step, while the ability is not cancelled.</summary>
    public interface IBattleEffect : IAbilityEffect
    {
        BattleStep Step { get; }

        void Apply(BattleCalculation calculation, PlayerId caster);
    }
}
