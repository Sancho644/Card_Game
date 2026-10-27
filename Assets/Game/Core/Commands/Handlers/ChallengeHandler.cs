using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    internal sealed class ChallengeHandler : CommandHandler<Challenge>
    {
        protected override RejectReason Validate(Challenge command, MatchContext context)
        {
            var reason = context.Turns.CheckMainActionAllowed(command.Player);
            if (reason != RejectReason.None)
                return reason;

            var state = context.State;
            if (state.Player(command.Player).HasCreatureInHand)
                return RejectReason.HandHasCreatures;
            if (state.FindFieldWith(command.Player, command.CreatureId) == null)
                return RejectReason.CreatureNotOnField;

            var target = state.FindField(command.TargetFieldId);
            if (target == null)
                return RejectReason.FieldNotFound;
            if (target.IsRemoved)
                return RejectReason.FieldRemoved;
            if (target.OccupantOf(command.Player) != null)
                return RejectReason.AlreadyOnField;
            return target.OccupantOf(command.Player.Opponent()) == null
                ? RejectReason.NoOpponentCreature
                : RejectReason.None;
        }

        protected override void Apply(Challenge command, MatchContext context)
        {
            var state = context.State;
            var source = state.FindFieldWith(command.Player, command.CreatureId);
            var target = state.FindField(command.TargetFieldId);

            // TODO(rules): the moved creature counts as placed on the target now, so the opponent
            // (already standing there) acts first in the battle. See docs/decisions.md.
            var creature = source.Vacate(command.Player);
            target.Occupy(command.Player, creature, state.NextPlacementOrder());
            context.Events.Emit(new CreatureMoved(command.Player, creature.Id, source.Id, target.Id));
            context.Turns.CompleteMainAction();
            context.Battles.StartIfContested(target);
        }
    }
}
