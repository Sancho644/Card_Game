using System.Linq;
using Game.Core.Battle;
using Game.Core.Events;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>BattleResolved reports each creature's power by the steps of the count.</summary>
    public class PowerBreakdownTests
    {
        [Test]
        public void Breakdown_ShowsBaseFieldAndAbilities()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Open(P1);
            d.Ready(P1);
            d.Play(P2, Boost50);
            d.Ready(P1);
            d.Ready(P2);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(180, result.Host.BasePower);
            Assert.AreEqual(60, result.Host.FieldValue);
            CollectionAssert.IsEmpty(result.Host.Abilities);
            Assert.AreEqual(240, result.Host.FinalPower);

            Assert.AreEqual(200, result.Guest.BasePower);
            Assert.AreEqual(-30, result.Guest.FieldValue);
            Assert.AreEqual(Boost50, result.Guest.Abilities.Single().Source.CardId);
            Assert.AreEqual(50, result.Guest.Abilities.Single().Amount);
            Assert.AreEqual(220, result.Guest.FinalPower);
            Assert.AreEqual(ComparisonRule.HigherPowerWins, result.Rule);
        }

        [Test]
        public void Breakdown_PenaltyOnOpponent_NamesItsSource()
        {
            var d = BattleOnP1Forge(Earth250, Fire180, player1Deck: Deck(legendaryAbility: ForgeFlame));
            d.Play(P1, ForgeFlame);
            d.Ready(P2);
            d.Ready(P1);

            var penalty = d.Last<BattleResolved>().Guest.Abilities.Single();
            Assert.AreEqual(ForgeFlame, penalty.Source.CardId);
            Assert.AreEqual(-20, penalty.Amount);
        }

        [Test]
        public void Breakdown_ReportsComparisonRuleAndTieBreak()
        {
            var d = BattleOnP1Forge(Fire180, Fire180, player1Deck: Deck(epicAbility: TieBreaker));
            d.Play(P1, TieBreaker);
            d.Play(P2, Boost50);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(TieBreak.Ability, d.Last<BattleResolved>().TieBreak);

            var lower = BattleOnP1Field(HumbleHall, Fire180, Water200, player1Deck: Deck(legendaryField: HumbleHall));
            lower.Open(P1);
            lower.Ready(P1);
            lower.Ready(P2);
            Assert.AreEqual(ComparisonRule.LowerPowerWins, lower.Last<BattleResolved>().Rule);
        }
    }
}
