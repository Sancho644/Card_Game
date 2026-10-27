using Game.Core.Battle;

namespace Game.Core.Effects
{
    // TODO(rules): a comparison rule concerns both creatures, so it is not applied when one of them
    // is immune to the condition («Рассвет»). See docs/decisions.md.
    /// <summary>«Закон слабейшего»: the creature with lower final power wins.</summary>
    public sealed class LowerPowerWinsCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation)
        {
            if (calculation.ConditionAppliesToBoth)
                calculation.Comparison = new LowerPowerWins();
        }
    }
}
