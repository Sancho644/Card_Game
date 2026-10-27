using Game.Presentation.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    /// <summary>A dragged card lands on the nearest target within reach, even when released beside it.</summary>
    public class DropTargetPickerTests
    {
        private const float Padding = 10f;
        private const float Radius = 50f;

        private static readonly DropCandidate Left = new DropCandidate(1, new Rect(0f, 0f, 100f, 140f));
        private static readonly DropCandidate Right = new DropCandidate(2, new Rect(200f, 0f, 100f, 140f));

        private DropTargetPicker _picker;

        [SetUp]
        public void SetUp() => _picker = new DropTargetPicker();

        [Test]
        public void InsideTarget_PicksIt()
        {
            Assert.AreEqual(2, _picker.Pick(new Vector2(250f, 70f), new[] { Left, Right }, Padding, Radius));
        }

        [Test]
        public void JustOutsideTarget_WithinPadding_PicksIt()
        {
            Assert.AreEqual(1, _picker.Pick(new Vector2(108f, 70f), new[] { Left, Right }, Padding, Radius));
        }

        [Test]
        public void BesideTarget_PicksNearest()
        {
            Assert.AreEqual(1, _picker.Pick(new Vector2(140f, 70f), new[] { Left, Right }, Padding, Radius));
            Assert.AreEqual(2, _picker.Pick(new Vector2(170f, 70f), new[] { Left, Right }, Padding, Radius));
        }

        [Test]
        public void BeyondRadius_PicksNothing()
        {
            Assert.IsNull(_picker.Pick(new Vector2(150f, 300f), new[] { Left, Right }, Padding, Radius));
            Assert.IsNull(_picker.Pick(Vector2.zero, new DropCandidate[0], Padding, Radius));
        }

        [Test]
        public void OverlappingTargets_NearerCentreWins()
        {
            var big = new DropCandidate(3, new Rect(0f, 0f, 300f, 300f));
            var small = new DropCandidate(4, new Rect(200f, 200f, 60f, 60f));

            Assert.AreEqual(4, _picker.Pick(new Vector2(225f, 225f), new[] { big, small }, Padding, Radius));
            Assert.AreEqual(3, _picker.Pick(new Vector2(140f, 140f), new[] { big, small }, Padding, Radius));
        }
    }
}
