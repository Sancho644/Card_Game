using System.Linq;
using Game.Core.Battle;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Field conditions from the catalog (docs/cards.md §3). Conditions act only on an open field.</summary>
    public class FieldConditionTests
    {
        [Test]
        public void Silence_BlocksCommonAndEpic_ButNotLegendary()
        {
            var d = BattleOnP1Field(SilenceField, Fire180, Water200, player1Deck: Deck(epicField: SilenceField));
            d.Open(P1);

            d.Rejected(new PlayAbility(P1, d.Id(P1, Boost50)), RejectReason.BlockedByFieldCondition);
            d.Rejected(new PlayAbility(P1, d.Id(P1, Boost40)), RejectReason.BlockedByFieldCondition);
            d.Play(P1, Gambit);
        }

        [Test]
        public void Silence_AbilityPlayedBeforeOpening_StaysInForce()
        {
            var d = BattleOnP1Field(SilenceField, Fire180, Water200, player1Deck: Deck(epicField: SilenceField));
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(180 + 60 + 50, d.Last<BattleResolved>().HostPower);
        }

        [Test]
        public void HostRight_OnlyHostPlaysCommonAndEpic()
        {
            var d = BattleOnP1Field(HostRightField, Fire180, Water200, player1Deck: Deck(epicField: HostRightField));
            d.Open(P1);
            d.Play(P1, Boost50);

            d.Rejected(new PlayAbility(P2, d.Id(P2, Boost50)), RejectReason.BlockedByFieldCondition);
            d.Play(P2, Gambit);
        }

        [Test]
        public void StingyBattle_CountsAbilitiesPlayedBeforeOpening()
        {
            var d = BattleOnP1Field(StingyField, Fire180, Water200, player1Deck: Deck(epicField: StingyField));
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Open(P1);

            d.Rejected(new PlayAbility(P1, d.Id(P1, Boost40)), RejectReason.BlockedByFieldCondition);
        }

        [Test]
        public void Seal_BlocksLegendary()
        {
            var d = BattleOnP1Field(SealField, Fire180, Water200, player1Deck: Deck(epicField: SealField));
            d.Open(P1);

            d.Rejected(new PlayAbility(P1, d.Id(P1, Gambit)), RejectReason.BlockedByFieldCondition);
            d.Play(P1, Boost50);
        }

        [Test]
        public void CancelledCondition_NoLongerRestricts()
        {
            var deck = Deck(epicAbility: CancelCondition, epicField: StingyField);
            var d = BattleOnP1Field(StingyField, Fire180, Water200, player1Deck: deck);
            d.Open(P1);
            d.Play(P1, CancelCondition);
            d.Ready(P2);

            d.Play(P1, Boost50);
        }

        [Test]
        public void LowerPowerWins_EqualPower_PlacedEarlierWins()
        {
            var d = BattleOnP1Field(HumbleHall, Fire180, Fire180, player1Deck: Deck(legendaryField: HumbleHall));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(d.Last<BattleResolved>().HostPower, d.Last<BattleResolved>().GuestPower);
            Assert.AreEqual(P1, d.Last<BattleResolved>().Winner);
            Assert.AreEqual(TieBreak.EarlierPlacement, d.Last<BattleResolved>().TieBreak);
        }

        [Test]
        public void NarrowMargin_SmallDifferenceIsDraw()
        {
            var d = BattleOnP1Field(NarrowField, Fire180, Earth250, player1Deck: Deck(legendaryField: NarrowField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(240, result.HostPower);
            Assert.AreEqual(250, result.GuestPower);
            Assert.IsNull(result.Winner);
        }

        [Test]
        public void NarrowMargin_LargeDifferenceWins()
        {
            var d = BattleOnP1Field(NarrowField, Fire180, Water200, player1Deck: Deck(legendaryField: NarrowField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(P1, d.Last<BattleResolved>().Winner);
        }

        [Test]
        public void Mercy_LoserReturnsToHand()
        {
            var d = BattleOnP1Field(MercyField, Fire180, Water200, player1Deck: Deck(commonField: MercyField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(P2, d.Last<BattleResolved>().Winner);
            Assert.IsTrue(d.InHand(P1, Fire180));
            Assert.IsFalse(d.IsOut(P1, Fire180));
            Assert.AreEqual(1, d.Wins(P2));
        }

        [Test]
        public void Unbreakable_FieldStaysOpenAfterBattle()
        {
            var d = BattleOnP1Field(UnbreakableField, Fire180, Water200, player1Deck: Deck(legendaryField: UnbreakableField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            var field = d.Field(P1, UnbreakableField);
            Assert.AreEqual(P1, d.Last<BattleResolved>().Winner);
            Assert.IsFalse(field.IsRemoved);
            Assert.IsTrue(field.IsOpen);
            Assert.IsTrue(d.InHand(P1, Fire180));
            Assert.IsTrue(d.IsOut(P2, Water200));
        }

        [Test]
        public void Sacrifice_WinnerGoesOutToo()
        {
            var d = BattleOnP1Field(SacrificeField, Fire180, Water200, player1Deck: Deck(legendaryField: SacrificeField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(P1, d.Last<BattleResolved>().Winner);
            Assert.IsTrue(d.IsOut(P1, Fire180));
            Assert.IsTrue(d.IsOut(P2, Water200));
            Assert.AreEqual(1, d.Wins(P1));
        }

        [Test]
        public void SacrificeOnlyGuest_WinningHostStays()
        {
            var d = BattleOnP1Field(GuestSacrificeField, Fire180, Water200, player1Deck: Deck(legendaryField: GuestSacrificeField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(P1, d.Last<BattleResolved>().Winner);
            Assert.IsFalse(d.IsOut(P1, Fire180));
            Assert.IsTrue(d.IsOut(P2, Water200));
        }

        [Test]
        public void SacrificeOnlyGuest_WinningGuestGoesOutToo()
        {
            var d = BattleOnP1Field(GuestSacrificeField, Water200, Earth250, player1Deck: Deck(legendaryField: GuestSacrificeField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(P2, d.Last<BattleResolved>().Winner);
            Assert.IsTrue(d.IsOut(P2, Earth250));
            Assert.IsTrue(d.IsOut(P1, Water200));
            Assert.AreEqual(1, d.Wins(P2));
        }

        [Test]
        public void Trophy_WinnerChoosesAbilityFromDiscard()
        {
            var d = BattleOnP1Field(TrophyField, Fire180, Water200, player1Deck: Deck(epicField: TrophyField));
            d.Open(P1);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(MatchPhase.AwaitingChoice, d.State.Phase);
            var request = d.Last<ChoiceRequested>();
            Assert.AreEqual(P1, request.Player);
            CollectionAssert.AreEqual(new[] { Boost50 }, request.Options.Select(option => option.CardId).ToArray());

            d.Rejected(new EndTurn(P2), RejectReason.WrongPhase);
            d.Rejected(new ChooseCard(P2, d.Id(P1, Boost50)), RejectReason.NotYourTurn);
            d.Rejected(new ChooseCard(P1, d.Id(P1, Boost40)), RejectReason.NotAChoiceOption);

            d.Ok(new ChooseCard(P1, d.Id(P1, Boost50)));
            Assert.IsTrue(d.InHand(P1, Boost50));
            Assert.AreEqual(MatchPhase.Turn, d.State.Phase);
            d.EndTurn(P2);
        }

        [Test]
        public void Trophy_NothingToTake_NoChoice()
        {
            var d = BattleOnP1Field(TrophyField, Fire180, Water200, player1Deck: Deck(epicField: TrophyField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(MatchPhase.Turn, d.State.Phase);
        }

        [Test]
        public void HostHome_PenaltiesOnlyForGuest()
        {
            var d = BattleOnP1Field(HostHomeField, Water200, Fire180, player1Deck: Deck(commonField: HostHomeField));
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(200, d.Last<BattleResolved>().HostPower, "Water −30 does not act on the host.");
            Assert.AreEqual(180 + 40, d.Last<BattleResolved>().GuestPower);
        }
    }
}
