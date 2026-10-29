using System.Linq;
using Game.Core;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.App;
using Game.Presentation.Match;
using Game.Presentation.Ui;
using Game.Services.Match;
using Game.Services.Matchmaking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Presentation.Menu
{
    /// <summary>
    /// Composition root of the main menu scene. Prepares a match: against the AI through the matchmaking
    /// service, or two players at one screen, then hands it to the match scene.
    /// </summary>
    public sealed class MainMenuRoot : MonoBehaviour, IAppScene
    {
        [SerializeField] private MainMenuView _menu;
        [SerializeField] private SearchingView _searching;
        [SerializeField] private OptionListView _options;
        [SerializeField, Tooltip("Loaded when this scene is opened directly in the editor")] private string _bootstrapScene = "Bootstrap";

        private AppServices _app;
        private bool _searchingForMatch;

        public void Enter(AppServices app) => _app = app;

        private UiTexts Texts => _app.Texts;

        private void Start()
        {
            if (_app == null)
            {
                SceneManager.LoadScene(_bootstrapScene);
                return;
            }

            _searching.Hide();
            _options.Hide();
            _app.DeckIndex = Mathf.Clamp(_app.DeckIndex, 0, Mathf.Max(0, _app.Decks.Count - 1));
            ShowMenu();
        }

        private void OnDestroy()
        {
            if (_searchingForMatch)
                _app.Matchmaking.Cancel();
        }

        private void ShowMenu() =>
            _menu.Show(
                Texts.Get(UiTextKeys.MenuTitle),
                Texts.Get(UiTextKeys.MenuVersusAi),
                Texts.Get(UiTextKeys.MenuHotSeat),
                Texts.Format(UiTextKeys.MenuDeck, _app.Decks[_app.DeckIndex].DisplayName),
                Texts.Format(UiTextKeys.MenuDifficulty, DifficultyName(_app.Difficulty)),
                Texts.Format(UiTextKeys.MenuFastAnimations, Texts.Get(_app.PlayerSettings.FastAnimations ? UiTextKeys.On : UiTextKeys.Off)),
                PlayVersusAi,
                ChooseSecondDeck,
                ChooseDeck,
                ChooseDifficulty,
                ToggleFastAnimations);

        private void ToggleFastAnimations()
        {
            _app.PlayerSettings.FastAnimations = !_app.PlayerSettings.FastAnimations;
            ShowMenu();
        }

        private void ChooseDeck()
        {
            _menu.Hide();
            _options.Show(
                Texts.Get(UiTextKeys.MenuChooseDeck),
                _app.Decks.Select(deck => deck.DisplayName).ToList(),
                index =>
                {
                    _app.DeckIndex = index;
                    ShowMenu();
                },
                Texts.Get(UiTextKeys.Cancel),
                ShowMenu);
        }

        private void ChooseDifficulty()
        {
            var profiles = _app.Ai.Profiles;
            _menu.Hide();
            _options.Show(
                Texts.Get(UiTextKeys.MenuChooseDifficulty),
                profiles.Select(profile => profile.DisplayName).ToList(),
                index =>
                {
                    _app.Difficulty = profiles[index].Id;
                    ShowMenu();
                },
                Texts.Get(UiTextKeys.Cancel),
                ShowMenu);
        }

        private void PlayVersusAi()
        {
            _menu.Hide();
            _searchingForMatch = true;
            _searching.Show(Texts.Get(UiTextKeys.SearchTitle), Texts.Get(UiTextKeys.SearchHint), Texts.Get(UiTextKeys.Cancel), CancelSearch);
            _app.Matchmaking.FindMatch(new MatchRequest(_app.Decks[_app.DeckIndex].ToDeckList(), _app.Difficulty), OnMatchFound);
        }

        private void CancelSearch()
        {
            _searchingForMatch = false;
            _app.Matchmaking.Cancel();
            _searching.Hide();
            ShowMenu();
        }

        private void OnMatchFound(MatchFound found)
        {
            _searchingForMatch = false;
            _app.PendingMatch = new MatchLaunch(
                found.Session,
                found.Setup,
                found.Seed,
                MatchMode.VersusOpponent(found.LocalPlayer, UiTextKeys.PlayerYou, UiTextKeys.PlayerOpponent),
                new[] { found.LocalPlayer },
                found.Opponent);
            _app.Scenes.ToMatch();
        }

        /// <summary>Hot-seat: the menu deck is the first player's, the second player picks theirs now.</summary>
        private void ChooseSecondDeck()
        {
            _menu.Hide();
            _options.Show(
                Texts.Get(UiTextKeys.MenuChooseSecondDeck),
                _app.Decks.Select(deck => deck.DisplayName).ToList(),
                index => PlayHotSeat(_app.DeckIndex, index),
                Texts.Get(UiTextKeys.Cancel),
                ShowMenu);
        }

        private void PlayHotSeat(int firstDeck, int secondDeck)
        {
            var setup = new MatchSetup(_app.Decks[firstDeck].ToDeckList(), _app.Decks[secondDeck].ToDeckList());
            var seed = ((ulong)(uint)_app.Random.Next(int.MaxValue) << 31) ^ (uint)_app.Random.Next(int.MaxValue);
            _app.PendingMatch = new MatchLaunch(
                new LocalMatchSession(_app.Catalog, _app.Rules),
                setup,
                seed,
                MatchMode.HotSeat(UiTextKeys.Player1, UiTextKeys.Player2),
                PlayerIds.All,
                opponent: null);
            _app.Scenes.ToMatch();
        }

        private string DifficultyName(string id) =>
            _app.Ai.Profiles.FirstOrDefault(profile => profile.Id == id)?.DisplayName ?? id;
    }
}
