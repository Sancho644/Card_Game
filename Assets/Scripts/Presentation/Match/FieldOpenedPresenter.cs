using DG.Tweening;
using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Audio;
using Game.Presentation.Cards;

namespace Game.Presentation.Match
{
    /// <summary>
    /// An opened field card turns over where it lies, so the viewer sees it being revealed. The owner, who
    /// already saw the face of the closed card, sees it light up and swell instead.
    /// </summary>
    public sealed class FieldOpenedPresenter : IEventPresenter
    {
        private readonly CardViewRegistry _registry;
        private readonly AnimationSettings _animation;
        private readonly ISoundPlayer _sound;

        public FieldOpenedPresenter(CardViewRegistry registry, AnimationSettings animation, ISoundPlayer sound)
        {
            _registry = registry;
            _animation = animation;
            _sound = sound;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is FieldOpened opened))
                return;

            context.Enqueue(new TweenStep(() =>
            {
                var card = _registry.Find(opened.FieldId);
                if (card == null)
                    return null;

                _sound.Play(SoundCue.FieldFlip);
                if (card.IsKnown)
                {
                    card.SetDimming(1f);
                    return card.Pulse(_animation.BattlePulse, _animation.FlipSeconds);
                }

                _registry.GetOrCreate(opened.FieldId, opened.CardId, default);
                return card.Flip(_animation.FlipSeconds, _animation.FlipLift);
            }));
        }
    }
}
