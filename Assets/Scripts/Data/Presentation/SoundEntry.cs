using System;
using UnityEngine;

namespace Game.Data.Presentation
{
    [Serializable]
    public struct SoundEntry
    {
        public SoundCue Cue;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume;

        public SoundEntry(SoundCue cue)
        {
            Cue = cue;
            Clip = null;
            Volume = 1f;
        }
    }
}
