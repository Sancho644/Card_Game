using System;

namespace Game.Presentation.Animation
{
    /// <summary>One step of the presentation: a tween, a pause, a panel waiting for the player.</summary>
    public interface IAnimationStep
    {
        /// <summary>Starts the step; <paramref name="done"/> is called exactly once when it ends.</summary>
        void Play(Action done);

        /// <summary>Jumps to the end state without calling done; used when the queue is cleared.</summary>
        void Complete();
    }
}
