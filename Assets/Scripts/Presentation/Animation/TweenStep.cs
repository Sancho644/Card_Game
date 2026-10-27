using System;
using DG.Tweening;

namespace Game.Presentation.Animation
{
    /// <summary>Plays a DOTween tween created at the step's turn, so it starts from the current state.</summary>
    public sealed class TweenStep : IAnimationStep
    {
        private readonly Func<Tween> _create;
        private Tween _tween;

        public TweenStep(Func<Tween> create)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
        }

        public void Play(Action done)
        {
            _tween = _create();
            if (_tween == null || !_tween.IsActive() || _tween.Duration() <= 0f)
            {
                _tween?.Complete();
                done();
                return;
            }

            _tween.OnComplete(() => done());
        }

        public void Complete()
        {
            if (_tween != null && _tween.IsActive())
                _tween.Kill(complete: true);
        }
    }
}
