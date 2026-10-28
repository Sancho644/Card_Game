using System;
using System.Linq;
using Game.Core;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// The expected value of the battle in progress for one player: the chance to win from the power margin,
    /// the host's option to open the field and the abilities both players still hold, times what winning and
    /// losing would bring. Powers and consequences come from the forecast engine.
    /// </summary>
    internal sealed class BattleOutlook
    {
        private const AbilityTiming BattleTimings = AbilityTiming.InBattle | AbilityTiming.InBattleOnOpponentTurn;

        private readonly AiProfile _profile;
        private readonly GameRules _rules;
        private readonly CardValues _values;

        public BattleOutlook(AiProfile profile, GameRules rules, CardValues values)
        {
            _profile = profile;
            _rules = rules;
            _values = values;
        }

        private AiWeights Weights => _profile.Weights;

        public double Expectation(MatchEngine engine, PlayerId me)
        {
            var winChance = WinChance(engine, me);
            var ifWin = OutcomeValue(engine, engine.PreviewConsequences(me), me);
            var ifLose = OutcomeValue(engine, engine.PreviewConsequences(me.Opponent()), me);
            return winChance * ifWin + (1 - winChance) * ifLose;
        }

        public double WinChance(MatchEngine engine, PlayerId me)
        {
            var current = engine.GetBattlePower();
            var margin = Margin(current, me) + EqualPowerEdge(current, me) + OpenOption(engine, current, me)
                         + Weights.OwnReserveShare * Reserve(engine, current, me);
            if (_profile.ExpectsOpponentResponse)
                margin -= Weights.OpponentReserveShare * Reserve(engine, current, me.Opponent());
            return 1 / (1 + Math.Exp(-margin / Weights.MarginScale));
        }

        /// <summary>How far the player is ahead, by the comparison rule in force.</summary>
        private static double Margin(BattlePower power, PlayerId player)
        {
            double margin = power.Of(player).FinalPower - power.Of(player.Opponent()).FinalPower;
            return power.Rule == ComparisonRule.LowerPowerWins ? -margin : margin;
        }

        /// <summary>Equal power is not a draw: it goes to the creature placed earlier or to «Перевес».</summary>
        private double EqualPowerEdge(BattlePower power, PlayerId me)
        {
            if (power.EqualPowerWinner == null)
                return 0;
            return power.EqualPowerWinner == me ? Weights.EqualPowerEdge : -Weights.EqualPowerEdge;
        }

        /// <summary>A closed field the host can still open: the host opens it only if that helps the host.</summary>
        private static double OpenOption(MatchEngine engine, BattlePower current, PlayerId me)
        {
            var battle = engine.State.Battle;
            if (battle.Field.IsOpen || battle.Field.IsLocked || battle.IsReady(battle.Host))
                return 0;

            var gain = Margin(engine.PreviewPowerWithOpenField(), battle.Host) - Margin(current, battle.Host);
            if (gain <= 0)
                return 0;
            return me == battle.Host ? gain : -gain;
        }

        /// <summary>
        /// Margin the player's abilities in hand could add: every useful common and epic one and the best
        /// legendary, if the legendary limit allows. Abilities the engine forbids right now are skipped.
        /// </summary>
        private double Reserve(MatchEngine engine, BattlePower current, PlayerId player)
        {
            var state = engine.State;
            var legendaryAllowed = state.Player(player).LegendariesPlayedThisTurn < _rules.LegendaryAbilitiesPerTurn;
            double regular = 0, legendary = 0;
            foreach (var card in state.Player(player).Hand.Where(card => card.Definition is AbilityCard))
            {
                var ability = (AbilityCard)card.Definition;
                if ((ability.Timing & BattleTimings) == 0 || IsForbidden(engine, player, card.Id))
                    continue;

                var gain = Margin(engine.PreviewPowerWithAbility(player, card.Id), player) - Margin(current, player);
                if (gain <= 0)
                    continue;
                if (ability.Rarity != Rarity.Legendary)
                    regular += gain;
                else if (legendaryAllowed)
                    legendary = Math.Max(legendary, gain);
            }

            return regular + legendary;
        }

        private static bool IsForbidden(MatchEngine engine, PlayerId player, int abilityId)
        {
            var reason = engine.Validate(new PlayAbility(player, abilityId));
            return reason == RejectReason.BlockedByFieldCondition
                   || reason == RejectReason.BlockedByAbility
                   || reason == RejectReason.LegendaryLimitReached
                   || reason == RejectReason.SignatureCreatureNotInBattle;
        }

        /// <summary>What the consequences mean for <paramref name="me"/>, beyond the position as it stands.</summary>
        private double OutcomeValue(MatchEngine engine, BattleConsequences consequences, PlayerId me)
        {
            var state = engine.State;
            var field = state.Battle.Field;
            double value = 0;
            if (consequences.WinScoredBy != null)
            {
                var scorer = consequences.WinScoredBy.Value;
                var gain = Weights.WinPoint;
                if (state.Player(scorer).Wins + 1 >= _rules.WinsToWin)
                    gain += Weights.MatchWin;
                value += Sign(scorer, me) * gain;
            }

            foreach (var id in PlayerIds.All)
            {
                if (consequences.FateOf(id) == CreatureFate.OutOfGame)
                    value -= Sign(id, me) * _values.Creature(field.OccupantOf(id).Definition);
            }

            if (consequences.FieldRemoved)
                value -= Sign(field.Owner, me) * Weights.FieldCard;
            foreach (var choice in consequences.Choices)
                value += Sign(choice.Player, me) * Weights.ChoiceAfterBattle;
            return value;
        }

        private static int Sign(PlayerId player, PlayerId me) => player == me ? 1 : -1;
    }
}
