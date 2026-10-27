using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    /// <summary>
    /// Opening is a free action: the host still plays an ability or presses Ready afterwards.
    /// It resets the opponent's readiness so they can respond (docs/rules.md §6).
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
            if (battle.Field.IsOpen)
                return RejectReason.FieldAlreadyOpen;
            return battle.Field.IsLocked ? RejectReason.FieldLocked : RejectReason.None;
        }

        protected override void Apply(OpenField command, MatchContext context)
        {
            context.Battles.OpenField();
            context.Battles.ResetReadiness(command.Player.Opponent());
        }
    }
}
