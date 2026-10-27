using Game.Core.Events;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>
    /// The three-step power count from docs/rules.md §6: field values (with conditions and abilities
    /// that change them), then the other abilities in play order, then comparison and consequences.
    /// A cancelled condition acts at no step.
    /// </summary>
    public class PowerCountOrderTests
    {
        [Test]
        public void FieldValueAbility_PlayedLater_AppliesBeforePowerAbilities()
        {
            var d = BattleOnP1Forge(Fire180, Water200,
                player1Deck: Deck(epicAbility: Mirror),
                player2Deck: Deck(commonAbility: Desperation));
            d.Open(P1);
            d.Ready(P1);
            d.Play(P2, Desperation); // Played first, but counted after the mirror: 230 is not below 120.
            d.Play(P1, Mirror);
            d.Ready(P2);
            d.Ready(P1);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(180 - 60, result.HostPower);
            Assert.AreEqual(200 + 30, result.GuestPower);
        }

        [Test]
        public void PowerAbility_SeesAbilitiesPlayedBeforeIt()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(commonAbility: Desperation));
            d.Play(P1, Boost40);
            d.Play(P2, Desperation); // 200 is below 220 here.
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(200 + 60, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void PowerAbility_DoesNotSeeAbilitiesPlayedAfterIt()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(commonAbility: Desperation));
            d.Ready(P1);
            d.Play(P2, Desperation); // 200 is not below 180 yet.
            d.Play(P1, Boost40);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(180 + 40, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void NumberCondition_ChangesFieldValues()
        {
            var deck = Deck(epicField: DoublingLake);
            var d = BattleOnP1Field(DoublingLake, Fire180, Water200, player1Deck: deck);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(180 + 100, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200 - 40, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void ConditionCancelledBeforeOpening_DoesNotApply()
        {
            var deck = Deck(epicAbility: CancelCondition, epicField: DoublingLake);
            var d = BattleOnP1Field(DoublingLake, Fire180, Water200, player1Deck: deck);
            d.Play(P1, CancelCondition);
            d.Ready(P2);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(180 + 50, d.Last<BattleResolved>().HostPower, "Bonuses stay, doubling is gone.");
            Assert.AreEqual(200 - 20, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void ComparisonCondition_AppliesAtTheEnd()
        {
            var deck = Deck(legendaryField: HumbleHall);
            var d = BattleOnP1Field(HumbleHall, Fire180, Water200, player1Deck: deck);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(240, result.HostPower);
            Assert.AreEqual(200, result.GuestPower);
            Assert.AreEqual(P2, result.Winner, "Lower power wins.");
        }

        [Test]
        public void CancelledComparisonCondition_DoesNotApply()
        {
            var deck = Deck(epicAbility: CancelCondition, legendaryField: HumbleHall);
            var d = BattleOnP1Field(HumbleHall, Fire180, Water200, player1Deck: deck);
            d.Open(P1);
            d.Play(P1, CancelCondition);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(P1, d.Last<BattleResolved>().Winner);
        }
    }
}
