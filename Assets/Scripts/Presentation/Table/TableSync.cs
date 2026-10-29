using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Cards;

namespace Game.Presentation.Table
{
    /// <summary>
    /// Brings every card view to where <see cref="PlayerView"/> says the card is. Event presenters add
    /// emphasis; this keeps the table correct even if an animation was skipped.
    /// </summary>
    public sealed class TableSync
    {
        private readonly TableLayout _layout;
        private readonly CardViewRegistry _registry;
        private readonly AnimationSettings _animation;
        private readonly VisualSettings _visual;
        private readonly UiTexts _texts;

        public TableSync(
            TableLayout layout,
            CardViewRegistry registry,
            AnimationSettings animation,
            VisualSettings visual,
            UiTexts texts)
        {
            _layout = layout;
            _registry = registry;
            _animation = animation;
            _visual = visual;
            _texts = texts;
        }

        /// <param name="view">The match as the viewer sees it; the viewer sits at the bottom.</param>
        /// <param name="revealedOpponent">The opponent's own view, to show their hand face up (debug); null keeps it hidden.</param>
        /// <param name="instant">Place cards without animation, e.g. behind the hand-off screen.</param>
        public Tween Sync(PlayerView view, PlayerView revealedOpponent, bool instant)
        {
            var duration = instant ? 0f : _animation.MoveDuration;
            var sequence = DOTween.Sequence();
            var visible = new HashSet<int>();

            void Move(CardRef card, TableSide owner, CardPose pose, string status = null, float brightness = 1f)
            {
                visible.Add(card.InstanceId);
                var cardView = _registry.GetOrCreate(card.InstanceId, card.CardId, _layout.Spawn(owner));
                cardView.SetStatus(status);
                cardView.SetDimming(brightness);
                var tween = cardView.MoveTo(pose, duration);
                if (tween != null)
                    sequence.Join(tween);
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
                Move(view.Hand[i], TableSide.Bottom, _layout.Hand(TableSide.Bottom, i, view.Hand.Count, true));

            if (revealedOpponent != null)
            {
                _registry.SetAnonymousCount(0, default);
                var hand = revealedOpponent.Hand;
                for (var i = 0; i < hand.Count; i++)
                    Move(hand[i], TableSide.Top, _layout.Hand(TableSide.Top, i, hand.Count, true));
            }
            else
            {
                _registry.SetAnonymousCount(view.OpponentHandCount, _layout.Spawn(TableSide.Top));
                for (var i = 0; i < _registry.Anonymous.Count; i++)
                {
                    var tween = _registry.Anonymous[i].MoveTo(_layout.Hand(TableSide.Top, i, view.OpponentHandCount, false), duration);
                    if (tween != null)
                        sequence.Join(tween);
                }
            }

            _registry.RemoveExcept(visible);
            MarkBattle(view);
            return sequence;
        }

        private void MarkBattle(PlayerView view)
        {
            foreach (var card in _registry.All)
                card.SetBattle(null);
            var field = view.Battle == null ? null : _registry.Find(view.Battle.FieldId);
            if (field != null)
                field.SetBattle(_visual.Battle);
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
