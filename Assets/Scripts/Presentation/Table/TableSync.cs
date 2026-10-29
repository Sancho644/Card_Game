using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Audio;
using Game.Presentation.Cards;

namespace Game.Presentation.Table
{
    /// <summary>
    /// Brings every card view to where <see cref="PlayerView"/> says the card is. Event presenters add
    /// emphasis; this keeps the table correct even if an animation was skipped. Cards that appear on the
    /// table can fly in one by one, as when they are dealt.
    /// </summary>
    public sealed class TableSync
    {
        private readonly TableLayout _layout;
        private readonly CardViewRegistry _registry;
        private readonly AnimationSettings _animation;
        private readonly VisualSettings _visual;
        private readonly UiTexts _texts;
        private readonly ISoundPlayer _sound;

        public TableSync(
            TableLayout layout,
            CardViewRegistry registry,
            AnimationSettings animation,
            VisualSettings visual,
            UiTexts texts,
            ISoundPlayer sound)
        {
            _layout = layout;
            _registry = registry;
            _animation = animation;
            _visual = visual;
            _texts = texts;
            _sound = sound;
        }

        /// <param name="view">The match as the viewer sees it; the viewer sits at the bottom.</param>
        /// <param name="revealedOpponent">The opponent's own view, to show their hand face up (debug); null keeps it hidden.</param>
        /// <param name="instant">Place cards without animation, e.g. behind the hand-off screen.</param>
        /// <param name="dealInterval">Cards new on the table start one after another with this pause; 0 moves all at once.</param>
        public Tween Sync(PlayerView view, PlayerView revealedOpponent, bool instant, float dealInterval = 0f)
        {
            var duration = instant ? 0f : _animation.MoveDuration;
            var sequence = DOTween.Sequence();
            var visible = new HashSet<int>();
            var dealt = 0;

            void Add(Tween tween, bool isNew)
            {
                if (tween == null)
                    return;
                if (!isNew || dealInterval <= 0f)
                {
                    sequence.Insert(0f, tween);
                    return;
                }

                var start = dealt++ * dealInterval;
                sequence.Insert(start, tween);
                sequence.InsertCallback(start, () => _sound.Play(SoundCue.CardDeal));
            }

            void Move(CardRef card, TableSide owner, CardPose pose, string status = null, float brightness = 1f, bool inHand = false)
            {
                visible.Add(card.InstanceId);
                var isNew = _registry.Find(card.InstanceId) == null;
                var cardView = _registry.GetOrCreate(card.InstanceId, card.CardId, _layout.Spawn(owner));
                cardView.SetStatus(status);
                cardView.SetDimming(brightness);
                cardView.SetInHand(inHand);
                Add(cardView.MoveTo(pose, duration), isNew);
            }

            foreach (var owner in PlayerIds.All)
            {
                var side = SideOf(owner, view.Viewer);
                var fields = view.Fields.Where(field => field.Owner == owner).ToList();
                var removed = 0;
                for (var i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];
                    var known = field.CardId != null;
                    var pose = field.IsRemoved
                        ? _layout.Pile(side, PileKind.RemovedFields, removed++, known)
                        : _layout.Field(side, i, fields.Count, known);
                    // The owner sees the face of a closed field, darkened; the opponent sees its back.
                    var brightness = known && !field.IsOpen && !field.IsRemoved ? _visual.ClosedFieldBrightness : 1f;
                    Move(new CardRef(field.Id, field.CardId), side, pose, StatusOf(field), brightness);

                    foreach (var occupantOwner in PlayerIds.All)
                    {
                        var creature = field.Occupant(occupantOwner);
                        if (creature != null)
                            Move(creature, SideOf(occupantOwner, view.Viewer), _layout.Creature(pose, SideOf(occupantOwner, view.Viewer)));
                    }
                }

                var discard = view.Discard(owner);
                for (var i = 0; i < discard.Count; i++)
                    Move(discard[i], side, _layout.Pile(side, PileKind.Discard, i, true));

                var outOfGame = view.OutOfGame(owner);
                for (var i = 0; i < outOfGame.Count; i++)
                    Move(outOfGame[i], side, _layout.Pile(side, PileKind.OutOfGame, i, true));
            }

            for (var i = 0; i < view.Hand.Count; i++)
                Move(view.Hand[i], TableSide.Bottom, _layout.Hand(TableSide.Bottom, i, view.Hand.Count, true), inHand: true);

            if (revealedOpponent != null)
            {
                _registry.SetAnonymousCount(0, default);
                var hand = revealedOpponent.Hand;
                for (var i = 0; i < hand.Count; i++)
                    Move(hand[i], TableSide.Top, _layout.Hand(TableSide.Top, i, hand.Count, true));
            }
            else
            {
                var known = _registry.Anonymous.Count;
                _registry.SetAnonymousCount(view.OpponentHandCount, _layout.Spawn(TableSide.Top));
                for (var i = 0; i < _registry.Anonymous.Count; i++)
                    Add(_registry.Anonymous[i].MoveTo(_layout.Hand(TableSide.Top, i, view.OpponentHandCount, false), duration), i >= known);
            }

            _registry.RemoveExcept(visible);
            MarkBattle(view);
            return sequence;
        }

        /// <summary>Lights the field of the current battle and shows whether its condition acts.</summary>
        private void MarkBattle(PlayerView view)
        {
            foreach (var card in _registry.All)
            {
                card.SetBattle(null);
                card.SetCondition(ConditionMark.None);
            }

            var battle = view.Battle;
            var field = battle == null ? null : _registry.Find(battle.FieldId);
            if (field == null)
                return;

            field.SetBattle(_visual.Battle);
            if (view.Field(battle.FieldId).IsOpen)
                field.SetCondition(battle.IsConditionCancelled ? ConditionMark.Cancelled : ConditionMark.Active);
        }

        private string StatusOf(FieldView field)
        {
            if (field.IsRemoved)
                return null;
            if (field.IsLocked)
                return _texts.Get(UiTextKeys.FieldLocked);
            return field.IsOpen ? null : _texts.Get(UiTextKeys.FieldClosed);
        }

        private static TableSide SideOf(PlayerId player, PlayerId viewer) =>
            player == viewer ? TableSide.Bottom : TableSide.Top;
    }
}
