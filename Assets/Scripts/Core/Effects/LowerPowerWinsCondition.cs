using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>«Закон слабейшего»: the creature with lower final power wins.</summary>
    public sealed class LowerPowerWinsCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.Comparison = new LowerPowerWins();
    }
}
