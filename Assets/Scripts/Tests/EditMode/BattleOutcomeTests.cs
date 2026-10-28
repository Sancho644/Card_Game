using Game.Core.Battle;
using Game.Core.Events;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>The consequences table and the draw rule from docs/rules.md §6.</summary>
    public class BattleOutcomeTests
    {
        [Test]
        public void ClosedField_HostWins_FieldAndHostCreatureStay_GuestCreatureOut()
        {
            var d = BattleOnP1Forge(Water200, Fire180);
            d.Ready(P1);
            d.Ready(P2);

            var forge = d.Field(P1, Forge);
            var result = d.Last<BattleResolved>();
            Assert.AreEqual(200, result.HostPower);
            Assert.AreEqual(180, result.GuestPower);
            Assert.AreEqual(P1, result.Winner);
            Assert.IsFalse(forge.IsRemoved);
            Assert.IsFalse(forge.IsOpen);
            Assert.AreEqual(Water200, forge.OccupantOf(P1).Definition.Id);
            Assert.IsNull(forge.OccupantOf(P2));
            Assert.IsTrue(d.IsOut(P2, Fire180));
            Assert.AreEqual(1, d.Wins(P1));
            Assert.AreEqual(0, d.Wins(P2));
        }

        [Test]
        public void ClosedField_GuestWins_FieldRemoved_GuestCreatureToHand_HostCreatureOut()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Ready(P1);
            d.Ready(P2);

            var forge = d.Field(P1, Forge);
            Assert.AreEqual(P2, d.Last<BattleResolved>().Winner);
            Assert.IsTrue(forge.IsRemoved);
            Assert.IsNull(forge.OccupantOf(P1));
            Assert.IsNull(forge.OccupantOf(P2));
            Assert.IsTrue(d.InHand(P2, Water200));
            Assert.IsTrue(d.IsOut(P1, Fire180));
            Assert.AreEqual(0, d.Wins(P1));
            Assert.AreEqual(1, d.Wins(P2));
        }

        [Test]
        public void OpenField_HostWins_FieldRemoved_HostCreatureToHand_GuestCreatureOut()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(240, result.HostPower);
            Assert.AreEqual(170, result.GuestPower);
            Assert.AreEqual(P1, result.Winner);
            Assert.IsTrue(d.Field(P1, Forge).IsRemoved);
            Assert.IsTrue(d.InHand(P1, Fire180));
            Assert.IsTrue(d.IsOut(P2, Water200));
            Assert.AreEqual(1, d.Wins(P1));
        }

        [Test]
        public void OpenField_GuestWins_FieldRemoved_GuestCreatureToHand_HostCreatureOut()
        {
            var d = BattleOnP1Forge(Water200, Fire180);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(170, result.HostPower);
            Assert.AreEqual(240, result.GuestPower);
            Assert.AreEqual(P2, result.Winner);
            Assert.IsTrue(d.Field(P1, Forge).IsRemoved);
            Assert.IsTrue(d.InHand(P2, Fire180));
            Assert.IsTrue(d.IsOut(P1, Water200));
            Assert.AreEqual(1, d.Wins(P2));
        }

        [Test]
        public void EqualPower_ClosedField_HostPlacedEarlierWins()
        {
            var d = BattleOnP1Forge(Fire180, Fire180);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(P1, result.Winner);
            Assert.AreEqual(TieBreak.EarlierPlacement, result.TieBreak);
            Assert.AreEqual(1, d.Wins(P1));
            Assert.IsTrue(d.IsOut(P2, Fire180));
            Assert.AreEqual(Fire180, d.Field(P1, Forge).OccupantOf(P1).Definition.Id, "The host keeps its closed field.");
        }

        [Test]
        public void EqualPower_OpenField_PlacedEarlierWins()
        {
            var d = BattleOnP1Forge(Fire180, Fire180);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(240, result.HostPower);
            Assert.AreEqual(240, result.GuestPower);
            Assert.AreEqual(P1, result.Winner);
            Assert.IsTrue(d.Field(P1, Forge).IsRemoved);
        }

        [Test]
        public void EqualPower_GuestPlacedEarlierWins()
        {
            var d = Begin(P1);
            d.Place(P1, Fire180, P2, Forge);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P2, Forge);
            Assert.AreEqual(P1, d.State.Battle.CurrentActor, "The creature placed earlier acts first.");

            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(P2, d.Last<BattleStarted>().Host);
            Assert.AreEqual(P1, result.Winner);
            Assert.AreEqual(TieBreak.EarlierPlacement, result.TieBreak);
        }

        [Test]
        public void Draw_ByCardText_EqualPower_BothCreaturesToHand_OpenFieldRemoved_AbilitiesSpent()
        {
            var d = BattleOnP1Field(NarrowField, Fire180, Earth250, player1Deck: Deck(legendaryField: NarrowField));
            d.Open(P1);
            d.Play(P1, Boost50);
            d.Play(P2, Boost40);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(180 + 60 + 50, result.HostPower);
            Assert.AreEqual(250 + 40, result.GuestPower);
            Assert.IsNull(result.Winner, "«Узкая грань»: equal power is a draw, the placement order does not decide it.");
            Assert.AreEqual(TieBreak.None, result.TieBreak);
            Assert.IsTrue(d.Field(P1, NarrowField).IsRemoved);
            Assert.IsTrue(d.InHand(P1, Fire180));
            Assert.IsTrue(d.InHand(P2, Earth250));
            Assert.IsTrue(d.InDiscard(P1, Boost50));
            Assert.IsTrue(d.InDiscard(P2, Boost40));
            Assert.AreEqual(0, d.Wins(P1) + d.Wins(P2));
        }

        [Test]
        public void AfterBattle_TurnContinuesForActivePlayer()
        {
            var d = BattleOnP1Forge(Water200, Fire180);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(MatchPhase.Turn, d.State.Phase);
            Assert.AreEqual(P2, d.State.ActivePlayer);
            Assert.IsNull(d.State.Battle);
        }
    }
}
