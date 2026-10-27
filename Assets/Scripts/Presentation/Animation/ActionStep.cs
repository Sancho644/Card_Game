using System;

namespace Game.Presentation.Animation
{
    /// <summary>Runs an action at its turn in the queue and moves on at once.</summary>
    public sealed class ActionStep : IAnimationStep
    {
        private readonly Action _action;

        public ActionStep(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Play(Action done)
        {
            _action();
            done();
        }

        public void Complete()
        {
        }
    }
}
