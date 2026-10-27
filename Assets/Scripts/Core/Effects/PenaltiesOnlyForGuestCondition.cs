using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>«Дом хозяина»: the field's penalties act only on the guest's creature.</summary>
    public sealed class PenaltiesOnlyForGuestCondition : FieldCondition
    {
        public override void ModifyFieldValues(BattleCalculation calculation)
        {
            if (calculation.ConditionAppliesTo(calculation.Host))
                calculation.MapFieldValues(calculation.Host, value => value < 0 ? 0 : value);
        }
    }
}
