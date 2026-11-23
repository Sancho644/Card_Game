using System.Collections.Generic;
using Game.Data.Presentation;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Presentation.Audio
{
    /// <summary>Plays short effects from sources owned by the application root.</summary>
    public sealed class AudioSourceSoundPlayer : MonoBehaviour, ISoundPlayer
    {
        private const int Voices = 4;
        private const double MinimumDealSpacing = .045;
        private const double MinimumDeniedSpacing = .20;
        private readonly Dictionary<SoundCue, int> _nextVariant = new Dictionary<SoundCue, int>();
        private double _lastDealAt = double.NegativeInfinity;
        private double _lastDeniedAt = double.NegativeInfinity;

        private SoundSettings _settings;
        private AudioSource[] _sources;
        private AudioSource _ui;
        private MusicPlayer _music;
        private int _next;

        public void Configure(SoundSettings settings, AudioMixerGroup sfx = null, AudioMixerGroup ui = null, MusicPlayer music = null)
        {
            if (_settings != settings)
            {
                _nextVariant.Clear();
                _lastDealAt = double.NegativeInfinity;
                _lastDeniedAt = double.NegativeInfinity;
            }
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

            if (IsUiCue(cue))
            {
                if (_ui.volume <= 0f)
                    return;
                if (cue == SoundCue.UiDenied)
                {
                    var now = Time.unscaledTimeAsDouble;
                    if (now - _lastDeniedAt < MinimumDeniedSpacing)
                        return;
                    _lastDeniedAt = now;
                }
                _ui.PlayOneShot(entry.Clip, trim);
                return;
            }

            if (cue == SoundCue.CardDeal)
            {
                var now = Time.unscaledTimeAsDouble;
                if (now - _lastDealAt < MinimumDealSpacing)
                    return;
                _lastDealAt = now;
            }

            _sources[_next].PlayOneShot(NextClip(entry), trim);
            _next = (_next + 1) % _sources.Length;
        }

        internal static bool IsUiCue(SoundCue cue) =>
            cue == SoundCue.Button || cue >= SoundCue.UiConfirm && cue <= SoundCue.UiSettingsPreview;

        private AudioClip NextClip(SoundEntry entry)
        {
            if (entry.Variants == null || entry.Variants.Length == 0)
                return entry.Clip;

            _nextVariant.TryGetValue(entry.Cue, out var index);
            _nextVariant[entry.Cue] = (index + 1) % (entry.Variants.Length + 1);
            return index == 0 ? entry.Clip : entry.Variants[index - 1] ?? entry.Clip;
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
