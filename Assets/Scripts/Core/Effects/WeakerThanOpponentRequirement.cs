using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>
    /// The caster's creature is weaker than the opponent's, counting the effects applied before
    /// this one in play order.
    /// </summary>
    public sealed class WeakerThanOpponentRequirement : IEffectRequirement
    {
        public bool IsMet(BattleCalculation calculation, PlayerId caster) =>
            calculation.PowerOf(caster) < calculation.PowerOf(caster.Opponent());
    }
}
