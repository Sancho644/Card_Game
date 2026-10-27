using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    /// <summary>Power is counted when both players become ready one after the other.</summary>
    internal sealed class ReadyHandler : CommandHandler<Ready>
    {
        protected override RejectReason Validate(Ready command, MatchContext context)
        {
            if (context.State.Phase != MatchPhase.Battle)
                return RejectReason.WrongPhase;
            return context.State.Battle.CurrentActor == command.Player ? RejectReason.None : RejectReason.NotYourTurn;
        }

        protected override void Apply(Ready command, MatchContext context)
        {
            var battle = context.State.Battle;
            battle.SetReady(command.Player, true);
            context.Events.Emit(new PlayerReady(command.Player));

            if (battle.IsReady(command.Player.Opponent()))
                context.Battles.Resolve();
            else
                context.Battles.PassTurn();
        }
    }
}
