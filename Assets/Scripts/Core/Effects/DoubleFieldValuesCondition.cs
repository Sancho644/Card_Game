using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>«Удвоение»: the field's bonuses and penalties are doubled.</summary>
    public sealed class DoubleFieldValuesCondition : FieldCondition
    {
        public override void ModifyFieldValues(BattleCalculation calculation)
        {
            foreach (var player in PlayerIds.All)
                calculation.MapFieldValues(player, value => value * 2);
        }
    }
}
