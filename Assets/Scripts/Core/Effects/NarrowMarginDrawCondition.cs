using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>«Узкая грань»: a difference in final power below the margin is a draw.</summary>
    public sealed class NarrowMarginDrawCondition : FieldCondition
    {
        private readonly int _margin;

        public NarrowMarginDrawCondition(int margin)
        {
            _margin = margin;
        }

        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.Comparison = new NarrowMarginDraw(calculation.Comparison, _margin);
    }
}
