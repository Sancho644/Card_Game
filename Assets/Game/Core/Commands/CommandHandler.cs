using System;
using Game.Core.Flow;

namespace Game.Core.Commands
{
    internal interface ICommandHandler
    {
        Type CommandType { get; }
        RejectReason Validate(GameCommand command, MatchContext context);
        void Apply(GameCommand command, MatchContext context);
    }

    /// <summary>Validation and execution of one command type. Apply runs only after Validate returned None.</summary>
    internal abstract class CommandHandler<TCommand> : ICommandHandler
        where TCommand : GameCommand
    {
        public Type CommandType => typeof(TCommand);

        public RejectReason Validate(GameCommand command, MatchContext context) =>
            Validate((TCommand)command, context);

        public void Apply(GameCommand command, MatchContext context) => Apply((TCommand)command, context);

        protected abstract RejectReason Validate(TCommand command, MatchContext context);

        protected abstract void Apply(TCommand command, MatchContext context);
    }
}
