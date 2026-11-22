using System;
using System.Linq;
using DG.Tweening;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Cards;
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
        private readonly CardDatabase _database;
        private readonly AnimationSettings _animation;
        private readonly ISoundPlayer _sound;

        public FieldOpenedPresenter(CardViewRegistry registry, CardDatabase database, AnimationSettings animation, ISoundPlayer sound)
        {
            _registry = registry;
            _database = database;
            _animation = animation;
            _sound = sound;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is FieldOpened opened))
                return;

            var field = context.View.Field(opened.FieldId);
            if (field?.IsOpen != true || field.CardId != opened.CardId ||
                context.PreviousView?.Field(opened.FieldId)?.IsOpen != false ||
                context.Batch.Take(context.Index).OfType<FieldOpened>().Any(earlier => earlier.FieldId == opened.FieldId))
                return;

            var element = RevealCue(field, context.Power);
            context.Enqueue(new TweenStep(() =>
            {
                var card = _registry.Find(opened.FieldId);
                if (card == null)
                    return null;

                card.SetFieldState(false,false);
                _sound.Play(SoundCue.FieldFlip);
                Tween reveal;
                if (card.IsKnown)
                {
                    card.SetDimming(1f);
                    reveal = card.Pulse(_animation.BattlePulse, _animation.FlipSeconds);
                }
                else
                {
                    _registry.GetOrCreate(opened.FieldId, opened.CardId, default);
                    reveal = card.Flip(_animation.FlipSeconds, _animation.FlipLift);
                }

                if (!element.HasValue || reveal == null)
                    return reveal;

                return DOTween.Sequence().Append(reveal).AppendCallback(() => _sound.Play(element.Value));
            }));
        }

        private SoundCue? RevealCue(FieldView field, BattlePower power)
        {
            if (power?.FieldId != field.Id)
                return null;

            var host = Math.Abs(power.HostPower.FieldValue);
            var guest = Math.Abs(power.GuestPower.FieldValue);
            if (host == 0 && guest == 0)
                return null;

            // Use one accent for the strongest public modifier, including penalties.
            var occupant = field.Occupant(host >= guest ? power.Host : power.Guest);
            return (_database.Find(occupant?.CardId) as CreatureDef)?.Element switch
            {
                Element.Fire => SoundCue.ElementFire,
                Element.Water => SoundCue.ElementWater,
                Element.Earth => SoundCue.ElementEarth,
                Element.Wind => SoundCue.ElementWind,
                Element.Light => SoundCue.ElementLight,
                Element.Dark => SoundCue.ElementDark,
                _ => null,
            };
        }
    }
}
