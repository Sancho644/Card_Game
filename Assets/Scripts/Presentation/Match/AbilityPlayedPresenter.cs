using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Audio;
using Game.Presentation.Cards;
using Game.Presentation.Table;

namespace Game.Presentation.Match
{
    /// <summary>A played ability flies to the middle of the table, large, stays there a moment and goes to the discard pile.</summary>
    public sealed class AbilityPlayedPresenter : IEventPresenter
    {
        private readonly TableLayout _layout;
        private readonly CardViewRegistry _registry;
        private readonly AnimationSettings _animation;
        private readonly ISoundPlayer _sound;

        public AbilityPlayedPresenter(TableLayout layout, CardViewRegistry registry, AnimationSettings animation, ISoundPlayer sound)
        {
            _layout = layout;
            _registry = registry;
            _animation = animation;
            _sound = sound;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is AbilityPlayed played))
                return;

            var side = played.Player == context.View.Viewer ? TableSide.Bottom : TableSide.Top;
            context.Enqueue(new TweenStep(() =>
            {
                var card = _registry.GetOrCreate(played.Ability.InstanceId, played.Ability.CardId, _layout.Spawn(side));
                card.SetInHand(false);
                _sound.Play(SoundCue.AbilityPlay);
                return card.MoveTo(_layout.BattleFocus(_animation.AbilityShowScale), _animation.MoveDuration);
            }));
            context.Enqueue(new DelayStep(_animation.AbilityShowSeconds));
            context.Enqueue(() => _sound.Play(SoundCue.AbilityDiscard));
            context.EnqueueSync();
        }
    }
}
