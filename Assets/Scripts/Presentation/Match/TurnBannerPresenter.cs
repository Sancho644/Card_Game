using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Audio;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    public sealed class TurnBannerPresenter : IEventPresenter
    {
        private readonly BannerView _banner;
        private readonly BattleDetails _details;
        private readonly MatchMode _mode;
        private readonly UiTexts _texts;
        private readonly AnimationSettings _animation;
        private readonly ISoundPlayer _sound;

        public TurnBannerPresenter(BannerView banner, BattleDetails details, MatchMode mode,
            UiTexts texts, AnimationSettings animation, ISoundPlayer sound)
        {
            _banner = banner; _details = details; _mode = mode;
            _texts = texts; _animation = animation; _sound = sound;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is TurnStarted turn)) return;
            context.Enqueue(_details.Clear);
            if (_mode.ShowsHandoff) return;
            context.Enqueue(() =>
            {
                _sound.Play(SoundCue.TurnStart);
                _banner.Show(_texts.Get(turn.Player == context.View.Viewer ? UiTextKeys.YourTurn : UiTextKeys.OpponentTurn), "");
            });
            context.Enqueue(new DelayStep(_animation.TurnBannerSeconds));
            context.Enqueue(_banner.Hide);
        }
    }
}
