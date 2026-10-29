using System.Collections.Generic;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using UnityEngine;

namespace Game.Presentation.Table
{
    /// <summary>
    /// Card places on the table, in world space. Fields of both players in the centre, the viewer's row
    /// nearer to the camera; creatures stand at the edge of the field facing their owner, so the field's
    /// numbers and condition stay visible. The viewer's hand faces the camera and squeezes to fit the safe
    /// area; the opponent's hand of backs lies at the far edge, partly off screen.
    /// </summary>
    public sealed class TableLayout : MonoBehaviour
    {
        [Header("Cards")]
        [SerializeField] private Vector2 _cardSize = new Vector2(1.4f, 2f);

        [Header("Fields")]
        [SerializeField] private float _fieldSpacing = 2.2f;
        [SerializeField] private float _fieldRowDistance = 2.6f;

        [Header("Hands")]
        [SerializeField] private float _handDistance = 5.8f;
        [SerializeField] private float _opponentHandDistance = 6.4f;
        [SerializeField] private float _handLift = 1.2f;
        [SerializeField] private float _minHandSpacing = 0.35f;
        [Tooltip("Part of a resting hand card kept on screen, from its top edge; the rest may hang below. A card being looked at rises in full.")]
        [SerializeField, Range(0.3f, 1f)] private float _handVisibleShare = 0.65f;

        [Header("Piles")]
        [SerializeField] private float _discardX = 5.6f;
        [SerializeField] private float _outOfGameX = 7.4f;
        [SerializeField] private float _removedFieldsX = -5.8f;
        [SerializeField] private float _pileDistance = 2.4f;
        [SerializeField] private float _stackStep = 0.03f;

        [Header("Other")]
        [SerializeField] private Vector3 _battleFocus = new Vector3(0f, 2.2f, -1.2f);
        [SerializeField] private float _layerLift = 0.02f;

        /// <summary>The fan bends as for this many gaps between cards at most.</summary>
        private const float MaxFanSteps = 5f;
        private const float FanDroop = 0.05f;

        private static readonly Quaternion FaceUp = Quaternion.Euler(90f, 0f, 0f);
        private static readonly Quaternion FaceDown = Quaternion.Euler(-90f, 0f, 0f);

        private TablePresentationSettings _settings;
        private Camera _camera;

        /// <summary>Size of a card at scale 1, in world units.</summary>
        public Vector2 CardSize => _cardSize;

        public void Configure(TablePresentationSettings settings, Camera camera)
        {
            _settings = settings;
            _camera = camera;
        }

        public CardPose Field(TableSide side, int index, int count, bool faceUp) =>
            new CardPose(
                new Vector3(RowX(index, count, _fieldSpacing), 0f, Sign(side) * _fieldRowDistance),
                Face(faceUp),
                _settings.FieldScale);

        /// <summary>A creature on a field: at the field's edge that faces its owner, covering only a strip of it.</summary>
        public CardPose Creature(CardPose field, TableSide owner)
        {
            var scale = _settings.CreatureOnFieldScale;
            var offset = _cardSize.y * (field.Scale + scale) / 2f - _settings.CreatureOverlap;
            return new CardPose(field.Position + new Vector3(0f, _layerLift * 2f, Sign(owner) * offset), FaceUp, scale);
        }

        public CardPose Hand(TableSide side, int index, int count, bool faceUp)
        {
            var x = RowX(index, count, HandSpacing(side, count));
            var centre = (count - 1) / 2f;
            var lift = _handLift + index * _layerLift;
            if (side == TableSide.Top)
                return new CardPose(new Vector3(x, index * _layerLift, _opponentHandDistance), Face(faceUp));
            var position = new Vector3(x, lift, -_handDistance);

            // The whole fan keeps the same arc however many cards there are, so the outer ones never sink off screen.
            var bend = count > 1 ? Mathf.Min(1f, MaxFanSteps / (count - 1)) : 1f;
            var tilt = FacingCamera;
            var fan = Quaternion.AngleAxis((centre - index) * _settings.HandFanDegrees * bend, tilt * Vector3.back);
            return new CardPose(position + Vector3.down * Mathf.Abs(index - centre) * FanDroop * bend, fan * tilt);
        }

