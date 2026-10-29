using System;
using DG.Tweening;
using Game.Data.Presentation;
using Game.Services.Settings;

namespace Game.Presentation.Animation
{
    /// <summary>
    /// The speed of every table animation: the designer's speed, the player's «fast animations» and the
    /// temporary speed-up while the player taps during animations. Tweens and pauses run on DOTween's time
    /// scale, so durations stay as authored in <see cref="AnimationSettings"/>.
    /// </summary>
    public sealed class AnimationSpeed : IDisposable
    {
        private readonly AnimationSettings _settings;
        private readonly IPlayerSettingsService _player;
        private bool _fastForward;

        public AnimationSpeed(AnimationSettings settings, IPlayerSettingsService player)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _player.Changed += Apply;
            Apply();
        }

        /// <summary>The time scale for the given state.</summary>
        public static float TimeScale(AnimationSettings settings, bool fast, bool fastForward) =>
            settings.Speed * (fast ? settings.FastSpeed : 1f) * (fastForward ? settings.TapFastForward : 1f);

        /// <summary>Speeds up what is playing now, until <see cref="EndFastForward"/>.</summary>
        public void FastForward()
        {
            _fastForward = true;
            Apply();
        }

        public void EndFastForward()
        {
            _fastForward = false;
            Apply();
        }

        /// <summary>Stops following the player's settings and leaves DOTween at normal speed.</summary>
        public void Dispose()
        {
            _player.Changed -= Apply;
            DOTween.timeScale = 1f;
        }

        private void Apply() => DOTween.timeScale = TimeScale(_settings, _player.FastAnimations, _fastForward);
    }
}
