using System.Collections.Generic;
using System.Text;
using Game.Core.Battle;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Match
{
    /// <summary>The text of the power breakdown panel: both creatures step by step, the rule, the consequences.</summary>
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

        /// <param name="consequences">The events that followed the result in the same batch.</param>
        public string Build(BattleResolved result, PlayerId host, int hostCreature, int guestCreature, IReadOnlyList<GameEvent> consequences)
        {
            var text = new StringBuilder();
            AppendSide(text, host, hostCreature, result.Host);
            text.Append('\n');
            AppendSide(text, host.Opponent(), guestCreature, result.Guest);
            text.Append('\n');

            if (result.Rule == ComparisonRule.LowerPowerWins)
                text.Append(_texts.Get(UiTextKeys.RuleLower)).Append('\n');
            else if (result.Rule == ComparisonRule.NarrowMarginDraw)
                text.Append(_texts.Get(UiTextKeys.RuleNarrow)).Append('\n');
            if (result.TieBroken)
                text.Append(_texts.Get(UiTextKeys.TieBroken)).Append('\n');

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

            return text.ToString().TrimEnd();
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
