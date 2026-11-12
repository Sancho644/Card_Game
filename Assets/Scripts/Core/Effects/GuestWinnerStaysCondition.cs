using Game.Core.Battle;

namespace Game.Core.Effects
{
    /// <summary>A winning guest holds the field instead of going out of the game.</summary>
    public sealed class GuestWinnerStaysCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences =>
            {
                if (consequences.Winner != consequences.Guest)
                    return;

                consequences.SetFate(consequences.Guest, CreatureFate.StaysOnField);
                consequences.FieldRemoved = false;
            });
    }
}