using System;
using DG.Tweening;

namespace Game.Presentation.App
{
    /// <summary>Cancels a delayed call when disposed.</summary>
    public sealed class TweenHandle : IDisposable
    {
        private Tween _tween;

        public TweenHandle(Tween tween)
        {
            _tween = tween;
        }

        public void Dispose()
        {
            if (_tween != null && _tween.IsActive())
                _tween.Kill();
            _tween = null;
        }
    }
}
