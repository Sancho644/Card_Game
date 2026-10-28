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

        /// <summary>
        /// An optional decision while the match waits for the opponent: actions the rules allow out of turn,
        /// e.g. an ability on the opponent's battle turn. The controller may submit one or do nothing.
        /// A new event batch cancels the offer through <see cref="CancelDecision"/>.
        /// </summary>
        void OfferReaction(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit);

        /// <summary>The decision is no longer needed, e.g. a new match was started. The controller may update its interface.</summary>
        void CancelDecision();

        /// <summary>
        /// Drops a pending decision without any visible reaction: the match is being shut down and the
        /// interface may already be destroyed.
        /// </summary>
        void AbandonDecision();
    }
}
