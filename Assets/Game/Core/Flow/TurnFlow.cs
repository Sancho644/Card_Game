using System.Linq;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>Player turns, the main action and the end of the match.</summary>
    internal sealed class TurnFlow
    {
        /// <summary>"Both players skipped the main action in a row" (docs/rules.md §9).</summary>
        private const int SkipsToEndMatch = 2;

        private readonly MatchState _state;
        private readonly EventLog _events;

        public TurnFlow(MatchState state, EventLog events)
        {
            _state = state;
            _events = events;
        }

        public void StartTurn(PlayerId player)
        {
            _state.Phase = MatchPhase.Turn;
            _state.ActivePlayer = player;
            _state.MainActionDone = false;
            // TODO(rules): the legendary limit counts per player turn, battle included. See docs/decisions.md.
            foreach (var id in PlayerIds.All)
                _state.Player(id).LegendariesPlayedThisTurn = 0;
            _events.Emit(new TurnStarted(player));
        }

        public RejectReason CheckMainActionAllowed(PlayerId player)
        {
            if (_state.Phase != MatchPhase.Turn)
                return RejectReason.WrongPhase;
            if (player != _state.ActivePlayer)
                return RejectReason.NotYourTurn;
            return _state.MainActionDone ? RejectReason.MainActionAlreadyDone : RejectReason.None;
        }

        public void CompleteMainAction()
        {
            _state.MainActionDone = true;
            _state.ConsecutiveSkips = 0;
        }

        /// <summary>Whether the player could place a creature or, with no creatures in hand, challenge.</summary>
        public bool HasLegalMainAction(PlayerId player)
        {
            var fields = _state.Fields.Where(field => !field.IsRemoved).ToList();
            if (_state.Player(player).HasCreatureInHand)
                return fields.Any(field => field.OccupantOf(player) == null);

            var hasCreatureOnField = fields.Any(field => field.OccupantOf(player) != null);
            var hasTarget = fields.Any(field =>
                field.OccupantOf(player.Opponent()) != null && field.OccupantOf(player) == null);
            return hasCreatureOnField && hasTarget;
        }

        // TODO(rules): the "after" window is closed by an explicit EndTurn; without a main action it is a skip.
        // See docs/decisions.md.
        public void EndTurn(PlayerId player)
        {
            if (!_state.MainActionDone)
            {
                _state.ConsecutiveSkips++;
                _events.Emit(new MainActionSkipped(player));
            }

            _events.Emit(new TurnEnded(player));

            if (_state.ConsecutiveSkips >= SkipsToEndMatch)
                EndMatch(LeaderByWins());
            else
                StartTurn(player.Opponent());
        }

        public void EndMatch(PlayerId? winner)
        {
            _state.Phase = MatchPhase.Ended;
            _state.Battle = null;
            _state.Result = new MatchResult(winner);
            _events.Emit(new MatchEnded(winner));
        }

        private PlayerId? LeaderByWins()
        {
            var first = _state.Player(PlayerId.Player1).Wins;
            var second = _state.Player(PlayerId.Player2).Wins;
            if (first == second)
                return null;
            return first > second ? PlayerId.Player1 : PlayerId.Player2;
        }
    }
}
