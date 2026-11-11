using DG.Tweening;
using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Audio;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>Announces a battle without moving or enlarging its cards.</summary>
    public sealed class BattleStartedPresenter : IEventPresenter
    {
        private readonly BannerView _banner;
        private readonly CardNameLookup _lookup;
        private readonly UiTexts _texts;
        private readonly AnimationSettings _animation;
        private readonly ISoundPlayer _sound;

        public BattleStartedPresenter(
            BannerView banner,
            CardNameLookup lookup,
            UiTexts texts,
            AnimationSettings animation,
            ISoundPlayer sound)
        {
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
                return DOTween.Sequence().AppendInterval(_animation.BattleStartSeconds);
            }));
            context.Enqueue(_banner.Hide);
        }
    }
}
