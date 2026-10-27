using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    /// <summary>
    /// Opening is a free action: the host still plays an ability or presses Ready afterwards.
    /// It resets the opponent's readiness so they can respond (agreed with the author).
    /// </summary>
    internal sealed class OpenFieldHandler : CommandHandler<OpenField>
    {
        protected override RejectReason Validate(OpenField command, MatchContext context)
        {
            var battle = context.State.Battle;
            if (context.State.Phase != MatchPhase.Battle)
                return RejectReason.WrongPhase;
            if (battle.CurrentActor != command.Player)
                return RejectReason.NotYourTurn;
            if (battle.Host != command.Player)
                return RejectReason.NotFieldOwner;
            return battle.Field.IsOpen ? RejectReason.FieldAlreadyOpen : RejectReason.None;
        }

        protected override void Apply(OpenField command, MatchContext context)
        {
            var field = context.State.Battle.Field;
            field.IsOpen = true;
            context.Events.Emit(new FieldOpened(field.Id, field.Definition.Id));
            context.Battles.ResetReadiness(command.Player.Opponent());
        }
    }
}
