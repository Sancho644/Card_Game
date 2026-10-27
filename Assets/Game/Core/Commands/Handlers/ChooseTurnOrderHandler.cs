using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    internal sealed class ChooseTurnOrderHandler : CommandHandler<ChooseTurnOrder>
    {
        protected override RejectReason Validate(ChooseTurnOrder command, MatchContext context)
        {
            if (context.State.Phase != MatchPhase.ChoosingTurnOrder)
                return RejectReason.WrongPhase;
            return command.Player == context.State.CoinTossWinner ? RejectReason.None : RejectReason.NotCoinTossWinner;
        }

        protected override void Apply(ChooseTurnOrder command, MatchContext context)
        {
            var first = command.GoFirst ? command.Player : command.Player.Opponent();
            context.Events.Emit(new TurnOrderChosen(command.Player, first));
            context.Turns.StartTurn(first);
        }
    }
}
