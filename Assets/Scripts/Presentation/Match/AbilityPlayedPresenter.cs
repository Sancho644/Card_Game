using System.Linq;
using Game.Core.Cards;
using Game.Core.Events;
using Game.Data.Cards;
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
        private readonly CardDatabase _database;
        private readonly AnimationSettings _animation;
        private readonly ISoundPlayer _sound;

        public AbilityPlayedPresenter(TableLayout layout, CardViewRegistry registry, CardDatabase database,
            AnimationSettings animation, ISoundPlayer sound)
        {
            _layout = layout;
            _registry = registry;
            _database = database;
            _animation = animation;
            _sound = sound;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (gameEvent is AbilityCancelled cancelled)
            {
                if (context.PreviousView?.Battle?.PlayedAbilities.Any(
                    ability => ability.Player == cancelled.Owner && ability.Card.InstanceId == cancelled.Ability.InstanceId
                        && !ability.IsCancelled) == true &&
                    !context.Batch.Take(context.Index).OfType<AbilityCancelled>()
                        .Any(earlier => earlier.Ability.InstanceId == cancelled.Ability.InstanceId))
                    context.Enqueue(() => _sound.Play(SoundCue.AbilityCancel));
                return;
            }

            if (!(gameEvent is AbilityPlayed played))
                return;

            var side = played.Player == context.View.Viewer ? TableSide.Bottom : TableSide.Top;
            var isNewPlay = context.PreviousView != null &&
                !context.PreviousView.Discard(played.Player).Any(card => card.InstanceId == played.Ability.InstanceId) &&
                !context.Batch.Take(context.Index).OfType<AbilityPlayed>()
                    .Any(earlier => earlier.Ability.InstanceId == played.Ability.InstanceId);
            context.Enqueue(new TweenStep(() =>
            {
                var card = _registry.GetOrCreate(played.Ability.InstanceId, played.Ability.CardId, _layout.Spawn(side));
                card.SetInHand(false);
                if (isNewPlay && _database.Find(played.Ability.CardId) is AbilityDef ability)
                    _sound.Play(CueFor(ability.Rarity));
                return card.MoveTo(_layout.BattleFocus(_animation.AbilityShowScale), _animation.MoveDuration);
            }));
            context.Enqueue(new DelayStep(_animation.AbilityShowSeconds));
            context.EnqueueSync();
        }

        private static SoundCue CueFor(Rarity rarity) => rarity switch
        {
            Rarity.Epic => SoundCue.AbilityEpic,
            Rarity.Legendary => SoundCue.AbilityLegendary,
            _ => SoundCue.AbilityPlay,
        };
    }
}
