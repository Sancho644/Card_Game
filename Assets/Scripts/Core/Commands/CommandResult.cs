using System.Collections.Generic;
using Game.Core.Events;

namespace Game.Core.Commands
{
    public sealed class CommandResult
    {
        private static readonly IReadOnlyList<GameEvent> NoEvents = new GameEvent[0];

        private CommandResult(RejectReason reason, IReadOnlyList<GameEvent> events)
        {
            Reason = reason;
            Events = events;
        }

        public RejectReason Reason { get; }
        public bool IsAccepted => Reason == RejectReason.None;
        public IReadOnlyList<GameEvent> Events { get; }

        public static CommandResult Accepted(IReadOnlyList<GameEvent> events) =>
            new CommandResult(RejectReason.None, events);

        public static CommandResult Rejected(RejectReason reason) => new CommandResult(reason, NoEvents);
    }
}
