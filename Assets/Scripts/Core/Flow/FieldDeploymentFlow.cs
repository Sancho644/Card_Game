using System.Linq;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>One active field per owner; the remaining fields stay in a private reserve.</summary>
    internal static class FieldDeploymentFlow
    {
        // TODO(rules): Confirm mandatory free replacement at the next owner turn; see docs/decisions.md.
        public static bool Request(MatchState state, EventLog events, PlayerId player, ChoicePurpose purpose)
        {
            if (state.Fields.Any(f => f.Owner == player && f.IsActive)) return false;
            var options = state.Fields.Where(f => f.Owner == player && f.IsInReserve).Select(f => f.Card).ToList();
            if (options.Count == 0) return false;
            state.PendingChoice = new PendingChoice(player, purpose, options);
            state.Phase = MatchPhase.AwaitingChoice;
            events.Emit(new ChoiceRequested(player, purpose, options.Select(CardRef.From).ToList()));
            return true;
        }

        public static void ContinueSetup(MatchContext context)
        {
            foreach (var player in PlayerIds.All)
                if (Request(context.State, context.Events, player, ChoicePurpose.InitialField)) return;
            var winner = PlayerIds.All[context.Random.Next(PlayerIds.All.Count)];
            context.State.CoinTossWinner = winner;
            context.State.Phase = MatchPhase.ChoosingTurnOrder;
            context.Events.Emit(new CoinTossed(winner));
        }

        public static void Deploy(MatchContext context, int fieldId, ChoicePurpose purpose)
        {
            var field = context.State.FindField(fieldId);
            field.IsInReserve = false;
            context.State.PendingChoice = null;
            context.Events.Emit(new FieldsDealt(field.Owner, new[] { field.Id }));
            if (purpose == ChoicePurpose.InitialField) ContinueSetup(context);
            else context.State.Phase = MatchPhase.Turn;
        }
    }
}
