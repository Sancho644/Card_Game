using System;
using Game.Core;
using Game.Core.State;
using Game.Services.Match;

namespace Game.Services.Matchmaking
{
    /// <summary>
    /// A match ready to start: the session to play through and the seat of the local player. The opponent is
    /// already connected to the session; disposing <see cref="Opponent"/> disconnects it when the match closes.
    /// </summary>
    public sealed class MatchFound
    {
        public MatchFound(IMatchSession session, PlayerId localPlayer, MatchSetup setup, ulong seed, IDisposable opponent)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            LocalPlayer = localPlayer;
            Setup = setup ?? throw new ArgumentNullException(nameof(setup));
            Seed = seed;
            Opponent = opponent;
        }

        public IMatchSession Session { get; }
        public PlayerId LocalPlayer { get; }
        public MatchSetup Setup { get; }
        public ulong Seed { get; }
        public IDisposable Opponent { get; }
    }
}
