using System.Linq;
using Game.Core.Commands;
using Game.Core.State;
using Game.Core.Events;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Basic "+N power" effects and their requirements.</summary>
    public class AbilityEffectTests
    {
        [Test]
        public void HostBoost_AppliesOnlyToHost()
        {
            var deck = Deck(commonAbility: HostBoost);
            var d = BattleOnP1Forge(Fire180, Fire180, player1Deck: deck, player2Deck: deck);
            d.Play(P1, HostBoost);
            AssertForbidden(d, P2, HostBoost);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(230, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(180, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void GuestBoost_AppliesOnlyToGuest()
        {
            var deck = Deck(commonAbility: GuestBoost);
            var d = BattleOnP1Forge(Fire180, Fire180, player1Deck: deck, player2Deck: deck);
            AssertForbidden(d, P1, GuestBoost);
            d.Ready(P1);
            d.Play(P2, GuestBoost);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(180, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(230, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void ClosedFieldBoost_AppliesWhileFieldStaysClosed()
        {
            var deck = Deck(commonAbility: ClosedFieldBoost);
            var d = BattleOnP1Forge(Fire180, Water200, player1Deck: deck);
            d.Play(P1, ClosedFieldBoost);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(230, d.Last<BattleResolved>().HostPower);
        }

        [Test]
        public void ClosedFieldBoost_LostIfFieldIsOpenedLater()
        {
            var deck = Deck(commonAbility: ClosedFieldBoost);
            var d = BattleOnP1Forge(Fire180, Water200, player1Deck: deck);
            d.Play(P1, ClosedFieldBoost);
            d.Ready(P2);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(180 + 60, d.Last<BattleResolved>().HostPower);
        }

        [Test]
        public void ElementBoost_AppliesOnlyToMatchingElement()
        {
            var deck = Deck(commonAbility: FireBoost);
            var d = BattleOnP1Forge(Fire180, Water200, player1Deck: deck, player2Deck: deck);
            d.Play(P1, FireBoost);
            AssertForbidden(d, P2, FireBoost);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(240, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200, d.Last<BattleResolved>().GuestPower);
        }
        private static void AssertForbidden(MatchDriver d, PlayerId player, string ability)
        {
            var id = d.Id(player, ability);
            var actor = d.State.Battle.CurrentActor;
            var events = d.Log.Count;
            Assert.IsFalse(d.Engine.GetLegalActions(player).OfType<PlayAbility>().Any(c => c.AbilityId == id));
            d.Rejected(new PlayAbility(player, id), RejectReason.AbilityRequirementNotMet);
            Assert.IsTrue(d.InHand(player, ability));
            Assert.IsFalse(d.InDiscard(player, ability));
            Assert.AreEqual(actor, d.State.Battle.CurrentActor);
            Assert.AreEqual(events, d.Log.Count);
        }

        [Test]
        public void HumbleHall_SurvivalReturnsHigherPowerLoser()
        {
            var d = BattleOnP1Field(HumbleHall, Fire180, Earth250,
                player1Deck: Deck(legendaryField: HumbleHall), player2Deck: Deck(epicAbility: Survival));
            d.Open(P1); d.Ready(P1); d.Play(P2, Survival); d.Ready(P1); d.Ready(P2);
            var result = d.Last<BattleResolved>();
            Assert.Less(result.HostPower, result.GuestPower);
            Assert.AreEqual(P1, result.Winner);
            Assert.IsTrue(d.IsOut(P1, Fire180));
            Assert.IsTrue(d.InHand(P2, Earth250));
            Assert.IsFalse(d.IsOut(P2, Earth250));
        }
    }
}
