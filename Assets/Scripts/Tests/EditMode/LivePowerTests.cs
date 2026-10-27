using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;
using Game.Core.Commands;
using Game.Core.Events;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>MatchEngine.GetBattlePower: the power during the battle, counted like the final result.</summary>
    public class LivePowerTests
    {
        [Test]
        public void OutsideBattle_IsNull()
        {
            var d = Begin(P1);

            Assert.IsNull(d.Engine.GetBattlePower());
        }

        [Test]
        public void ClosedField_GivesNothing_OpeningAddsFieldValue()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            var closed = d.Engine.GetBattlePower();
            Assert.AreEqual(d.Field(P1, Forge).Id, closed.FieldId);
            Assert.AreEqual(P1, closed.Host);
            Assert.AreEqual(0, closed.HostPower.FieldValue);
            Assert.AreEqual(180, closed.HostPower.FinalPower);
            Assert.AreEqual(200, closed.GuestPower.FinalPower);

            d.Open(P1);

            var open = d.Engine.GetBattlePower();
            Assert.AreEqual(60, open.Of(P1).FieldValue);
            Assert.AreEqual(240, open.Of(P1).FinalPower);
            Assert.AreEqual(-30, open.Of(P2).FieldValue);
            Assert.AreEqual(170, open.Of(P2).FinalPower);
        }

        [Test]
        public void PlayedAbility_AddsContribution_CancelledOneDropsIt()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(epicAbility: Intercept));
            d.Play(P1, Boost50);

            var boosted = d.Engine.GetBattlePower().Of(P1);
            Assert.AreEqual(Boost50, boosted.Abilities.Single().Source.CardId);
            Assert.AreEqual(230, boosted.FinalPower);

            d.Play(P2, Intercept);

            var cancelled = d.Engine.GetBattlePower().Of(P1);
            CollectionAssert.IsEmpty(cancelled.Abilities);
            Assert.AreEqual(180, cancelled.FinalPower);
        }

        [Test]
        public void ComparisonRule_ComesFromTheOpenFieldCondition()
        {
            var d = BattleOnP1Field(HumbleHall, Fire180, Water200, player1Deck: Deck(legendaryField: HumbleHall));
            Assert.AreEqual(ComparisonRule.HigherPowerWins, d.Engine.GetBattlePower().Rule);

            d.Open(P1);

            Assert.AreEqual(ComparisonRule.LowerPowerWins, d.Engine.GetBattlePower().Rule);
        }

        [Test]
        public void RandomMatches_PowerBeforeLastReady_EqualsFinalResult()
        {
            var decks = new List<Decks.DeckList>
            {
                Deck(),
                Deck(commonAbility: Desperation, epicAbility: Intercept, legendaryAbility: Maneuver, epicField: TrophyField),
                Deck(commonAbility: HostBoost, epicAbility: LastWord, legendaryAbility: SecondLife, legendaryField: SacrificeField),
                Deck(commonAbility: FieldBlessing, epicAbility: ForcedReveal, legendaryAbility: StormRush, commonField: MercyField),
                Deck(commonAbility: GuestBoost, epicAbility: CancelCondition, legendaryAbility: Retreat, epicField: SilenceField),
                Deck(commonAbility: FireBoost, epicAbility: TieBreaker, legendaryAbility: Scout, legendaryField: UnbreakableField),
                Deck(commonAbility: ClosedFieldBoost, epicAbility: Blockade, legendaryAbility: DarkPrice, epicField: DoublingLake),
                Deck(legendaryField: HumbleHall),
            };
            var compared = 0;

            for (var seed = 1; seed <= 40; seed++)
            {
                var random = new System.Random(seed);
                var d = Create((ulong)seed, player1Deck: decks[random.Next(decks.Count)], player2Deck: decks[random.Next(decks.Count)]);

                for (var step = 0; step < 1000; step++)
                {
                    var awaiting = d.Engine.GetView(P1).AwaitingPlayer;
                    if (awaiting == null)
                        break;

                    var actions = d.Engine.GetLegalActions(awaiting.Value);
                    var command = actions[random.Next(actions.Count)];
                    var before = d.Engine.GetBattlePower();
                    var accepted = d.Do(command);
                    Assert.IsTrue(accepted.IsAccepted, $"seed {seed}, step {step}");
                    var events = accepted.Events;

                    var result = events.OfType<BattleResolved>().FirstOrDefault();
                    if (command is Ready && result != null)
                    {
                        AssertSame(before.HostPower, result.Host, seed);
                        AssertSame(before.GuestPower, result.Guest, seed);
                        Assert.AreEqual(result.Rule, before.Rule, $"seed {seed}");
                        compared++;
                    }
                }
            }

            Assert.Greater(compared, 20, "Enough battles end with Ready to compare.");
        }

        private static void AssertSame(PowerBreakdown expected, PowerBreakdown actual, int seed)
        {
            Assert.AreEqual(expected.BasePower, actual.BasePower, $"seed {seed}");
            Assert.AreEqual(expected.FieldValue, actual.FieldValue, $"seed {seed}");
            Assert.AreEqual(expected.FinalPower, actual.FinalPower, $"seed {seed}");
            CollectionAssert.AreEqual(
                expected.Abilities.Select(ability => (ability.Source?.InstanceId, ability.Amount)),
                actual.Abilities.Select(ability => (ability.Source?.InstanceId, ability.Amount)),
                $"seed {seed}");
        }
    }
}
