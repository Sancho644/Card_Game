using System;
using System.Collections.Generic;
using System.Linq;
using Game.AI;
using Game.Core.Randomness;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using Game.Data.Presentation;
using Game.Presentation.Audio;
using Game.Presentation.Ui;
using Game.Services.Matchmaking;
using Game.Services.Settings;
using UnityEngine;
using UnityEngine.Audio;
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
        [SerializeField] private OnlineSettings _onlineSettings;
        [SerializeField] private UiTexts _texts;
        [SerializeField] private SoundSettings _sounds;
        [SerializeField] private LoadingScreenSettings _loadingSettings;
        [SerializeField] private LoadingScreenView _loadingScreen;
        [SerializeField] private List<DeckDef> _decks = new List<DeckDef>();

        [Header("Scenes")]
        [SerializeField] private string _mainMenuScene = "MainMenu";
        [SerializeField] private string _matchScene = "Match";

        private AppServices _context;
        private AudioSourceSoundPlayer _sound;
        private MusicPlayer _music;
        private AudioMixerLevels _audioLevels;
        private AudioMixerGroup _sfxGroup;
        private AudioMixerGroup _uiGroup;
        private AudioMixerGroup _musicGroup;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var loading = Instantiate(_loadingScreen, transform);
            loading.Initialize(_loadingSettings, _texts);
            var scenes = gameObject.AddComponent<SceneNavigator>();
            scenes.Configure(_mainMenuScene, _matchScene, loading);

            var catalog = _database.ToCatalog();
            var rules = _rules.ToRules();
            var random = new XorShiftRandom((ulong)DateTime.UtcNow.Ticks);
            var scheduler = new UnityActionScheduler();
            var opponents = new AiOpponentFactory(catalog, rules, _ai.ToProfile, scheduler);
            var matchmaking = new LocalMatchmakingService(
                catalog, rules, opponents, scheduler, random, _matchmaking.SearchSecondsMin, _matchmaking.SearchSecondsMax);
            var mixer = _sounds != null ? _sounds.Mixer : null;
            _sfxGroup = FindGroup(mixer, "SFX");
            _uiGroup = FindGroup(mixer, "UI");
            _musicGroup = FindGroup(mixer, "Music");
            _music = gameObject.AddComponent<MusicPlayer>();
            _music.Configure(_musicGroup);
            _sound = gameObject.AddComponent<AudioSourceSoundPlayer>();
            _sound.Configure(_sounds, _sfxGroup, _uiGroup, _music);
            _audioLevels = new AudioMixerLevels((name, gain) =>
                mixer != null && mixer.GetFloat(name, out _) && mixer.SetFloat(name, gain));

            _context = new AppServices(
                _database,
                rules,
                _texts,
                _decks.Where(deck => deck != null).ToList(),
                _ai,
                matchmaking,
                scenes,
                random,
                new LocalPlayerSettingsService(new PlayerPrefsSettingsStore()),
                _sound,
                _onlineSettings);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            _context.PlayerSettings.Changed += ApplyAudioLevels;
            ApplyAudioLevels();
            _context.Scenes.ToMainMenu();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                _context?.PlayerSettings.SaveAudio();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_context == null)
                return;

            _context.PlayerSettings.Changed -= ApplyAudioLevels;
            _context.PlayerSettings.SaveAudio();
            _context.Matchmaking.Cancel();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
                    {
                        var scope = canvas.GetComponent<UiAudioScope>() ?? canvas.gameObject.AddComponent<UiAudioScope>();
                        scope.Sound = _sound;
                    }
                }

                foreach (var appScene in root.GetComponentsInChildren<MonoBehaviour>(true).OfType<IAppScene>())
                    appScene.Enter(_context);
            }

            if (scene.name == _mainMenuScene)
                _music.Play(_sounds != null ? _sounds.MenuMusic : null);
            else if (scene.name == _matchScene)
                _music.Play(_sounds != null ? _sounds.MatchMusic : null);
            else
                _music.Stop();
        }

        private void ApplyAudioLevels()
        {
            _audioLevels.Apply(_context.PlayerSettings);
            _sound.SetVolume(_audioLevels.SourceGain(AudioBus.Sfx, _sfxGroup != null),
                _audioLevels.SourceGain(AudioBus.Ui, _uiGroup != null));
            _music.SetVolume(_audioLevels.SourceGain(AudioBus.Music, _musicGroup != null));
        }

        private static AudioMixerGroup FindGroup(AudioMixer mixer, string name) =>
            mixer == null ? null : mixer.FindMatchingGroups("Master/" + name).FirstOrDefault(group => group.name == name);
    }
}
