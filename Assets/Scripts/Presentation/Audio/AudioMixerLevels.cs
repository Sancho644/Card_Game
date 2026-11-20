using System;
using Game.Services.Settings;
using UnityEngine;

namespace Game.Presentation.Audio
{
    /// <summary>Applies user gain at the mixer, with a source-level fallback for missing routes.</summary>
    public sealed class AudioMixerLevels
    {
        private static readonly string[] Parameters = { "MasterVolume", "MusicVolume", "SfxVolume", "UiVolume" };
        private readonly Func<string, float, bool> _setParameter;
        private readonly float[] _levels = { 1f, 1f, 1f, 1f };
        private readonly bool[] _applied = new bool[Parameters.Length];

        public AudioMixerLevels(Func<string, float, bool> setParameter) => _setParameter = setParameter;

        public void Apply(IPlayerSettingsService settings)
        {
            for (var i = 0; i < Parameters.Length; i++)
            {
                var bus = (AudioBus)i;
                _levels[i] = bus == AudioBus.Master && settings.AudioMuted ? 0f : settings.Volume(bus);
                _applied[i] = _setParameter?.Invoke(Parameters[i], ToDecibels(_levels[i])) == true;
            }
        }

        public float SourceGain(AudioBus bus, bool routed)
        {
            var channel = (int)bus;
            var master = GainAtSource((int)AudioBus.Master);
            return bus == AudioBus.Master ? master : master * GainAtSource(channel);

            float GainAtSource(int index) =>
                routed && _applied[index] && _levels[index] > 0f ? 1f : _levels[index];
        }

        public static float ToDecibels(float level)
        {
            if (float.IsNaN(level) || level <= 0f)
                return -80f;
            return Mathf.Max(-80f, 20f * Mathf.Log10(Mathf.Min(1f, level)));
        }
    }
}
