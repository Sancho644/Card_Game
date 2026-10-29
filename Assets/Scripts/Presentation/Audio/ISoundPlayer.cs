using Game.Data.Presentation;

namespace Game.Presentation.Audio
{
    /// <summary>Plays the sound of a game moment; a cue without a clip is silent.</summary>
    public interface ISoundPlayer
    {
        void Play(SoundCue cue);
    }
}
