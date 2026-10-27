using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Cards;
using Game.Presentation.Table;

namespace Game.Presentation.Match
{
    /// <summary>A played ability flies to the front of the table and stays there a moment.</summary>
    public sealed class AbilityPlayedPresenter : IEventPresenter
    {
        private readonly TableLayout _layout;
        private readonly CardViewRegistry _registry;
        private readonly TablePresentationSettings _settings;

        public AbilityPlayedPresenter(TableLayout layout, CardViewRegistry registry, TablePresentationSettings settings)
        {
            _layout = layout;
            _registry = registry;
            _settings = settings;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is AbilityPlayed played))
                return;

            var side = played.Player == context.View.Viewer ? TableSide.Bottom : TableSide.Top;
            context.Enqueue(new TweenStep(() =>
            {
                var card = _registry.GetOrCreate(played.Ability.InstanceId, played.Ability.CardId, _layout.Spawn(side));
                return card.MoveTo(_layout.BattleFocus(), _settings.MoveDuration);
            }));
            context.Enqueue(new DelayStep(_settings.AbilityShowSeconds));
        }
    }
}
