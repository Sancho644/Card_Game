using Game.Core.Battle;

namespace Game.Core.Effects
{
    // TODO(rules): a comparison rule concerns both creatures, so it is not applied when one of them
    // is immune to the condition («Рассвет»). See docs/decisions.md.
    /// <summary>«Узкая грань»: a difference in final power below the margin is a draw.</summary>
    public sealed class NarrowMarginDrawCondition : FieldCondition
    {
        private readonly int _margin;

        public NarrowMarginDrawCondition(int margin)
        {
            _margin = margin;
        }

        public override void ModifyOutcome(BattleCalculation calculation)
        {
            if (calculation.ConditionAppliesToBoth)
                calculation.Comparison = new NarrowMarginDraw(calculation.Comparison, _margin);
        }
    }
}
