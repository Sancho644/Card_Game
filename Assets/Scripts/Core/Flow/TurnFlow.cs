using System;
using System.Linq;
using Game.Core.Cards;
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
            _state.BattleHappenedThisTurn = false;
            // Each player may play one legendary per turn, own or opponent's, battle included (docs/rules.md §7).
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
        public bool HasLegalMainAction(PlayerId player) => SkipReasonOf(player) == SkipReason.None;

        /// <summary>Why the player has no main action, or <see cref="SkipReason.None"/> if they have one.</summary>
        public SkipReason SkipReasonOf(PlayerId player)
        {
            var fields = _state.Fields.Where(field => !field.IsRemoved).ToList();
            if (_state.Player(player).HasCreatureInHand)
                return fields.Any(field => field.OccupantOf(player) == null) ? SkipReason.None : SkipReason.NoFreeField;

            if (!fields.Any(field => field.OccupantOf(player) != null))
                return SkipReason.NoCreatures;
            var hasTarget = fields.Any(field =>
                field.OccupantOf(player.Opponent()) != null && field.OccupantOf(player) == null);
            return hasTarget ? SkipReason.None : SkipReason.NoChallengeTarget;
        }

        /// <summary>Closes the "after" window. Without a main action the turn counts as a skip (docs/decisions.md).</summary>
        public void EndTurn(PlayerId player)
        {
            if (!_state.MainActionDone)
            {
                _state.ConsecutiveSkips++;
                _events.Emit(new MainActionSkipped(player, SkipReasonOf(player)));
            }

            _events.Emit(new TurnEnded(player));

            if (_state.ConsecutiveSkips >= SkipsToEndMatch)
                EndByScore();
            else
                StartTurn(player.Opponent());
        }

        public void EndMatch(PlayerId? winner, MatchEndReason reason)
        {
            _state.Phase = MatchPhase.Ended;
            _state.Battle = null;
            _state.Result = new MatchResult(winner, reason);
            _events.Emit(new MatchEnded(winner, reason));
        }

        /// <summary>
        /// The match ended without the wins needed (docs/rules.md §9): more wins decide; on equal wins, more
        /// creatures not out of the game (in hand and on the table); otherwise a draw. The only place of this rule.
        /// </summary>
        private void EndByScore()
        {
            var winner = Leader(player => _state.Player(player).Wins);
            if (winner != null)
            {
                EndMatch(winner, MatchEndReason.MoreWins);
                return;
            }

            winner = Leader(CreaturesInPlay);
            EndMatch(winner, winner == null ? MatchEndReason.Draw : MatchEndReason.MoreCreatures);
        }

        private int CreaturesInPlay(PlayerId player) =>
            _state.Player(player).Hand.Count(card => card.Type == CardType.Creature) +
            _state.Fields.Count(field => field.OccupantOf(player) != null);

        private static PlayerId? Leader(Func<PlayerId, int> score)
        {
            var first = score(PlayerId.Player1);
            var second = score(PlayerId.Player2);
            if (first == second)
                return null;
            return first > second ? PlayerId.Player1 : PlayerId.Player2;
        }
    }
}
