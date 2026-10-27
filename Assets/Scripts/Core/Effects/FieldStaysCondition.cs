using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>«Нерушимая»: after the battle the field is not removed and stays open.</summary>
    public sealed class FieldStaysCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences => consequences.FieldRemoved = false);
    }
}
