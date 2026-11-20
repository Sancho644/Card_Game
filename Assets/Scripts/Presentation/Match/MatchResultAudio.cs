using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Audio;

namespace Game.Presentation.Match
{
    /// <summary>One result signal for the lifetime of a match, even if its result view is refreshed.</summary>
    public sealed class MatchResultAudio
    {
        private bool _played;

        public void PlayOnce(PlayerId? winner, PlayerId viewer, bool hotSeat, ISoundPlayer sound)
        {
            if (_played)
                return;

            _played = true;
            sound.Play(winner == null ? SoundCue.Draw : hotSeat || winner == viewer ? SoundCue.Victory : SoundCue.Defeat);
        }
    }
}
