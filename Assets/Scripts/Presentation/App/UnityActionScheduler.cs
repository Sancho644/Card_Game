using System;
using DG.Tweening;
using Game.Services.Match;

namespace Game.Presentation.App
{
    /// <summary>Runs scheduled actions on the main thread after a real-time delay, at the earliest on the next frame.</summary>
    public sealed class UnityActionScheduler : IActionScheduler
    {
        public IDisposable Schedule(float delaySeconds, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            return new TweenHandle(DOVirtual.DelayedCall(Math.Max(0f, delaySeconds), () => action(), ignoreTimeScale: true));
        }
    }
}
