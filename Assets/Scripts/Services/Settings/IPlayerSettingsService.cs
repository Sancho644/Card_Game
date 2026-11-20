using System;

namespace Game.Services.Settings
{
    /// <summary>Settings kept on the current device.</summary>
    public interface IPlayerSettingsService
    {
        bool FastAnimations { get; set; }
        bool AudioMuted { get; set; }

        float Volume(AudioBus bus);
        void SetVolume(AudioBus bus, float value);
        void SaveAudio();

        event Action Changed;
    }
}
