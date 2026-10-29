using DG.Tweening;
using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Audio;
using Game.Presentation.Cards;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>A battle starts: «Битва!» with the two creatures' names, and the field and both creatures swell.</summary>
    public sealed class BattleStartedPresenter : IEventPresenter
    {
        private readonly CardViewRegistry _registry;
        private readonly BannerView _banner;
        private readonly CardNameLookup _lookup;
        private readonly UiTexts _texts;
        private readonly AnimationSettings _animation;
        private readonly ISoundPlayer _sound;

        public BattleStartedPresenter(
            CardViewRegistry registry,
            BannerView banner,
            CardNameLookup lookup,
            UiTexts texts,
            AnimationSettings animation,
            ISoundPlayer sound)
        {
            _registry = registry;
            _banner = banner;
            _lookup = lookup;
            _texts = texts;
            _animation = animation;
            _sound = sound;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is BattleStarted started))
                return;

            var field = context.View.Field(started.FieldId);
            var host = field?.Occupant(started.Host);
            var guest = field?.Occupant(started.Guest);
            var versus = host == null || guest == null
                ? ""
                : _texts.Format(UiTextKeys.BattleVersus, _lookup.NameOf(host.InstanceId), _lookup.NameOf(guest.InstanceId));

            // The creature that started the battle arrives first.
            context.EnqueueSync();
            context.Enqueue(new TweenStep(() =>
            {
                _sound.Play(SoundCue.BattleStart);
                _banner.Show(_texts.Get(UiTextKeys.BattleStart), versus);
                var sequence = DOTween.Sequence();
                foreach (var id in new[] { started.FieldId, host?.InstanceId ?? -1, guest?.InstanceId ?? -1 })
                {
                    var card = _registry.Find(id);
                    var pulse = card == null ? null : card.Pulse(_animation.BattlePulse, _animation.BattleStartSeconds * 0.6f);
                    if (pulse != null)
                        sequence.Join(pulse);
                }

                sequence.AppendInterval(_animation.BattleStartSeconds * 0.4f);
                return sequence;
            }));
            context.Enqueue(_banner.Hide);
        }
    }
}
