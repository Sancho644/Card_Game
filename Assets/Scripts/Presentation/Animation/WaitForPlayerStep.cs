using System;

namespace Game.Presentation.Animation
{
    /// <summary>Shows something the player must confirm (a panel, a hand-off screen) and waits for it.</summary>
    public sealed class WaitForPlayerStep : IAnimationStep
    {
        private readonly Action<Action> _show;

        /// <param name="show">Shows the panel and calls the given callback when the player confirms.</param>
        public WaitForPlayerStep(Action<Action> show)
        {
            _show = show ?? throw new ArgumentNullException(nameof(show));
        }

        public void Play(Action done) => _show(done);

        public void Complete()
        {
        }
    }
}
