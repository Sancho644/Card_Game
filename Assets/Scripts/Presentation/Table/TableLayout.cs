using Game.Presentation.Cards;
using UnityEngine;

namespace Game.Presentation.Table
{
    /// <summary>
    /// Card places on the table, in world space. Fields of both players in the centre, the viewer's row
    /// nearer to the camera; creatures stand on the side of the field facing their owner.
    /// </summary>
    public sealed class TableLayout : MonoBehaviour
    {
        [Header("Fields")]
        [SerializeField] private float _fieldSpacing = 2.0f;
        [SerializeField] private float _fieldRowDistance = 2.4f;
        [SerializeField] private float _creatureOffset = 1.1f;

        [Header("Hands")]
        [SerializeField] private float _handDistance = 5.4f;
        [SerializeField] private float _handLift = 1.2f;
        [SerializeField] private float _handSpacing = 1.15f;
        [SerializeField] private float _handFanDegrees = 5f;
        [SerializeField] private float _handTiltDegrees = 40f;

        [Header("Piles")]
        [SerializeField] private float _discardX = 5.6f;
        [SerializeField] private float _outOfGameX = 7.4f;
        [SerializeField] private float _removedFieldsX = -5.8f;
        [SerializeField] private float _pileDistance = 2.4f;
        [SerializeField] private float _stackStep = 0.03f;

        [Header("Other")]
        [SerializeField] private Vector3 _battleFocus = new Vector3(0f, 2.2f, -1.2f);
        [SerializeField] private float _layerLift = 0.02f;

        private static readonly Quaternion FaceUp = Quaternion.Euler(90f, 0f, 0f);
        private static readonly Quaternion FaceDown = Quaternion.Euler(-90f, 0f, 0f);

        public void Configure(float handSpacing, float handFanDegrees, float handTiltDegrees)
        {
            _handSpacing = handSpacing;
            _handFanDegrees = handFanDegrees;
            _handTiltDegrees = handTiltDegrees;
        }

        public CardPose Field(TableSide side, int index, int count, bool faceUp) =>
            new CardPose(new Vector3(RowX(index, count, _fieldSpacing), 0f, Sign(side) * _fieldRowDistance), Face(faceUp));

        /// <summary>A creature on a field; it stands on the field's side that faces its owner.</summary>
        public CardPose Creature(CardPose field, TableSide owner) =>
            new CardPose(field.Position + new Vector3(0f, _layerLift * 2f, Sign(owner) * _creatureOffset), FaceUp);

        public CardPose Hand(TableSide side, int index, int count, bool faceUp)
        {
            var x = RowX(index, count, _handSpacing);
            var centre = (count - 1) / 2f;
            var lift = _handLift + index * _layerLift;
            var position = new Vector3(x, lift, Sign(side) * _handDistance);
            if (side == TableSide.Top)
                return new CardPose(position, Face(faceUp));

            var tilt = Quaternion.Euler(90f - _handTiltDegrees, 0f, 0f);
            var fan = Quaternion.AngleAxis((centre - index) * _handFanDegrees, tilt * Vector3.back);
            return new CardPose(position + Vector3.down * Mathf.Abs(index - centre) * 0.05f, fan * tilt);
        }

        public CardPose Pile(TableSide side, PileKind pile, int index, bool faceUp)
        {
            var x = pile == PileKind.Discard ? _discardX : pile == PileKind.OutOfGame ? _outOfGameX : _removedFieldsX;
            return new CardPose(new Vector3(x, index * _stackStep, Sign(side) * _pileDistance), Face(faceUp));
        }

        /// <summary>Where a card appears from when it shows up on this side without a view yet.</summary>
        public CardPose Spawn(TableSide side) =>
            new CardPose(new Vector3(0f, _handLift, Sign(side) * (_handDistance + 2.5f)), FaceDown);

        /// <summary>Above the table in front of the camera: where a played ability is shown.</summary>
        public CardPose BattleFocus() => new CardPose(_battleFocus, Quaternion.Euler(90f - _handTiltDegrees, 0f, 0f));

        private static Quaternion Face(bool faceUp) => faceUp ? FaceUp : FaceDown;

        private static float Sign(TableSide side) => side == TableSide.Bottom ? -1f : 1f;

        private static float RowX(int index, int count, float spacing) => (index - (count - 1) / 2f) * spacing;
    }
}
