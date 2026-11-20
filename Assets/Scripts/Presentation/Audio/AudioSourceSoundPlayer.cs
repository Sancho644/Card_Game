using Game.Data.Presentation;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Presentation.Audio
{
    /// <summary>Plays short effects from sources owned by the application root.</summary>
    public sealed class AudioSourceSoundPlayer : MonoBehaviour, ISoundPlayer
    {
        private const int Voices = 4;

        private SoundSettings _settings;
        private AudioSource[] _sources;
        private AudioSource _ui;
        private MusicPlayer _music;
        private int _next;

        public void Configure(SoundSettings settings, AudioMixerGroup sfx = null, AudioMixerGroup ui = null, MusicPlayer music = null)
        {
            _settings = settings;
            _music = music;
            if (_sources == null)
            {
                _sources = new AudioSource[Voices];
                for (var i = 0; i < Voices; i++)
                    _sources[i] = CreateSource();
                _ui = CreateSource();
            }

            foreach (var source in _sources)
                source.outputAudioMixerGroup = sfx;
            _ui.outputAudioMixerGroup = ui;
        }

        public void SetVolume(float sfx, float ui)
        {
            foreach (var source in _sources)
                source.volume = sfx;
            _ui.volume = ui;
        }

        public void Play(SoundCue cue)
        {
            if (_settings == null || _sources == null)
                return;

            var entry = _settings.Find(cue);
            var trim = entry.Volume * _settings.MasterVolume;
            if (_music != null && (cue == SoundCue.Victory || cue == SoundCue.Defeat || cue == SoundCue.Draw))
            {
                _music.Play(entry.Clip, loop: false, fadeSeconds: .35f, trim: trim);
                return;
            }

            if (entry.Clip == null)
                return;

            if (cue == SoundCue.Button)
            {
                _ui.PlayOneShot(entry.Clip, trim);
                return;
            }

            _sources[_next].PlayOneShot(entry.Clip, trim);
            _next = (_next + 1) % _sources.Length;
        }

        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
