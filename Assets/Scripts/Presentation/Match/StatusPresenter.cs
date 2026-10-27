using System.Text;
using Game.Core.Battle;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>The score, whose turn it is and the battle panel, refreshed from the viewer's view.</summary>
    public sealed class StatusPresenter
    {
        private readonly TopBarView _topBar;
        private readonly TextPanelView _battlePanel;
        private readonly TextPanelView _ruleBadge;
        private readonly PlayerNames _names;
        private readonly CardNameLookup _lookup;
        private readonly UiTexts _texts;

        public StatusPresenter(
            TopBarView topBar,
            TextPanelView battlePanel,
            TextPanelView ruleBadge,
            PlayerNames names,
            CardNameLookup lookup,
            UiTexts texts)
        {
            _topBar = topBar;
            _battlePanel = battlePanel;
            _ruleBadge = ruleBadge;
            _names = names;
            _lookup = lookup;
            _texts = texts;
        }

        /// <param name="power">The engine's current count of the battle, for the comparison rule badge; null outside a battle.</param>
        public void Show(PlayerView view, BattlePower power)
        {
            var score = _texts.Format(
                UiTextKeys.Score,
                _names.Of(PlayerId.Player1),
                view.Wins(PlayerId.Player1),
                _names.Of(PlayerId.Player2),
                view.Wins(PlayerId.Player2));
            var turn = view.AwaitingPlayer == null ? "" : _texts.Format(UiTextKeys.Turn, _names.Of(view.AwaitingPlayer));
            _topBar.Show(score, turn);

            if (view.Battle == null)
            {
                Hide();
                return;
            }

            ShowRule(power?.Rule ?? ComparisonRule.HigherPowerWins);

            var battle = view.Battle;
            var field = view.Field(battle.FieldId);
            var text = new StringBuilder();
            text.AppendLine(_texts.Format(UiTextKeys.BattleTitle, _lookup.NameOf(battle.FieldId)));
            if (field != null && !field.IsOpen)
                text.AppendLine(_texts.Get(UiTextKeys.BattleFieldClosed));
            text.AppendLine(_texts.Format(UiTextKeys.BattleHost, _names.Of(battle.Host)) + Ready(battle, battle.Host));
            text.AppendLine(_texts.Format(UiTextKeys.BattleGuest, _names.Of(battle.Guest)) + Ready(battle, battle.Guest));
            text.Append(_texts.Format(UiTextKeys.BattleActing, _names.Of(battle.CurrentActor)));
            _battlePanel.Show(text.ToString());
        }

        public void Hide()
        {
            _battlePanel.Hide();
            _ruleBadge.Hide();
        }

        /// <summary>A badge when an open field condition changes how the powers are compared.</summary>
        private void ShowRule(ComparisonRule rule)
        {
            switch (rule)
            {
                case ComparisonRule.LowerPowerWins:
                    _ruleBadge.Show(_texts.Get(UiTextKeys.RuleLower));
                    break;
                case ComparisonRule.NarrowMarginDraw:
                    _ruleBadge.Show(_texts.Get(UiTextKeys.RuleNarrow));
                    break;
                default:
                    _ruleBadge.Hide();
                    break;
            }
        }

        private string Ready(BattleView battle, PlayerId player) =>
            battle.IsReady(player) ? " — " + _texts.Get(UiTextKeys.BattleReady) : "";
    }
}
