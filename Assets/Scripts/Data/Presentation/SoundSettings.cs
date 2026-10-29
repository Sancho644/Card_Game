using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Data.Presentation
{
    /// <summary>The sound of each game moment. A cue without a clip stays silent.</summary>
    [CreateAssetMenu(menuName = "Game/Config/Sound Settings", fileName = "SoundSettings")]
    public sealed class SoundSettings : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField] private List<SoundEntry> _sounds = AllCues().Select(cue => new SoundEntry(cue)).ToList();

        public float MasterVolume => _masterVolume;

        /// <summary>The entry of the cue; a silent one if the list has none.</summary>
        public SoundEntry Find(SoundCue cue)
        {
            var index = _sounds.FindIndex(entry => entry.Cue == cue);
            return index < 0 ? new SoundEntry(cue) : _sounds[index];
        }

        private static IEnumerable<SoundCue> AllCues() => Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>();

        /// <summary>Adds an entry for every cue missing from the list, e.g. after a new cue was added in code.</summary>
        private void OnValidate()
        {
            foreach (var cue in AllCues())
            {
                if (_sounds.All(entry => entry.Cue != cue))
                    _sounds.Add(new SoundEntry(cue));
            }
        }
    }
}
