using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Commands;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Interaction;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Offers a local player the engine's legal actions: turn order buttons, the action bar, the table
    /// (through <see cref="ActionSelector"/>) and option lists. Sends back the one the player picks.
    /// </summary>
    public sealed class HumanDecisionUi : IDisposable
    {
        private readonly IReadOnlyList<LocalHumanController> _humans;
        private readonly ActionSelector _selector;
        private readonly TableInteraction _interaction;
        private readonly ActionBarView _actionBar;
        private readonly DialogView _turnOrder;
        private readonly OptionListView _options;
        private readonly TextPanelView _prompt;
        private readonly PlayerNames _names;
        private readonly CardNameLookup _lookup;
        private readonly UiTexts _texts;
        private LocalHumanController _current;
        private PlayerView _view;
        private bool _disposed;

        public HumanDecisionUi(
            IReadOnlyList<LocalHumanController> humans,
            ActionSelector selector,
            TableInteraction interaction,
            ActionBarView actionBar,
            DialogView turnOrder,
            OptionListView options,
            TextPanelView prompt,
            PlayerNames names,
            CardNameLookup lookup,
            UiTexts texts)
        {
            _humans = humans;
            _selector = selector;
            _interaction = interaction;
            _actionBar = actionBar;
            _turnOrder = turnOrder;
            _options = options;
            _prompt = prompt;
            _names = names;
            _lookup = lookup;
            _texts = texts;

            foreach (var human in _humans)
            {
                human.DecisionRequested += OnDecisionRequested;
                human.ReactionOffered += OnDecisionRequested;
                human.DecisionCancelled += OnDecisionCancelled;
            }

            _selector.Changed += OnSelectionChanged;
            _selector.OptionsRequested += OnOptionsRequested;
            _selector.CommandChosen += Submit;
        }

        /// <summary>Stops listening; after this the views are never touched, they may already be destroyed.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _current = null;
            _view = null;
            foreach (var human in _humans)
            {
                human.DecisionRequested -= OnDecisionRequested;
                human.ReactionOffered -= OnDecisionRequested;
                human.DecisionCancelled -= OnDecisionCancelled;
            }

            _selector.Changed -= OnSelectionChanged;
            _selector.OptionsRequested -= OnOptionsRequested;
            _selector.CommandChosen -= Submit;
        }

        public void HideAll()
        {
            if (_disposed)
                return;

            _current = null;
            _view = null;
            _interaction.Enabled = false;
            _selector.ClearActions();
            _actionBar.Hide();
            _options.Hide();
            _turnOrder.Hide();
            _prompt.Hide();
        }

        private void OnDecisionRequested(LocalHumanController human, PlayerView view, IReadOnlyList<GameCommand> actions)
        {
            if (_disposed)
                return;

            _current = human;
            _view = view;
            _lookup.Remember(view);

            var order = actions.OfType<ChooseTurnOrder>().ToList();
            if (order.Count > 0)
            {
                ShowTurnOrder(view, order);
                return;
            }

            _actionBar.Show(
                ButtonFor(actions.OfType<EndTurn>().FirstOrDefault()),
                ButtonFor(actions.OfType<Ready>().FirstOrDefault()),
                ButtonFor(actions.OfType<OpenField>().FirstOrDefault()));
            _interaction.Enabled = true;
            _selector.SetActions(actions);
        }

        private void ShowTurnOrder(PlayerView view, IReadOnlyList<ChooseTurnOrder> order)
        {
            var first = order.FirstOrDefault(choice => choice.GoFirst);
            var second = order.FirstOrDefault(choice => !choice.GoFirst);
            _turnOrder.Show(
                _texts.Format(UiTextKeys.CoinResult, _names.Of(view.CoinTossWinner)),
                null,
                _texts.Get(UiTextKeys.GoFirst),
                first == null ? (Action)null : () => Submit(first),
                _texts.Get(UiTextKeys.GoSecond),
                second == null ? (Action)null : () => Submit(second));
        }

        private Action ButtonFor(GameCommand command) => command == null ? (Action)null : () => Submit(command);

        private void OnSelectionChanged()
        {
            if (_current == null)
                return;

            var choosing = _selector.IsSelecting && _selector.Source != null;
            _actionBar.ShowCancel(choosing ? _selector.Cancel : (Action)null);
            _prompt.Show(_texts.Get(choosing ? UiTextKeys.PromptTarget : UiTextKeys.PromptSelect));
        }

        private void OnOptionsRequested(IReadOnlyList<int> values)
        {
            var title = values.Contains(ActionPath.NoOption)
                ? UiTextKeys.ChoiceSignature
                : _view?.ChoosingPlayer != null ? UiTextKeys.ChoiceTrophy : UiTextKeys.ChoiceCard;
            var labels = values
                .Select(value => value == ActionPath.NoOption ? _texts.Get(UiTextKeys.NoAbility) : _lookup.NameOf(value))
                .ToList();
            var canCancel = _selector.Source != null;
            _options.Show(
                _texts.Get(title),
                labels,
                index => _selector.ChooseOption(values[index]),
                canCancel ? _texts.Get(UiTextKeys.Cancel) : null,
                canCancel ? _selector.Cancel : (Action)null);
        }

        private void Submit(GameCommand command)
        {
            var human = _current;
            HideAll();
            human?.Submit(command);
        }

        private void OnDecisionCancelled(LocalHumanController human)
        {
            if (human == _current)
                HideAll();
        }
    }
}
