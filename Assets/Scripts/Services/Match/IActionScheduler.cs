using System;

namespace Game.Services.Match
{
    /// <summary>
    /// Runs an action later: after a delay in the scene, in order and without delay in a simulation.
    /// Never runs it inside the call, so a decision is never made in the middle of an event batch.
    /// </summary>
    public interface IActionScheduler
    {
        /// <summary>Disposing the handle cancels the action if it has not run yet.</summary>
        IDisposable Schedule(float delaySeconds, Action action);
    }
}
