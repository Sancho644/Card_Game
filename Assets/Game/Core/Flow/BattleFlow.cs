using Game.Core.Battle;
using Game.Core.Events;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>Starting a battle, passing turns inside it and applying its result.</summary>
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
            if (!field.IsContested)
                return;

            var battle = new BattleState(field, field.EarlierOccupant);
            _state.Battle = battle;
            _state.Phase = MatchPhase.Battle;
            _events.Emit(new BattleStarted(field.Id, battle.Host, battle.Guest, battle.CurrentActor));
        }

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

        /// <summary>Applies the result. The turn then continues for the active player: the "after" window (docs/rules.md §5).</summary>
        public void Resolve()
        {
            var battle = _state.Battle;
            var resolution = _resolver.Resolve(battle);
            _events.Emit(new BattleResolved(battle.Field.Id, resolution.HostPower, resolution.GuestPower, resolution.Winner));

            ApplyConsequences(battle.Field, resolution.Consequences);
            _state.Battle = null;
            _state.Phase = MatchPhase.Turn;

            var scorer = resolution.Consequences.WinScoredBy;
            if (scorer == null)
                return;

            var player = _state.Player(scorer.Value);
            player.Wins++;
            _events.Emit(new WinScored(scorer.Value, player.Wins));
            if (player.Wins >= _rules.WinsToWin)
                _turns.EndMatch(scorer.Value);
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
            {
                field.IsRemoved = true;
                _events.Emit(new FieldRemoved(field.Id));
            }
        }
    }
}
