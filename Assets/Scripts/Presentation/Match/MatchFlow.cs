using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.State;
using Game.Data.Decks;
using Game.Data.Presentation;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>Stages of a session at the table: each player picks a deck, the match runs, a new match.</summary>
    public sealed class MatchFlow
    {
        private readonly IReadOnlyList<DeckDef> _decks;
        private readonly OptionListView _deckSelect;
        private readonly PlayerNames _names;
        private readonly UiTexts _texts;
        private readonly Func<MatchPresenter> _createPresenter;
        private readonly Action _resetTable;
        private MatchPresenter _presenter;

        public MatchFlow(
            IReadOnlyList<DeckDef> decks,
            OptionListView deckSelect,
            PlayerNames names,
            UiTexts texts,
            Func<MatchPresenter> createPresenter,
            Action resetTable)
        {
            _decks = decks;
            _deckSelect = deckSelect;
            _names = names;
            _texts = texts;
            _createPresenter = createPresenter;
            _resetTable = resetTable;
        }

        public MatchPresenter Presenter => _presenter;

        public void Begin()
        {
            _presenter?.Dispose();
            _presenter = null;
            _resetTable();
            ChooseDeck(PlayerId.Player1, first => ChooseDeck(PlayerId.Player2, second => StartMatch(first, second)));
        }

        private void ChooseDeck(PlayerId player, Action<DeckDef> chosen) =>
            _deckSelect.Show(
                _texts.Format(UiTextKeys.DeckSelectTitle, _names.Of(player)),
                _decks.Select(deck => deck.DisplayName).ToList(),
                index => chosen(_decks[index]));

        private void StartMatch(DeckDef first, DeckDef second)
        {
            _presenter = _createPresenter();
            var seed = (ulong)DateTime.UtcNow.Ticks;
            _presenter.Start(new MatchSetup(first.ToDeckList(), second.ToDeckList()), seed);
        }
    }
}
