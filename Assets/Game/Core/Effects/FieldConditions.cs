using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Effects
{
    // ---- Restrict abilities (checked when an ability is played) ----

    /// <summary>«Тишина»: common and epic abilities cannot be played.</summary>
    public sealed class NoCommonOrEpicAbilitiesCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity == Rarity.Legendary;
    }

    /// <summary>«Право хозяина»: only the host may play common and epic abilities.</summary>
    public sealed class HostOnlyAbilitiesCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity == Rarity.Legendary || player == battle.Host;
    }

    /// <summary>«Скупая битва»: each player may play at most one common or epic ability in the whole battle.</summary>
    public sealed class OneAbilityPerPlayerCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity == Rarity.Legendary ||
            battle.CountPlayed(player, played => played.Rarity != Rarity.Legendary) == 0;
    }

    /// <summary>«Печать»: legendary abilities cannot be played.</summary>
    public sealed class NoLegendaryAbilitiesCondition : FieldCondition
    {
        public override bool AllowsAbility(AbilityCard ability, PlayerId player, BattleState battle) =>
            ability.Rarity != Rarity.Legendary;
    }

    // ---- Change the field's numbers (step 1) ----

    /// <summary>«Удвоение»: the field's bonuses and penalties are doubled.</summary>
    public sealed class DoubleFieldValuesCondition : FieldCondition
    {
        public override void ModifyFieldValues(BattleCalculation calculation)
        {
            foreach (var player in PlayerIds.All)
            {
                if (calculation.ConditionAppliesTo(player))
                    calculation.MapFieldValues(player, value => value * 2);
            }
        }
    }

    /// <summary>«Дом хозяина»: the field's penalties act only on the guest's creature.</summary>
    public sealed class PenaltiesOnlyForGuestCondition : FieldCondition
    {
        public override void ModifyFieldValues(BattleCalculation calculation)
        {
            if (calculation.ConditionAppliesTo(calculation.Host))
                calculation.MapFieldValues(calculation.Host, value => value < 0 ? 0 : value);
        }
    }

    // ---- Change the comparison (step 3) ----

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

    // ---- Change the consequences (step 3) ----

    /// <summary>«Милосердие»: the losing creature returns to hand instead of going out of the game.</summary>
    public sealed class LoserReturnsToHandCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences =>
            {
                var loser = consequences.Loser;
                if (loser != null && calculation.ConditionAppliesTo(loser.Value) &&
                    consequences.FateOf(loser.Value) == CreatureFate.OutOfGame)
                    consequences.SetFate(loser.Value, CreatureFate.ReturnsToHand);
            });
    }

    /// <summary>«Нерушимая»: after the battle the field is not removed and stays open.</summary>
    public sealed class FieldStaysCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences => consequences.FieldRemoved = false);
    }

    /// <summary>«Жертва»: the winning creature goes out of the game too.</summary>
    public sealed class WinnerOutOfGameCondition : FieldCondition
    {
        public override void ModifyOutcome(BattleCalculation calculation) =>
            calculation.ModifyConsequences(consequences =>
            {
                var winner = consequences.Winner;
                if (winner != null && calculation.ConditionAppliesTo(winner.Value))
                    consequences.SetFate(winner.Value, CreatureFate.OutOfGame);
            });
    }

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
                if (winner != null && calculation.ConditionAppliesTo(winner.Value))
                    consequences.RequestChoice(new ChoiceRequest(
                        winner.Value,
                        ChoicePurpose.TakeAbilityFromDiscard,
                        card => card.Type == CardType.Ability && card.Definition.Rarity != Rarity.Legendary));
            });
    }
}
