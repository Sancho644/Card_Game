using System;
using Game.Presentation.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Menu
{
    /// <summary>The main menu: play against the AI, play two at one screen, choose the deck and the difficulty, settings.</summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _versusAi;
        [SerializeField] private Button _hotSeat;
        [SerializeField] private Button _deck;
        [SerializeField] private Button _difficulty;
        [SerializeField] private Button _fastAnimations;
        private MainMenuLayout _layout;
        private string _collectionLabel;

        public void Initialize(Game.Data.Presentation.CollectionSettings style, Game.Data.Presentation.MainMenuSettings settings,
            Game.Presentation.App.AppServices app, Action chooseDeck)
        {
            var font = _title.font;
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            _layout = new MainMenuLayout(transform, font, style, settings, app, chooseDeck);
            _collectionLabel = app.Texts.Get("library.collection");
            _title = _layout.Title; _versusAi = _layout.VersusAi; _hotSeat = _layout.HotSeat;
            _deck = _layout.Collection; _difficulty = _layout.Difficulty; _fastAnimations = _layout.FastAnimations;
        }

        public void OpenSettings() => _layout?.OpenSettings();
        private void OnDestroy() => _layout?.Dispose();

        public void Show(
            string title,
            string versusAiLabel,
            string hotSeatLabel,
            string deckLabel,
            string difficultyLabel,
            string fastAnimationsLabel,
            Action versusAi,
            Action hotSeat,
            Action chooseDeck,
            Action chooseDifficulty,
            Action toggleFastAnimations)
        {
            _title.text = title;
            UiButtons.SetLabel(_versusAi, versusAiLabel);
            UiButtons.Bind(_versusAi, versusAi);
            UiButtons.SetLabel(_hotSeat, hotSeatLabel);
            UiButtons.Bind(_hotSeat, hotSeat);
            UiButtons.SetLabel(_deck, _layout == null ? deckLabel : _collectionLabel);
            UiButtons.Bind(_deck, chooseDeck);
            UiButtons.SetLabel(_difficulty, difficultyLabel);
            UiButtons.Bind(_difficulty, chooseDifficulty);
            UiButtons.SetLabel(_fastAnimations, fastAnimationsLabel);
            UiButtons.Bind(_fastAnimations, toggleFastAnimations);
            _layout?.RefreshDeck();
            gameObject.SetActive(true);
        }

        public void Hide() { _layout?.CloseModals(); gameObject.SetActive(false); }
    }
}
