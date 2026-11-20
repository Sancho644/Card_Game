using System.Collections.Generic;
using Game.Data.Presentation;
using Game.Presentation.Audio;

namespace Game.Presentation.Tests
{
    internal sealed class RecordingSoundPlayer : ISoundPlayer
    {
        public List<SoundCue> Calls { get; } = new List<SoundCue>();

        public void Play(SoundCue cue) => Calls.Add(cue);
    }
}
