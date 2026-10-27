using System;
using System.Collections.Generic;
using Game.Core.Commands;
using Game.Core.State;

namespace Game.Services.Match
{
    /// <summary>
    /// Where a player's decisions come from: a local human through the UI, the AI, later the network.
    /// The controller picks one of the legal actions and passes it to <c>submit</c>.
    /// </summary>
    public interface IPlayerController
    {
        PlayerId Player { get; }

        void RequestDecision(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit);

        /// <summary>The decision is no longer needed, e.g. a new match was started.</summary>
        void CancelDecision();
    }
}
