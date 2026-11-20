using System;

namespace Game.Services.Settings
{
    /// <summary>Player settings kept in a local store on this device.</summary>
    public sealed class LocalPlayerSettingsService : IPlayerSettingsService
    {
        private const string FastAnimationsKey = "settings.fast-animations";
        private const string MutedKey = "settings.audio.muted";
        private const int VolumeSteps = 100;
        private static readonly string[] VolumeKeys =
        {
            "settings.audio.master", "settings.audio.music", "settings.audio.sfx", "settings.audio.ui"
        };

        private readonly ISettingsStore _store;
        private bool _audioDirty;

        public LocalPlayerSettingsService(ISettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public event Action Changed;

        public bool AudioMuted
        {
            get => _store.GetInt(MutedKey, 0) != 0;
            set
            {
                if (AudioMuted == value)
                    return;

                _store.SetInt(MutedKey, value ? 1 : 0);
                _audioDirty = true;
                SaveAudio();
                Changed?.Invoke();
            }
        }

        public bool FastAnimations
        {
            get => _store.GetInt(FastAnimationsKey, 0) != 0;
            set
            {
                if (value == FastAnimations)
                    return;
                _store.SetInt(FastAnimationsKey, value ? 1 : 0);
                _store.Save();
                Changed?.Invoke();
            }
        }

        public float Volume(AudioBus bus) =>
            ClampPercent(_store.GetInt(VolumeKeys[(int)bus], VolumeSteps)) / (float)VolumeSteps;

        public void SetVolume(AudioBus bus, float value)
        {
            if (float.IsNaN(value))
                return;

            var key = VolumeKeys[(int)bus];
            var clamped = Math.Max(0, Math.Min(1, value));
            var percent = (int)Math.Round(clamped * VolumeSteps);
            if (percent == _store.GetInt(key, VolumeSteps))
                return;

            _store.SetInt(key, percent);
            _audioDirty = true;
            Changed?.Invoke();
        }

        public void SaveAudio()
        {
            if (!_audioDirty)
                return;

            _store.Save();
            _audioDirty = false;
        }

        private static int ClampPercent(int percent) => Math.Max(0, Math.Min(VolumeSteps, percent));
    }
}
