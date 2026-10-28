using System.Collections.Generic;
using System.Text;
using Game.Core.Battle;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Match
{
    /// <summary>The texts of the battle result panel: a short result and, on request, the power count step by step.</summary>
    public sealed class BattleSummaryBuilder
    {
        private readonly UiTexts _texts;
        private readonly PlayerNames _names;
        private readonly CardNameLookup _lookup;

        public BattleSummaryBuilder(UiTexts texts, PlayerNames names, CardNameLookup lookup)
        {
            _texts = texts;
            _names = names;
            _lookup = lookup;
        }

        public string Title => _texts.Get(UiTextKeys.BreakdownTitle);

        /// <summary>The result in a few lines: both powers, the rule if unusual, the winner and the consequences.</summary>
        /// <param name="consequences">The events that followed the result in the same batch.</param>
        public string BuildShort(BattleResolved result, PlayerId host, IReadOnlyList<GameEvent> consequences)
        {
            var text = new StringBuilder();
            text.Append(_texts.Format(
                UiTextKeys.ResultPowers,
                _names.Of(host),
                result.HostPower,
                _names.Of(host.Opponent()),
                result.GuestPower)).Append('\n');
            AppendOutcome(text, result, consequences);
            return text.ToString().TrimEnd();
        }

        /// <summary>Both creatures step by step, then the same outcome as the short text.</summary>
        public string BuildDetails(BattleResolved result, PlayerId host, int hostCreature, int guestCreature, IReadOnlyList<GameEvent> consequences)
        {
            var text = new StringBuilder();
            AppendSide(text, host, hostCreature, result.Host);
            text.Append('\n');
            AppendSide(text, host.Opponent(), guestCreature, result.Guest);
            text.Append('\n');
            AppendOutcome(text, result, consequences);
            return text.ToString().TrimEnd();
        }

        private void AppendOutcome(StringBuilder text, BattleResolved result, IReadOnlyList<GameEvent> consequences)
        {
            if (result.Rule == ComparisonRule.LowerPowerWins)
                text.Append(_texts.Get(UiTextKeys.RuleLower)).Append('\n');
            else if (result.Rule == ComparisonRule.NarrowMarginDraw)
                text.Append(_texts.Get(UiTextKeys.RuleNarrow)).Append('\n');
            if (result.TieBreak == TieBreak.Ability)
                text.Append(_texts.Get(UiTextKeys.TieBroken)).Append('\n');
            else if (result.TieBreak == TieBreak.EarlierPlacement)
                text.Append(_texts.Get(UiTextKeys.TieEarlierPlacement)).Append('\n');

            text.Append(result.Winner == null
                ? _texts.Get(UiTextKeys.Draw)
                : _texts.Format(UiTextKeys.Winner, _names.Of(result.Winner.Value))).Append('\n');

            foreach (var consequence in consequences)
            {
                switch (consequence)
                {
                    case CreatureOut creature:
                        text.Append(_texts.Format(UiTextKeys.Out, _lookup.NameOf(creature.CreatureId))).Append('\n');
                        break;
                    case CreatureReturnedToHand creature:
                        text.Append(_texts.Format(UiTextKeys.ToHand, _lookup.NameOf(creature.CreatureId))).Append('\n');
                        break;
                    case FieldRemoved _:
                        text.Append(_texts.Get(UiTextKeys.FieldRemoved)).Append('\n');
                        break;
                }
            }
        }

        private void AppendSide(StringBuilder text, PlayerId player, int creature, PowerBreakdown power)
        {
            text.Append(_texts.Format(UiTextKeys.BreakdownCreature, _names.Of(player), _lookup.NameOf(creature))).Append('\n');
            text.Append(_texts.Format(UiTextKeys.BreakdownBase, power.BasePower)).Append('\n');
            if (power.FieldValue != 0)
                text.Append(_texts.Format(UiTextKeys.BreakdownField, Signed(power.FieldValue))).Append('\n');
            foreach (var ability in power.Abilities)
            {
                _lookup.Remember(ability.Source);
                text.Append(_texts.Format(UiTextKeys.BreakdownAbility, _lookup.NameOf(ability.Source?.InstanceId ?? 0), Signed(ability.Amount))).Append('\n');
            }

            text.Append(_texts.Format(UiTextKeys.BreakdownTotal, power.FinalPower)).Append('\n');
        }

        private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();
    }
}
