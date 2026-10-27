using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>A skipped main action gets a banner with the reason, so nobody wonders why the turn passed.</summary>
    public sealed class SkipPresenter : IEventPresenter
    {
        private readonly BannerView _banner;
        private readonly PlayerNames _names;
        private readonly UiTexts _texts;
        private readonly TablePresentationSettings _settings;

        public SkipPresenter(BannerView banner, PlayerNames names, UiTexts texts, TablePresentationSettings settings)
        {
            _banner = banner;
            _names = names;
            _texts = texts;
            _settings = settings;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is MainActionSkipped skipped))
                return;

            var text = _texts.Format(UiTextKeys.SkipBanner, _texts.Get(UiTextKeys.SkipReasonPrefix + skipped.Reason));
            context.Enqueue(() => _banner.Show(_names.Of(skipped.Player), text));
            context.Enqueue(new DelayStep(_settings.BannerSeconds));
            context.Enqueue(_banner.Hide);
        }
    }
}
