using UnityEngine;
using UnityEngine.Audio;

namespace Game.Presentation.Audio
{
    public sealed class MusicPlayer : MonoBehaviour
    {
        private readonly MusicFade<AudioClip> _fade = new MusicFade<AudioClip>();
        private readonly float[] _trims = { 1f, 1f };
        private AudioSource[] _sources;
        private float _volume = 1f;

        public void Configure(AudioMixerGroup group)
        {
            if (_sources != null)
                return;

            _sources = new[] { CreateSource(), CreateSource() };

            AudioSource CreateSource()
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.outputAudioMixerGroup = group;
                return source;
            }
        }

        public void SetVolume(float gain)
        {
            _volume = Mathf.Clamp01(gain);
            UpdateSources();
        }

        public void Play(AudioClip clip, bool loop = true, float fadeSeconds = .65f, float trim = 1f)
        {
            if (_sources == null)
                return;
            if (clip == null)
            {
                Stop(fadeSeconds);
                return;
            }

            var slot = _fade.Play(clip, fadeSeconds, immediateAttack: !loop);
            var source = _sources[slot];
            _trims[slot] = trim;
            if (source.clip != clip)
            {
                source.Stop();
                source.clip = clip;
            }

            source.loop = loop;
            UpdateSources();
            if (!source.isPlaying)
                source.Play();
        }

        public void Stop(float fadeSeconds = .65f)
        {
            _fade.Stop(fadeSeconds);
            UpdateSources();
        }

        private void Update()
        {
            _fade.Advance(Time.unscaledDeltaTime);
            UpdateSources();
        }

        private void UpdateSources()
        {
            if (_sources == null)
                return;

            for (var i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                if (_fade.Track(i) == null && source.clip != null)
                {
                    source.Stop();
                    source.clip = null;
                }

                source.volume = _volume * _trims[i] * _fade.Weight(i);
            }
        }
    }
}
