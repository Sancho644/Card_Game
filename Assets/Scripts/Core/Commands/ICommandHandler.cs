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
}
