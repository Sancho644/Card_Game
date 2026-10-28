using Game.Core.Battle;
using Game.Core.Commands;
using Game.Core.Events;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Epic abilities and the remaining common requirement (docs/cards.md §5).</summary>
    public class EpicAbilityTests
    {
        [Test]
        public void FieldBlessing_AppliesWhenFieldGivesBonus()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player1Deck: Deck(commonAbility: FieldBlessing));
            d.Open(P1);
            d.Play(P1, FieldBlessing);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(180 + 60 + 50, d.Last<BattleResolved>().HostPower);
        }

        [Test]
        public void FieldBlessing_NoBonus_NoEffect()
        {
            var d = BattleOnP1Forge(Water200, Fire180, player1Deck: Deck(commonAbility: FieldBlessing));
            d.Open(P1);
            d.Play(P1, FieldBlessing);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(200 - 30, d.Last<BattleResolved>().HostPower);
        }

        [Test]
        public void ForcedReveal_OpensField()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(epicAbility: ForcedReveal));
            d.Ready(P1);
            d.Play(P2, ForcedReveal);

            Assert.IsTrue(d.Field(P1, Forge).IsOpen);
            Assert.AreEqual(Forge, d.Last<FieldOpened>().CardId);

            d.Ready(P1);
            d.Ready(P2);
            Assert.AreEqual(180 + 60, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200 - 30 + 60, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void Intercept_CancelsLastOpponentAbility()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(epicAbility: Intercept));
            d.Play(P1, Boost50);
            d.Play(P2, Intercept);

            Assert.AreEqual(Boost50, d.Last<AbilityCancelled>().Ability.CardId);

            d.Ready(P1);
            d.Ready(P2);
            Assert.AreEqual(180, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(200 + 40, d.Last<BattleResolved>().GuestPower);
            Assert.IsTrue(d.InDiscard(P1, Boost50));
        }

        [Test]
        public void Survival_LosingCreatureReturnsToHand()
        {
            var d = BattleOnP1Forge(Fire180, Earth250, player1Deck: Deck(epicAbility: Survival));
            d.Play(P1, Survival);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(P2, d.Last<BattleResolved>().Winner);
            Assert.IsTrue(d.InHand(P1, Fire180));
            Assert.AreEqual(1, d.Wins(P2));
        }

        [Test]
        public void LastWord_OpponentCannotPlayCommonOrEpic()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player1Deck: Deck(epicAbility: LastWord));
            d.Play(P1, LastWord);

            d.Rejected(new PlayAbility(P2, d.Id(P2, Boost50)), RejectReason.BlockedByAbility);
            d.Play(P2, Gambit);
            d.Play(P1, Boost50);
        }

        [Test]
        public void LastWord_AlsoBlocksEpicCounter()
        {
            var d = BattleOnP1Forge(Fire180, Water200,
                player1Deck: Deck(epicAbility: LastWord),
                player2Deck: Deck(epicAbility: Intercept));
            d.Ready(P1);
            d.Play(P2, Boost50);
            d.Play(P1, LastWord);

            d.Rejected(new PlayAbility(P2, d.Id(P2, Intercept)), RejectReason.BlockedByAbility);
        }

        [Test]
        public void TieBreaker_WinsOnEqualPower_OverPlacementOrder()
        {
            var d = BattleOnP1Forge(Fire180, Fire180, player2Deck: Deck(epicAbility: TieBreaker));
            d.Play(P1, Boost50);
            d.Play(P2, TieBreaker);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(230, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(230, d.Last<BattleResolved>().GuestPower);
            Assert.AreEqual(P2, d.Last<BattleResolved>().Winner, "Player2 was placed later but holds «Перевес».");
            Assert.AreEqual(TieBreak.Ability, d.Last<BattleResolved>().TieBreak);
        }

        [Test]
        public void Blockade_OpponentLosesFieldBonus()
        {
            var d = BattleOnP1Forge(Water200, Fire180, player1Deck: Deck(epicAbility: Blockade));
            d.Open(P1);
            d.Play(P1, Blockade);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(200 - 30 + 60, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(180, d.Last<BattleResolved>().GuestPower);
        }
    }
}
