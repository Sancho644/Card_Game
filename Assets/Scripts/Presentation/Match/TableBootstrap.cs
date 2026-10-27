using System.Collections.Generic;
using Game.Core.State;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Cards;
using Game.Presentation.Input;
using Game.Presentation.Interaction;
using Game.Presentation.Table;
using Game.Presentation.Ui;
using Game.Services.Match;
using UnityEngine;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Composition root of the table scene: the only place where the presentation objects are created
    /// and connected. Everything else gets its dependencies through constructors.
    /// </summary>
    public sealed class TableBootstrap : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CardDatabase _database;
        [SerializeField] private GameRulesConfig _rules;
        [SerializeField] private VisualSettings _visual;
        [SerializeField] private UiTexts _texts;
        [SerializeField] private TablePresentationSettings _settings;
        [SerializeField] private List<DeckDef> _decks = new List<DeckDef>();

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
        [SerializeField] private TextPanelView _prompt;
        [SerializeField] private DialogView _breakdown;
        [SerializeField] private DialogView _turnOrder;
        [SerializeField] private DialogView _handoff;
        [SerializeField] private DialogView _matchEnd;
        [SerializeField] private OptionListView _options;
        [SerializeField] private OptionListView _deckSelect;
        [SerializeField] private CardZoomView _zoom;

        private MatchFlow _flow;
        private TableInteraction _interaction;
        private HumanDecisionUi _decisions;

        private void Awake()
        {
            _layout.Configure(_settings.HandSpacing, _settings.HandFanDegrees, _settings.HandTiltDegrees);
            _pointer.Configure(_settings.LongPressSeconds, _settings.DragThresholdPixels);
            _actionBar.Init(_texts.Get(UiTextKeys.EndTurn), _texts.Get(UiTextKeys.Ready), _texts.Get(UiTextKeys.OpenField), _texts.Get(UiTextKeys.Cancel));
            _log.Init(_settings.LogLines);
            _topBar.Init(_texts.Get(UiTextKeys.ShowBothHands));

            var catalog = _database.ToCatalog();
            var rules = _rules.ToRules();
            var faces = new CardFaceBuilder(_database, _visual, _texts);
            var registry = new CardViewRegistry(_cardPrefab, _cardRoot, faces, _visual);
            var queue = new AnimationQueue();
            var sync = new TableSync(_layout, registry, _settings, _visual, _texts);
            var names = new PlayerNames(_texts);
            var lookup = new CardNameLookup(faces);
            var hotSeat = new HotSeatDirector();
            var selector = new ActionSelector();
            var humans = new[] { new LocalHumanController(PlayerId.Player1), new LocalHumanController(PlayerId.Player2) };
            var status = new StatusPresenter(_topBar, _battlePanel, names, lookup, _texts);

            _interaction = new TableInteraction(
                _pointer, new TableRaycaster(_camera), registry, selector, _zoom, faces, queue, _visual, _settings);
            _decisions = new HumanDecisionUi(
                humans, selector, _interaction, _actionBar, _turnOrder, _options, _prompt, names, lookup, _texts);

            var presenters = new IEventPresenter[]
            {
                new LogPresenter(new EventLogFormatter(_texts, names, lookup, faces), _log),
                new AbilityPlayedPresenter(_layout, registry, _settings),
                new TableMovePresenter(),
                new BattleResolvedPresenter(new BattleSummaryBuilder(_texts, names, lookup), _breakdown, _texts),
                new ScoutedPresenter(_breakdown, faces, _texts),
            };

            _flow = new MatchFlow(
                _decks,
                _deckSelect,
                names,
                _texts,
                () => new MatchPresenter(
                    new LocalMatchSession(catalog, rules),
                    presenters,
                    queue,
                    sync,
                    hotSeat,
                    humans,
                    status,
                    _handoff,
                    _matchEnd,
                    names,
                    lookup,
                    _texts,
                    () => _flow.Begin()),
                () =>
                {
                    queue.Clear();
                    registry.Clear();
                    lookup.Clear();
                    hotSeat.Reset();
                    _decisions.HideAll();
                    _log.Clear();
                    status.Hide();
                    _zoom.Hide();
                    foreach (var dialog in new[] { _breakdown, _turnOrder, _handoff, _matchEnd })
                        dialog.Hide();
                });

            _interaction.ResyncRequested += () => _flow.Presenter?.Resync();
            _topBar.ShowBothHandsChanged += show =>
            {
                hotSeat.ShowBothHands = show;
                _flow.Presenter?.Resync();
            };
        }

        private void Start() => _flow.Begin();

        private void OnDestroy()
        {
            _flow?.Presenter?.Dispose();
            _decisions?.Dispose();
            _interaction?.Dispose();
        }
    }
}
