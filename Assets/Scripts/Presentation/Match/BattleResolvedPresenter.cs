using System.Linq;
using DG.Tweening;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Audio;
using Game.Presentation.Cards;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>Shows the outcome before the shared table sync moves cards to their final piles.</summary>
    public sealed class BattleResolvedPresenter : IEventPresenter
    {
        private readonly BattleSummaryBuilder _summary;
        private readonly BattleDetails _details;
        private readonly BannerView _banner;
        private readonly CardViewRegistry _cards;
        private readonly AnimationSettings _animation;
        private readonly VisualSettings _visual;
        private readonly ISoundPlayer _sound;
        private readonly UiTexts _texts;

        public BattleResolvedPresenter(BattleSummaryBuilder summary, BattleDetails details, BannerView banner,
            CardViewRegistry cards, AnimationSettings animation, VisualSettings visual, ISoundPlayer sound, UiTexts texts)
        {
            _summary = summary; _details = details; _banner = banner; _cards = cards;
            _animation = animation; _visual = visual; _sound = sound; _texts = texts;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is BattleResolved) && !(gameEvent is BattleRetreated)) return;
            var battle = context.PreviousView?.Battle;
            if (battle == null) return;
            var field = context.PreviousView.Field(battle.FieldId);
            var consequences = context.Batch.Skip(context.Index + 1)
                .TakeWhile(e => !(e is TurnStarted) && !(e is BattleStarted)).ToList();
            var result = gameEvent as BattleResolved;
            var shortText = result == null ? _texts.Get(UiTextKeys.BattleRetreat)
                : _summary.BuildShort(result, battle.Host, consequences);
            var details = result == null ? shortText : _summary.BuildDetails(result, battle.Host,
                field.Occupant(battle.Host).InstanceId, field.Occupant(battle.Guest).InstanceId, consequences);
            // Only already public sources are named; never reveal a closed opponent field.
            var sources = battle.PlayedAbilities.Where(a => !a.IsCancelled).Select(a => a.Card.CardId).ToList();
            if (field.IsOpen && !battle.IsConditionCancelled) sources.Insert(0, field.CardId);
            var sourceText = _summary.Sources(sources);
            if (!string.IsNullOrEmpty(sourceText)) details += "\n" + sourceText;
            context.Enqueue(() =>
            {
                if (result == null) _details.Remember(details);
                else _details.RememberCards(details, field.Occupant(battle.Host).CardId,
                    field.Occupant(battle.Guest).CardId, _summary.PlayerName(battle.Host), _summary.PlayerName(battle.Guest), result.Host, result.Guest);
                _banner.Show(_summary.Title, shortText);
            });
            context.Enqueue(new TweenStep(() =>
            {
                var sequence = DOTween.Sequence();
                if (result?.Winner != null)
                {
                    var winner = _cards.Find(field.Occupant(result.Winner.Value).InstanceId);
                    if (winner != null)
                    {
                        winner.SetBattle(_visual.PowerUp);
                        sequence.Join(winner.Pulse(_animation.BattlePulse, _animation.OutcomeSeconds));
                        sequence.AppendCallback(() => { if (winner != null) winner.SetBattle(null); });
                    }
                    _sound.Play(SoundCue.BattleWin);
                }
                foreach (var removed in consequences.OfType<CreatureOut>())
                {
                    var card = _cards.Find(removed.CreatureId);
                    if (card != null)
                        sequence.Insert(0, DOVirtual.Float(1f, _animation.OutBrightness,
                            _animation.OutcomeSeconds, value => { if (card != null) card.SetDimming(value); }));
                    _sound.Play(SoundCue.CreatureOut);
                }
                if (consequences.OfType<FieldRemoved>().Any()) _sound.Play(SoundCue.FieldRemove);
                sequence.AppendInterval(_animation.OutcomeBannerSeconds);
                return sequence;
            }));
            context.Enqueue(_banner.Hide);
            context.EnqueueSync();
        }
    }
}
