using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Audio;

namespace Game.Presentation.Match
{
    /// <summary>Sounds of cards put on the table that have no animation of their own.</summary>
    public sealed class CardSoundPresenter : IEventPresenter
    {
        private readonly ISoundPlayer _sound;

        public CardSoundPresenter(ISoundPlayer sound)
        {
            _sound = sound;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (gameEvent is CreaturePlaced || gameEvent is CreatureMoved)
                context.Enqueue(() => _sound.Play(SoundCue.CardPlace));
        }
    }
}
