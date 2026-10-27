using Game.Core.Cards;
using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    internal sealed class PlaceCreatureHandler : CommandHandler<PlaceCreature>
    {
        protected override RejectReason Validate(PlaceCreature command, MatchContext context)
        {
            var reason = context.Turns.CheckMainActionAllowed(command.Player);
            if (reason != RejectReason.None)
                return reason;

            var creature = context.State.Player(command.Player).FindInHand(command.CreatureId);
            if (creature == null)
                return RejectReason.CardNotInHand;
            if (creature.Type != CardType.Creature)
                return RejectReason.NotACreature;

            var field = context.State.FindField(command.FieldId);
            if (field == null)
                return RejectReason.FieldNotFound;
            if (field.IsRemoved)
                return RejectReason.FieldRemoved;
            if (field.OccupantOf(command.Player) != null)
                return RejectReason.AlreadyOnField;

            return command.SignatureAbilityId == null
                ? RejectReason.None
                : context.Abilities.ValidateWithPlacement(command.Player, command.SignatureAbilityId.Value, field, creature);
        }

        protected override void Apply(PlaceCreature command, MatchContext context)
        {
            var state = context.State;
            var player = state.Player(command.Player);
            var creature = player.FindInHand(command.CreatureId);
            var field = state.FindField(command.FieldId);

            player.RemoveFromHand(creature);
            field.Occupy(command.Player, creature, state.NextPlacementOrder());
            context.Events.Emit(new CreaturePlaced(command.Player, CardRef.From(creature), field.Id));
            context.Turns.CompleteMainAction();
            context.Abilities.StartBattle(command.Player, field, command.SignatureAbilityId);
        }
    }
}
