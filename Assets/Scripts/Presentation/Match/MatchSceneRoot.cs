using System.Collections.Generic;
using System.Linq;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.App;
using Game.Presentation.Cards;
using Game.Presentation.Input;
using Game.Presentation.Interaction;
using Game.Presentation.Table;
using Game.Presentation.Ui;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Composition root of the match scene: the only place where the table and interface objects are created
    /// and connected. Runs the match the menu prepared (<see cref="AppServices.PendingMatch"/>) and returns to
    /// the menu when it is over. Everything else gets its dependencies through constructors.
    /// </summary>
    public sealed class MatchSceneRoot : MonoBehaviour, IAppScene
    {
        [Header("Data")]
        [SerializeField] private VisualSettings _visual;
        [SerializeField] private TablePresentationSettings _settings;
        [SerializeField] private AnimationSettings _animation;
        [SerializeField, Tooltip("Loaded when this scene is opened directly in the editor")] private string _bootstrapScene = "Bootstrap";

        [Header("Table")]
        [SerializeField] private Camera _camera;
        [SerializeField] private TableLayout _layout;
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private Transform _cardRoot;
        [SerializeField] private PointerInput _pointer;

        [Header("Interface")]
        [SerializeField] private TopBarView _topBar;
        [SerializeField] private EventLogView _log;
        [SerializeField] private ActionBarView _actionBar;
        [SerializeField] private TextPanelView _battlePanel;
        [SerializeField] private TextPanelView _ruleBadge;
        [SerializeField] private BannerView _banner;
        [SerializeField] private TextPanelView _prompt;
        [SerializeField] private DialogView _breakdown;
        [SerializeField] private DialogView _turnOrder;
        [SerializeField] private DialogView _handoff;
        [SerializeField] private DialogView _matchEnd;
        [SerializeField] private OptionListView _options;
        [SerializeField] private CardZoomView _zoom;

        private AppServices _app;
        private MatchLaunch _launch;
        private MatchPresenter _presenter;
        private TableInteraction _interaction;
        private HumanDecisionUi _decisions;
        private AnimationQueue _queue;
        private AnimationSpeed _speed;

        public void Enter(AppServices app) => _app = app;

        private void Start()
        {
            _launch = _app?.PendingMatch;
            if (_launch == null)
            {
                SceneManager.LoadScene(_bootstrapScene);
                return;
            }

            _app.PendingMatch = null;
            Build();
            _presenter.Start(_launch.Setup, _launch.Seed);
        }

        private void Build()
        {
            var texts = _app.Texts;
            _layout.Configure(_settings, _camera);
            var framer = _camera.GetComponent<CameraFramer>();
            framer.Configure(_layout.FramingPoints, _settings.TopBarShare);
            _pointer.Configure(_settings.LongPressSeconds, _settings.DragThresholdPixels);
            _actionBar.Init(texts.Get(UiTextKeys.EndTurn), texts.Get(UiTextKeys.Ready), texts.Get(UiTextKeys.OpenField), texts.Get(UiTextKeys.Cancel));
            _log.Init(_settings.LogLines);
            _topBar.Init(texts.Get(UiTextKeys.ShowBothHands));
            foreach (var dialog in new[] { _breakdown, _turnOrder, _handoff, _matchEnd })
                dialog.Hide();
            _options.Hide();
            _zoom.Hide();

            var faces = new CardFaceBuilder(_app.Database, _visual, texts);
            var skin = new CardSkin(_visual);
            _zoom.Init(skin);
            var registry = new CardViewRegistry(_cardPrefab, _cardRoot, faces, skin);
            var queue = _queue = new AnimationQueue();
            var speed = _speed = new AnimationSpeed(_animation, _app.PlayerSettings);
            var sync = new TableSync(_layout, registry, _animation, _visual, texts);
            var names = new PlayerNames(texts);
            names.Use(_launch.Mode);
            var lookup = new CardNameLookup(faces);
            var viewer = new HotSeatDirector { Viewer = _launch.LocalPlayers.First() };
            var selector = new ActionSelector();
            var humans = _launch.LocalPlayers.Select(player => new LocalHumanController(player)).ToList();
            var status = new StatusPresenter(_topBar, _battlePanel, _ruleBadge, names, lookup, texts);
            var power = new BattlePowerPresenter(
                registry, new FloatingTextSpawner(_cardRoot, _camera), new PowerDiff(lookup), _visual, _animation);

            _interaction = new TableInteraction(
                _pointer, new TableRaycaster(_camera), registry, selector, new DropTargetPicker(), _zoom, faces, queue, speed, _visual, _settings);
            _decisions = new HumanDecisionUi(
                humans, selector, _interaction, _actionBar, _turnOrder, _options, _prompt, names, lookup, texts);

            var presenters = new IEventPresenter[]
            {
                new LogPresenter(new EventLogFormatter(texts, names, lookup, faces), _log),
                new AbilityPlayedPresenter(_layout, registry, _animation),
                new TableMovePresenter(),
                new BattleResolvedPresenter(new BattleSummaryBuilder(texts, names, lookup), _breakdown, texts),
                new ScoutedPresenter(_breakdown, faces, texts),
                new SkipPresenter(_banner, names, texts, _animation),
            };

            _presenter = new MatchPresenter(
                _launch.Session,
                presenters,
                queue,
                sync,
                viewer,
                humans,
                _launch.Mode,
                status,
                power,
                _handoff,
                _matchEnd,
                names,
                lookup,
                texts,
                () => _app.Scenes.ToMainMenu());

            _interaction.ResyncRequested += () => _presenter.Resync();
            framer.Changed += () => _presenter.Resync();
            _topBar.ShowBothHandsChanged += show =>
            {
                viewer.ShowBothHands = show;
                _presenter.Resync();
            };
        }

        /// <summary>
        /// The scene is unloading and other objects may already be destroyed: first everything stops
        /// listening, then the match and the opponent are dropped quietly, without updating any view.
        /// </summary>
        private void OnDestroy()
        {
            _queue?.Stop();
            _decisions?.Dispose();
            _interaction?.Dispose();
            _presenter?.Dispose();
            _launch?.Opponent?.Dispose();
            _speed?.Dispose();
        }
    }
}
