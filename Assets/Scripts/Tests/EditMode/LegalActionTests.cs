using System.Collections.Generic;
using System.Linq;
using Game.Core.Commands;
using Game.Core.Decks;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>MatchEngine.GetLegalActions: the only source of allowed actions for presentation and AI.</summary>
    public class LegalActionTests
    {
        [Test]
        public void TurnStart_OffersEveryPlacement_AndNothingElse()
        {
            var d = Begin(P1);

            var actions = d.Engine.GetLegalActions(P1);
            Assert.IsTrue(actions.All(action => action is PlaceCreature));
            Assert.AreEqual(3 * 6, actions.Count, "Three creatures, six fields.");
        }

        [Test]
        public void OpponentOfActivePlayer_HasNoActions()
        {
            var d = Begin(P1);

            CollectionAssert.IsEmpty(d.Engine.GetLegalActions(P2));
        }

        [Test]
        public void Battle_HostMayOpenPlayOrPressReady()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            var actions = d.Engine.GetLegalActions(P1);
            Assert.AreEqual(1, actions.OfType<OpenField>().Count());
            Assert.AreEqual(1, actions.OfType<Ready>().Count());
            Assert.AreEqual(3, actions.OfType<PlayAbility>().Count(), "Boost50, Boost40, Gambit.");
            Assert.AreEqual(5, actions.Count);
            CollectionAssert.IsEmpty(d.Engine.GetLegalActions(P2));
        }

        [Test]
        public void Retreat_IsOfferedOnOpponentBattleTurn()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(legendaryAbility: Retreat));

            var actions = d.Engine.GetLegalActions(P2);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(d.Id(P2, Retreat), ((PlayAbility)actions[0]).AbilityId);
        }

        [Test]
        public void Maneuver_OffersOnlyValidTargets()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Maneuver));
            d.Place(P1, Fire180, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Water200, P2, Plain);
            d.EndTurn(P2);

            var maneuvers = d.Engine.GetLegalActions(P1)
                .OfType<PlayAbility>()
                .Where(action => action.AbilityId == d.Id(P1, Maneuver))
                .ToList();
            Assert.AreEqual(5, maneuvers.Count, "Any field except the one Fire180 stands on.");
            Assert.IsTrue(maneuvers.All(action => action.Targets.CreatureId == d.Id(P1, Fire180)));
            Assert.IsTrue(maneuvers.All(action => action.Targets.FieldId != d.Field(P1, Plain).Id));
        }

        [Test]
        public void SignatureWithPlacement_OfferedOnlyWhereBattleStarts()
        {
            var d = Begin(P1, player2Deck: Deck(legendaryAbility: StormRush));
            d.Place(P1, Fire180, P1, Forge);
            d.EndTurn(P1);

            var withSignature = d.Engine.GetLegalActions(P2)
                .OfType<PlaceCreature>()
                .Where(action => action.SignatureAbilityId != null)
                .ToList();
            Assert.AreEqual(1, withSignature.Count);
            Assert.AreEqual(d.Id(P2, Earth250), withSignature[0].CreatureId);
            Assert.AreEqual(d.Field(P1, Forge).Id, withSignature[0].FieldId);
        }

        [Test]
        public void PendingChoice_OffersOptionsOnly()
        {
            var d = BattleOnP1Field(TrophyField, Fire180, Water200, player1Deck: Deck(epicField: TrophyField));
            d.Open(P1);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Ready(P1);

            var actions = d.Engine.GetLegalActions(P1);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(d.Id(P1, Boost50), ((ChooseCard)actions[0]).CardId);
        }

        [Test]
        public void RandomMatches_EveryLegalActionIsAccepted_AndAwaitingPlayerAlwaysHasOne()
        {
            var decks = new List<DeckList>
            {
                Deck(),
                Deck(commonAbility: Desperation, epicAbility: Intercept, legendaryAbility: Maneuver, epicField: TrophyField),
                Deck(commonAbility: HostBoost, epicAbility: LastWord, legendaryAbility: SecondLife, legendaryField: SacrificeField),
                Deck(commonAbility: FieldBlessing, epicAbility: ForcedReveal, legendaryAbility: StormRush, commonField: MercyField),
                Deck(commonAbility: GuestBoost, epicAbility: CancelCondition, legendaryAbility: Retreat, epicField: SilenceField),
                Deck(commonAbility: FireBoost, epicAbility: TieBreaker, legendaryAbility: Scout, legendaryField: UnbreakableField),
                Deck(commonAbility: ClosedFieldBoost, epicAbility: Blockade, legendaryAbility: DarkPrice, epicField: DoublingLake),
            };
            var ended = 0;

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
                    Assert.IsNotEmpty(actions, $"seed {seed}, step {step}: {awaiting} has nothing to do in {d.State.Phase}");
                    d.Ok(actions[random.Next(actions.Count)]);
                }

                if (d.State.Result != null)
                    ended++;
            }

            Assert.Greater(ended, 0, "At least some random matches reach the end.");
        }
    }
}
