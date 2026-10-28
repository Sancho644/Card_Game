using System;
using System.Collections.Generic;
using Game.Core;
using Game.Core.State;
using Game.Presentation.Match;
using Game.Services.Match;

namespace Game.Presentation.App
{
    /// <summary>
    /// Everything the match scene needs to run a match prepared in the menu: the session, how to start it,
    /// who plays at this screen and the opponent connection to close when the match is left.
    /// </summary>
    public sealed class MatchLaunch
    {
        public MatchLaunch(
            IMatchSession session,
            MatchSetup setup,
            ulong seed,
            MatchMode mode,
            IReadOnlyList<PlayerId> localPlayers,
            IDisposable opponent)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Setup = setup ?? throw new ArgumentNullException(nameof(setup));
            Seed = seed;
            Mode = mode ?? throw new ArgumentNullException(nameof(mode));
            LocalPlayers = localPlayers ?? throw new ArgumentNullException(nameof(localPlayers));
            Opponent = opponent;
        }

        public IMatchSession Session { get; }
        public MatchSetup Setup { get; }
        public ulong Seed { get; }
        public MatchMode Mode { get; }

        /// <summary>Players deciding at this screen: both in hot-seat, one against an opponent.</summary>
        public IReadOnlyList<PlayerId> LocalPlayers { get; }

        /// <summary>The connected opponent (the AI host), or null in hot-seat.</summary>
        public IDisposable Opponent { get; }
    }
}
