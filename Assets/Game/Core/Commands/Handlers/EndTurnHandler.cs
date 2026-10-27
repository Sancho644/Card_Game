using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    internal sealed class EndTurnHandler : CommandHandler<EndTurn>
    {
        protected override RejectReason Validate(EndTurn command, MatchContext context)
        {
            var state = context.State;
            if (state.Phase != MatchPhase.Turn)
                return RejectReason.WrongPhase;
            if (command.Player != state.ActivePlayer)
                return RejectReason.NotYourTurn;
            return !state.MainActionDone && context.Turns.HasLegalMainAction(command.Player)
                ? RejectReason.MainActionRequired
                : RejectReason.None;
        }

        protected override void Apply(EndTurn command, MatchContext context) => context.Turns.EndTurn(command.Player);
    }
}
