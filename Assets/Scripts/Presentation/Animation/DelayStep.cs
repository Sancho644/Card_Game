using System;
using DG.Tweening;

namespace Game.Presentation.Animation
{
    public sealed class DelayStep : IAnimationStep
    {
        private readonly float _seconds;
        private Tween _delay;

        public DelayStep(float seconds)
        {
            _seconds = seconds;
        }

        public void Play(Action done) => _delay = DOVirtual.DelayedCall(_seconds, () => done());

        public void Complete() => _delay?.Kill();
    }
}
