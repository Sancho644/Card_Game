using System;
using System.Linq;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Events;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>
    /// Starting a battle, passing turns inside it and applying its result.
    /// One action makes at most one field contested, and a battle is resolved before the next action,
    /// so battles always run one after another in the order they arise.
    /// </summary>
    internal sealed class BattleFlow
    {
        private readonly MatchState _state;
        private readonly GameRules _rules;
        private readonly EventLog _events;
        private readonly TurnFlow _turns;
        private readonly BattleResolver _resolver;

        public BattleFlow(MatchState state, GameRules rules, EventLog events, TurnFlow turns, BattleResolver resolver)
        {
            _state = state;
            _rules = rules;
            _events = events;
            _turns = turns;
            _resolver = resolver;
        }

        /// <summary>Starts a battle if both players now have a creature on the field.</summary>
        public void StartIfContested(FieldSlot field)
        {
            if (field.IsContested)
                Announce(Open(field));
        }

        /// <summary>Creates the battle without announcing it, so a signature ability can be played before it starts.</summary>
        public BattleState Open(FieldSlot field)
        {
            if (_state.Battle != null)
                throw new InvalidOperationException("A battle is already in progress.");

            var battle = new BattleState(field, field.EarlierOccupant);
            _state.Battle = battle;
            _state.Phase = MatchPhase.Battle;
            return battle;
        }

        public void Announce(BattleState battle) =>
            _events.Emit(new BattleStarted(battle.Field.Id, battle.Host, battle.Guest, battle.CurrentActor));

        public void PassTurn()
        {
            var battle = _state.Battle;
            battle.CurrentActor = battle.CurrentActor.Opponent();
            _events.Emit(new BattleTurnPassed(battle.CurrentActor));
        }

        public void ResetReadiness(PlayerId player)
        {
            var battle = _state.Battle;
            if (!battle.IsReady(player))
                return;

            battle.SetReady(player, false);
            _events.Emit(new ReadinessReset(player));
        }

        /// <summary>Turns the battle's field face up, unless it is locked.</summary>
        public void OpenField()
        {
            var field = _state.Battle.Field;
            if (field.IsOpen || field.IsLocked)
                return;

            field.IsOpen = true;
            _events.Emit(new FieldOpened(field.Id, field.Definition.Id));
        }

        /// <summary>Cancels the player's last ability in force that matches the filter, if any.</summary>
        public void CancelLastAbility(PlayerId player, Func<AbilityCard, bool> filter)
        {
            var target = _state.Battle.ActiveAbilities.LastOrDefault(played => played.Player == player && filter(played.Card));
            if (target == null)
                return;

            target.IsCancelled = true;
            _events.Emit(new AbilityCancelled(player, CardRef.From(target.Instance)));
        }

        /// <summary>
        /// Moves a creature to another field. It counts as placed at the moment of moving (docs/rules.md §6).
        /// </summary>
        public void MoveCreature(PlayerId owner, int creatureId, int fieldId)
        {
            var source = _state.FindFieldWith(owner, creatureId);
            var target = _state.FindField(fieldId);
            var creature = source.Vacate(owner);
            target.Occupy(owner, creature, _state.NextPlacementOrder());
            _events.Emit(new CreatureMoved(owner, creature.Id, source.Id, target.Id));
            StartIfContested(target);
        }

        /// <summary>
        /// The player's creature returns to hand, the battle ends without a winner, the opponent's creature
        /// stays and an open field is removed.
        /// </summary>
        public void Retreat(PlayerId player)
        {
            var field = _state.Battle.Field;
            _events.Emit(new BattleRetreated(field.Id, player));

            var creature = field.Vacate(player);
            _state.Player(player).AddToHand(creature);
            _events.Emit(new CreatureReturnedToHand(player, creature.Id));

            // TODO(rules): the retreat card text removes an open field; «Нерушимая» is not consulted,
            // since it speaks of the field after a resolved battle. The opponent's creature cannot stay
            // on a removed field, so it returns to its owner's hand. See docs/decisions.md.
            if (field.IsOpen)
            {
                var opponent = player.Opponent();
                var staying = field.Vacate(opponent);
                _state.Player(opponent).AddToHand(staying);
                _events.Emit(new CreatureReturnedToHand(opponent, staying.Id));
                RemoveField(field);
            }

            FinishBattle(field);
        }

        /// <summary>Applies the result. The turn then continues for the active player: the "after" window (docs/rules.md §5).</summary>
        public void Resolve()
        {
            var battle = _state.Battle;
            var resolution = _resolver.Resolve(battle);
            _events.Emit(new BattleResolved(battle.Field.Id, resolution.HostPower, resolution.GuestPower, resolution.Winner));

            var consequences = resolution.Consequences;
            ApplyConsequences(battle.Field, consequences);
            FinishBattle(battle.Field);

            if (consequences.WinScoredBy != null)
            {
                var winner = consequences.WinScoredBy.Value;
                var player = _state.Player(winner);
                player.Wins++;
                _events.Emit(new WinScored(winner, player.Wins));
                if (player.Wins >= _rules.WinsToWin)
                {
                    _turns.EndMatch(winner);
                    return;
                }
            }

            foreach (var request in consequences.Choices)
            {
                if (TryRequestChoice(request))
                    return;
            }
        }

        private void ApplyConsequences(FieldSlot field, BattleConsequences consequences)
        {
            foreach (var id in PlayerIds.All)
            {
                var fate = consequences.FateOf(id);
                if (fate == CreatureFate.StaysOnField)
                    continue;

                var creature = field.Vacate(id);
                if (fate == CreatureFate.ReturnsToHand)
                {
                    _state.Player(id).AddToHand(creature);
                    _events.Emit(new CreatureReturnedToHand(id, creature.Id));
                }
                else
                {
                    _state.Player(id).AddToOutOfGame(creature);
                    _events.Emit(new CreatureOut(id, creature.Id));
                }
            }

            if (consequences.FieldRemoved)
                RemoveField(field);
        }

        private void RemoveField(FieldSlot field)
        {
            field.IsRemoved = true;
            _events.Emit(new FieldRemoved(field.Id));
        }

        private void FinishBattle(FieldSlot field)
        {
            field.IsLocked = false;
            _state.Battle = null;
            _state.Phase = MatchPhase.Turn;
            _state.BattleHappenedThisTurn = true;
        }

        /// <summary>Asks the player to choose among matching cards in their discard pile. False if there is nothing to choose.</summary>
        private bool TryRequestChoice(ChoiceRequest request)
        {
            var options = _state.Player(request.Player).Discard.Where(request.Filter).ToList();
            if (options.Count == 0)
                return false;

            _state.PendingChoice = new PendingChoice(request.Player, request.Purpose, options);
            _state.Phase = MatchPhase.AwaitingChoice;
            _events.Emit(new ChoiceRequested(request.Player, request.Purpose, options.Select(CardRef.From).ToList()));
            return true;
        }
    }
}
