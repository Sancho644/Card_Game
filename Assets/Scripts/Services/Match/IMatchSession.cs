using System;
using System.Collections.Generic;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Services.Match
{
    /// <summary>
    /// A running match as the client sees it: commands go in, events come out. The local implementation
    /// runs the engine in-process; a network one will talk to the server behind the same interface.
    /// </summary>
    public interface IMatchSession
    {
        /// <summary>Raised with every batch of events, including the ones produced by <see cref="Start"/>.</summary>
        event Action<IReadOnlyList<GameEvent>> EventsProduced;

        void Start(MatchSetup setup, ulong seed);

        CommandResult Submit(GameCommand command);

        PlayerView GetView(PlayerId viewer);

        IReadOnlyList<GameCommand> GetLegalActions(PlayerId player);
    }
}
