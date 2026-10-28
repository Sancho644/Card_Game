using System;
using System.Collections.Generic;
using System.Linq;
using Game.AI;
using Game.Core.Randomness;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using Game.Data.Presentation;
using Game.Services.Matchmaking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Presentation.App
{
    /// <summary>
    /// The application root in the Bootstrap scene: survives scene changes, owns the services and hands them to
    /// every scene that loads (see <see cref="IAppScene"/>). The only place the services are created.
    /// </summary>
    public sealed class AppRoot : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CardDatabase _database;
        [SerializeField] private GameRulesConfig _rules;
        [SerializeField] private AiConfig _ai;
        [SerializeField] private MatchmakingConfig _matchmaking;
        [SerializeField] private UiTexts _texts;
        [SerializeField] private List<DeckDef> _decks = new List<DeckDef>();

        [Header("Scenes")]
        [SerializeField] private string _mainMenuScene = "MainMenu";
        [SerializeField] private string _matchScene = "Match";

        private AppServices _context;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            var catalog = _database.ToCatalog();
            var rules = _rules.ToRules();
            var random = new XorShiftRandom((ulong)DateTime.UtcNow.Ticks);
            var scheduler = new UnityActionScheduler();
            var opponents = new AiOpponentFactory(catalog, rules, _ai.ToProfile, scheduler);
            var matchmaking = new LocalMatchmakingService(
                catalog, rules, opponents, scheduler, random, _matchmaking.SearchSecondsMin, _matchmaking.SearchSecondsMax);

            _context = new AppServices(
                _database,
                rules,
                _texts,
                _decks.Where(deck => deck != null).ToList(),
                _ai,
                matchmaking,
                new SceneNavigator(_mainMenuScene, _matchScene),
                random);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start() => _context.Scenes.ToMainMenu();

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _context?.Matchmaking.Cancel();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var appScene in root.GetComponentsInChildren<MonoBehaviour>(true).OfType<IAppScene>())
                    appScene.Enter(_context);
            }
        }
    }
}
