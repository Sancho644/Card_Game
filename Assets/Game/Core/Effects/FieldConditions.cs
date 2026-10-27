using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>The field's bonuses and penalties are doubled.</summary>
    public sealed class DoubleFieldValuesCondition : FieldCondition
    {
        public override void ModifyFieldValues(BattleCalculation calculation)
        {
            foreach (var player in PlayerIds.All)
                calculation.MapFieldValues(player, value => value * 2);
        }
    }

    /// <summary>The creature with lower final power wins.</summary>
    public sealed class LowerPowerWinsCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.Comparison = new LowerPowerWins();
    }
}