        public CardPose Pile(TableSide side, PileKind pile, int index, bool faceUp)
        {
            var x = pile == PileKind.Discard ? _discardX : pile == PileKind.OutOfGame ? _outOfGameX : _removedFieldsX;
            return new CardPose(new Vector3(x, index * _stackStep, Sign(side) * _pileDistance), Face(faceUp));
        }

        /// <summary>Where a card appears from when it shows up on this side without a view yet.</summary>
        public CardPose Spawn(TableSide side) =>
            new CardPose(new Vector3(0f, _handLift, side == TableSide.Bottom ? -_handDistance - 2.5f : _opponentHandDistance + 2.5f), FaceDown);

        /// <summary>Above the table in front of the camera: where a played ability is shown.</summary>
        public CardPose BattleFocus() =>
            new CardPose(_battleFocus, FacingCamera);

        /// <summary>
        /// Points the camera must keep on screen: the visible part of the viewer's hand, the far edge of the
        /// opponent's creatures and the piles. The hands' width is not included, because the hands squeeze to
        /// the screen instead; the opponent's hand may hang off the top.
        /// </summary>
        public IEnumerable<Vector3> FramingPoints()
        {
            var halfHeight = _cardSize.y / 2f;
            var halfWidth = _cardSize.x / 2f;
            var nearEdge = FacingCamera * new Vector3(0f, halfHeight - 2f * halfHeight * _handVisibleShare, 0f);
            yield return new Vector3(0f, _handLift, -_handDistance) + nearEdge;
            var farField = Field(TableSide.Top, 0, 1, true);
            var farCreature = Creature(farField, TableSide.Top);
            yield return farCreature.Position + new Vector3(0f, 0f, halfHeight * farCreature.Scale);
            foreach (var x in new[] { _outOfGameX + halfWidth, _removedFieldsX - halfWidth })
            {
                yield return new Vector3(x, 0f, -_pileDistance - halfHeight);
                yield return new Vector3(x, 0f, _pileDistance + halfHeight);
            }
        }

        /// <summary>The widest step that keeps the whole hand inside the safe area, never above the configured one.</summary>
        private float HandSpacing(TableSide side, int count)
        {
            var spacing = _settings.HandSpacing;
            if (count < 2 || _camera == null)
                return spacing;

            var centre = side == TableSide.Bottom ? new Vector3(0f, _handLift, -_handDistance) : new Vector3(0f, 0f, _opponentHandDistance);
            var room = VisibleHalfWidth(centre) - _cardSize.x / 2f;
            return Mathf.Clamp(2f * room / (count - 1), _minHandSpacing, spacing);
        }

        /// <summary>Half of the table width visible at the point's depth, inside the safe area and margins.</summary>
        private float VisibleHalfWidth(Vector3 point)
        {
            var safe = Screen.safeArea;
            var left = safe.xMin / Screen.width + _settings.HandScreenMargin;
            var right = safe.xMax / Screen.width - _settings.HandScreenMargin;
            var viewport = _camera.WorldToViewportPoint(point);
            var leftEdge = _camera.ViewportToWorldPoint(new Vector3(left, viewport.y, viewport.z));
            var rightEdge = _camera.ViewportToWorldPoint(new Vector3(right, viewport.y, viewport.z));
            return Mathf.Min(point.x - leftEdge.x, rightEdge.x - point.x);
        }

        /// <summary>A card turned to face the camera, so it reads without perspective.</summary>
        private Quaternion FacingCamera => Quaternion.Euler(_settings.CameraPitch, 0f, 0f);

        private static Quaternion Face(bool faceUp) => faceUp ? FaceUp : FaceDown;

        private static float Sign(TableSide side) => side == TableSide.Bottom ? -1f : 1f;

        private static float RowX(int index, int count, float spacing) => (index - (count - 1) / 2f) * spacing;
    }
}
