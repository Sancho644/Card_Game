using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>A precondition such as "if you are the host" attached to an effect.</summary>
    public interface IEffectRequirement
    {
        bool IsMet(BattleCalculation calculation, PlayerId caster);
    }
}
