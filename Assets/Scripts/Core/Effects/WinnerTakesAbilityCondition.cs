using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>
    /// «Трофей»: the winner takes one common or epic ability from their discard pile into hand,
    /// including one played in this battle. The winner chooses which.
    /// </summary>
    public sealed class WinnerTakesAbilityCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences =>
            {
                var winner = consequences.Winner;
                if (winner != null)
                    consequences.RequestChoice(new ChoiceRequest(
                        winner.Value,
                        ChoicePurpose.TakeAbilityFromDiscard,
                        card => card.Type == CardType.Ability && card.Definition.Rarity != Rarity.Legendary));
            });
    }
}
