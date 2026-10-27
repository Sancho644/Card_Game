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
            d.Play(P2, HostBoost);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(230, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(180, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void GuestBoost_AppliesOnlyToGuest()
        {
            var deck = Deck(commonAbility: GuestBoost);
            var d = BattleOnP1Forge(Fire180, Fire180, player1Deck: deck, player2Deck: deck);
            d.Play(P1, GuestBoost);
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
            d.Play(P2, FireBoost);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(240, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200, d.Last<BattleResolved>().GuestPower);
        }
    }
}
