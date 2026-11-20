using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation.Audio
{
    /// <summary>Crossfade state for the two music sources, driven by unscaled time.</summary>
    public sealed class MusicFade<TTrack> where TTrack : class
    {
        private readonly TTrack[] _tracks = new TTrack[2];
        private readonly float[] _weights = new float[2];
        private readonly float[] _start = new float[2];
        private int _target = -1;
        private float _duration;
        private float _elapsed;

        public TTrack Track(int slot) => _tracks[slot];
        public float Weight(int slot) => _weights[slot];

        public int Play(TTrack track, float seconds, bool immediateAttack = false)
        {
            if (_target >= 0 && HasTrack(_target, track))
                return _target;

            var slot = HasTrack(0, track) ? 0 : HasTrack(1, track) ? 1 : Quietest();
            if (!HasTrack(slot, track))
                _weights[slot] = 0f;
            _tracks[slot] = track;
            if (immediateAttack)
                _weights[slot] = 1f;
            Begin(slot, seconds);
            return slot;
        }

        public void Stop(float seconds)
        {
            if (_target != -1 || seconds <= 0f)
                Begin(-1, seconds);
        }

        public void Advance(float unscaledSeconds)
        {
            _elapsed = Mathf.Min(_duration, _elapsed + unscaledSeconds);
            var fraction = _duration <= 0f ? 1f : _elapsed / _duration;
            for (var i = 0; i < _tracks.Length; i++)
            {
                _weights[i] = Mathf.Lerp(_start[i], i == _target ? 1f : 0f, fraction);
                if (fraction >= 1f && i != _target)
                    _tracks[i] = null;
            }
        }

        private bool HasTrack(int slot, TTrack track) => EqualityComparer<TTrack>.Default.Equals(_tracks[slot], track);

        private int Quietest() => _weights[0] <= _weights[1] ? 0 : 1;

        private void Begin(int slot, float seconds)
        {
            _target = slot;
            _start[0] = _weights[0];
            _start[1] = _weights[1];
            _duration = Mathf.Max(0f, seconds);
            _elapsed = 0f;
            Advance(0f);
        }
    }
}
