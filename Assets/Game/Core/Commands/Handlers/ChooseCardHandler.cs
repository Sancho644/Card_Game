using System.Linq;
using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    internal sealed class ChooseCardHandler : CommandHandler<ChooseCard>
    {
        protected override RejectReason Validate(ChooseCard command, MatchContext context)
        {
            var choice = context.State.PendingChoice;
            if (context.State.Phase != MatchPhase.AwaitingChoice)
                return RejectReason.WrongPhase;
            if (choice.Player != command.Player)
                return RejectReason.NotYourTurn;
            return choice.Options.Any(card => card.Id == command.CardId) ? RejectReason.None : RejectReason.NotAChoiceOption;
        }

        protected override void Apply(ChooseCard command, MatchContext context)
        {
            var state = context.State;
            var choice = state.PendingChoice;
            var card = choice.Options.First(option => option.Id == command.CardId);

            // The only purpose so far: take an ability from the discard pile into the hand.
            state.Player(command.Player).MoveDiscardToHand(card);
            context.Events.Emit(new AbilityReturnedToHand(command.Player, CardRef.From(card)));

            state.PendingChoice = null;
            state.Phase = MatchPhase.Turn;
        }
    }
}
