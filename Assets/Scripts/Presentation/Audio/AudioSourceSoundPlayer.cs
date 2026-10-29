using Game.Data.Presentation;
using UnityEngine;

namespace Game.Presentation.Audio
{
    /// <summary>Plays one-shot sounds through a few audio sources on the application root, so they survive scene changes.</summary>
    public sealed class AudioSourceSoundPlayer : MonoBehaviour, ISoundPlayer
    {
        private const int Voices = 4;

        private SoundSettings _settings;
        private AudioSource[] _sources;
        private int _next;

        public void Configure(SoundSettings settings)
        {
            _settings = settings;
            _sources = new AudioSource[Voices];
            for (var i = 0; i < Voices; i++)
            {
                _sources[i] = gameObject.AddComponent<AudioSource>();
                _sources[i].playOnAwake = false;
                _sources[i].spatialBlend = 0f;
            }
        }

        public void Play(SoundCue cue)
        {
            if (_settings == null || _sources == null)
                return;

            var entry = _settings.Find(cue);
            if (entry.Clip == null)
                return;

            var source = _sources[_next];
            _next = (_next + 1) % _sources.Length;
            source.PlayOneShot(entry.Clip, entry.Volume * _settings.MasterVolume);
        }
    }
}
