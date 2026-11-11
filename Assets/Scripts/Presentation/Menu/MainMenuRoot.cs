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

        [SerializeField] private CollectionSettings _collectionSettings;
        [SerializeField] private MainMenuSettings _menuSettings;
        [SerializeField] private SearchingSettings _searchSettings;
        private CollectionScreen _collection;
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

            _searching.Initialize(_searchSettings, _collectionSettings, _app);
            _searching.Hide();
            _options.Hide();
            _app.DeckIndex = Mathf.Clamp(_app.DeckIndex, 0, Mathf.Max(0, _app.Decks.Count - 1));
            _collection = new CollectionScreen(_menu.transform.parent, _menu.GetComponentInChildren<TMPro.TMP_Text>(true).font,
                _collectionSettings, _app, ShowMenu, PlayVersusAi);
            _menu.Initialize(_collectionSettings, _menuSettings, _app, ChooseDeck);
            ShowMenu();
        }

        private void OnDestroy()
        {
            _collection?.Dispose();
            if (_searchingForMatch)
                _app.Matchmaking.Cancel();
        }

        private void ShowMenu() =>
            _menu.Show(
                Texts.Get(UiTextKeys.MenuTitle),
                Texts.Get(UiTextKeys.MenuVersusAi),
                Texts.Get(UiTextKeys.MenuHotSeat),
                Texts.Format("collection.menu", string.IsNullOrEmpty(_app.Collection.Selected.Name) ? Texts.Get("collection.emptyPreset") : _app.Collection.Selected.Name),
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
            _collection.Show();
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
                    _menu.OpenSettings();
                },
                Texts.Get(UiTextKeys.Cancel),
                () => { ShowMenu(); _menu.OpenSettings(); });
        }

        private void PlayVersusAi()
        {
            if (!_app.Collection.Validate(_app.Collection.SelectedIndex).IsValid) { ChooseDeck(); return; }
            _menu.Hide();
            _searchingForMatch = true;
            _searching.Show(Texts.Get(UiTextKeys.SearchTitle), Texts.Get(UiTextKeys.SearchHint), Texts.Get(UiTextKeys.SearchCancel), CancelSearch);
            _app.Matchmaking.FindMatch(new MatchRequest(_app.Collection.Selected.ToDeckList(), _app.Difficulty), OnMatchFound);
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
            if (!_app.Collection.Validate(_app.Collection.SelectedIndex).IsValid) { ChooseDeck(); return; }
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
            var setup = new MatchSetup(_app.Collection.Selected.ToDeckList(), _app.Decks[secondDeck].ToDeckList());
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
