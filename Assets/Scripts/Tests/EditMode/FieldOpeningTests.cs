using Game.Core.Commands;
using Game.Core.Events;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    public class FieldOpeningTests
    {
        [Test]
        public void Opening_AppliesBonusesAndPenaltiesByElement()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(Forge, d.Last<FieldOpened>().CardId);
            Assert.AreEqual(180 + 60, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200 - 30, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void Opening_DoesNotPassTheTurn()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Open(P1);

            Assert.IsTrue(d.Field(P1, Forge).IsOpen);
            Assert.AreEqual(P1, d.State.Battle.CurrentActor);
        }

        [Test]
        public void Guest_CannotOpen()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Ready(P1);

            d.Rejected(new OpenField(P2), RejectReason.NotFieldOwner);
        }

        [Test]
        public void Host_CannotOpenAfterPressingReady()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Ready(P1);

            d.Rejected(new OpenField(P1), RejectReason.NotYourTurn);
        }

        [Test]
        public void Field_CannotBeOpenedTwice()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Open(P1);

            d.Rejected(new OpenField(P1), RejectReason.FieldAlreadyOpen);
        }

        [Test]
        public void Field_CannotBeOpenedOutsideBattle()
        {
            var d = Begin(P1);

            d.Rejected(new OpenField(P1), RejectReason.WrongPhase);
        }

        [Test]
        public void Opening_ResetsOpponentReadiness()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Open(P1);

            Assert.IsFalse(d.State.Battle.IsReady(P2));
            Assert.AreEqual(P2, d.Last<ReadinessReset>().Player);

            d.Ready(P1);
            Assert.IsNotNull(d.State.Battle, "Guest must get a chance to respond to the opening.");
            Assert.AreEqual(P2, d.State.Battle.CurrentActor);
        }

        [Test]
        public void AbilityPlayedBeforeOpening_StaysInForce()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(180 + 60 + 50, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200 - 30, d.Last<BattleResolved>().GuestPower);
        }
    }
}
