using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Table;
using Game.Presentation.Ui;
using Game.Services.Match;
using UnityEngine;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Runs one match on the table: turns each batch of engine events into animation steps, then asks
    /// the deciding player's controller for the next command. In hot-seat a hand-off screen comes first
    /// whenever the deciding player changes. Against an opponent elsewhere (the AI, later the network) only
    /// the local player has a controller here; while the opponent decides, the local player is offered the
    /// actions the rules allow out of turn.
    /// </summary>
    public sealed class MatchPresenter : IDisposable
    {
        private readonly IMatchSession _session;
        private readonly IReadOnlyList<IEventPresenter> _presenters;
        private readonly AnimationQueue _queue;
        private readonly TableSync _sync;
        private readonly HotSeatDirector _hotSeat;
        private readonly IReadOnlyList<IPlayerController> _controllers;
        private readonly MatchMode _mode;
        private readonly StatusPresenter _status;
        private readonly BattlePowerPresenter _power;
        private readonly DialogView _handoff;
        private readonly DialogView _matchEnd;
        private readonly PlayerNames _names;
        private readonly CardNameLookup _lookup;
        private readonly UiTexts _texts;
        private readonly Action _newMatch;
        private PlayerView _lastView;
        private bool _disposed;

        public MatchPresenter(
            IMatchSession session,
            IReadOnlyList<IEventPresenter> presenters,
            AnimationQueue queue,
            TableSync sync,
            HotSeatDirector hotSeat,
            IReadOnlyList<IPlayerController> controllers,
            MatchMode mode,
            StatusPresenter status,
            BattlePowerPresenter power,
            DialogView handoff,
            DialogView matchEnd,
            PlayerNames names,
            CardNameLookup lookup,
            UiTexts texts,
            Action newMatch)
        {
            _session = session;
            _presenters = presenters;
            _queue = queue;
            _sync = sync;
            _hotSeat = hotSeat;
            _controllers = controllers;
            _mode = mode;
            _status = status;
            _power = power;
            _handoff = handoff;
            _matchEnd = matchEnd;
            _names = names;
            _lookup = lookup;
            _texts = texts;
            _newMatch = newMatch;
        }

        /// <summary>The table is about to be handed to the other hot-seat player: nothing private may stay on screen.</summary>
        public event Action HandoffStarted;

        public void Start(MatchSetup setup, ulong seed)
        {
            _session.EventsProduced += OnEvents;
            _session.Start(setup, seed);
        }

        /// <summary>Moves every card back to its place, e.g. after a cancelled drag or the debug switch.</summary>
        public void Resync()
        {
            if (!_disposed)
                _queue.Enqueue(CreateSync());
        }

        /// <summary>
        /// Stops reacting to the match. Touches no views: it runs both before a new match, when the table is
        /// reset separately, and when the scene is unloading and the views may already be destroyed.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _session.EventsProduced -= OnEvents;
            foreach (var controller in _controllers)
                controller.AbandonDecision();
        }

        private void OnEvents(IReadOnlyList<GameEvent> batch)
        {
            if (_disposed)
                return;

            // The state changed, possibly by the opponent while a local player was deciding: ask again later.
            foreach (var controller in _controllers)
                controller.CancelDecision();

            var view = ViewerView();
            _lookup.Remember(view);
            var context = new PresentationContext(batch, _lastView, view, _queue, CreateSync);
            for (var i = 0; i < batch.Count; i++)
            {
                if (!_hotSeat.IsVisible(batch[i]))
                    continue;
                context.Index = i;
                foreach (var presenter in _presenters)
                    presenter.Present(batch[i], context);
            }

            context.EnqueueSync();
            _power.Present(_session.GetBattlePower(), context);
            _queue.Enqueue(RequestDecision);
        }

        private void RequestDecision()
        {
            if (_disposed)
                return;

            var view = ViewerView();
            _lastView = view;
            _lookup.Remember(view);
            _status.Show(view, _session.GetBattlePower());

            if (view.Phase == MatchPhase.Ended)
            {
                ShowMatchEnd(view);
                return;
            }

            var awaiting = view.AwaitingPlayer;
            if (awaiting == null)
                return;

            var controller = _controllers.FirstOrDefault(candidate => candidate.Player == awaiting.Value);
            if (controller == null)
            {
                OfferReactions();
                return;
            }

            if (_mode.ShowsHandoff && awaiting.Value != _hotSeat.Viewer)
            {
                _queue.Enqueue(HandoffStep(awaiting.Value));
                _queue.Enqueue(RequestDecision);
                return;
            }

            controller.RequestDecision(view, _session.GetLegalActions(awaiting.Value), Submit);
        }

        /// <summary>The opponent decides elsewhere; local players may still play what the rules allow on its turn.</summary>
        private void OfferReactions()
        {
            if (!_mode.OffersReactions)
                return;

            foreach (var controller in _controllers)
            {
                var legal = _session.GetLegalActions(controller.Player);
                if (legal.Count > 0)
                    controller.OfferReaction(_session.GetView(controller.Player), legal, Submit);
            }
        }

        private void Submit(GameCommand command)
        {
            if (_disposed)
                return;

            var result = _session.Submit(command);
            if (result.IsAccepted)
                return;

            Debug.LogError($"The engine rejected an offered action {command.GetType().Name}: {result.Reason}");
            _queue.Enqueue(RequestDecision);
        }

        /// <summary>An opaque screen hides the table; the table turns to the next player while it is up.</summary>
        private IAnimationStep HandoffStep(PlayerId next) =>
            new WaitForPlayerStep(done =>
            {
                HandoffStarted?.Invoke();
                _status.Hide();
                _handoff.Show(
                    _texts.Format(UiTextKeys.HandoffTitle, _names.Of(next)),
                    _texts.Format(UiTextKeys.HandoffHint, _names.Of(next)),
                    _texts.Get(UiTextKeys.HandoffReady),
                    () =>
                    {
                        _hotSeat.Viewer = next;
                        _lastView = ViewerView();

                        // Names the previous viewer could see must not leak to the next one.
                        _lookup.Clear();
                        _lookup.Remember(_lastView);
                        _sync.Sync(_lastView, RevealedOpponent(), instant: true);
                        done();
                    });
            });

        private void ShowMatchEnd(PlayerView view)
        {
            var result = view.Result;
            var title = result == null || result.IsDraw
                ? _texts.Get(UiTextKeys.EndDraw)
                : _texts.Format(UiTextKeys.EndWinner, _names.Of(result.Winner.Value));
            var score = _texts.Format(
                UiTextKeys.EndScore,
                _names.Of(PlayerId.Player1),
                view.Wins(PlayerId.Player1),
                _names.Of(PlayerId.Player2),
                view.Wins(PlayerId.Player2));
            _matchEnd.Show(title, score, _texts.Get(UiTextKeys.ToMenu), _newMatch);
        }

        private IAnimationStep CreateSync() =>
            new TweenStep(() => _sync.Sync(ViewerView(), RevealedOpponent(), instant: false));

        private PlayerView ViewerView() => _session.GetView(_hotSeat.Viewer);

        private PlayerView RevealedOpponent() =>
            _hotSeat.ShowBothHands ? _session.GetView(_hotSeat.Viewer.Opponent()) : null;
    }
}
