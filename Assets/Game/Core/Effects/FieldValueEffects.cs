using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>The open field's bonuses and penalties swap places, for both creatures.</summary>
    public sealed class InvertFieldValuesEffect : IAbilityEffect
    {
        public BattleStep Step => BattleStep.FieldValues;

        public void Apply(BattleCalculation calculation, PlayerId caster)
        {
            foreach (var player in PlayerIds.All)
                calculation.MapFieldValues(player, value => -value);
        }
    }

    /// <summary>The open field's condition stops working; bonuses and penalties stay.</summary>
    public sealed class CancelFieldConditionEffect : IAbilityEffect
    {
        public BattleStep Step => BattleStep.ConditionControl;

        public void Apply(BattleCalculation calculation, PlayerId caster) => calculation.FieldConditionActive = false;
    }
}
