using System;
using Game.Core.State;
using Game.Services.Match;

namespace Game.Services.Matchmaking
{
    /// <summary>Connects a local opponent (the AI) to a session; disposing the result disconnects it.</summary>
    public interface IOpponentFactory
    {
        IDisposable Attach(IMatchSession session, PlayerId player, string difficulty, ulong seed);
    }
}
