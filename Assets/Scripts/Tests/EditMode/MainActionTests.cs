using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Placing a creature, the challenge and skipping the main action.</summary>
    public class MainActionTests
    {
        [Test]
        public void Place_OnOpponentField_Allowed()
        {
            var d = Begin(P1);
            d.Place(P1, Fire180, P2, Plain);

            Assert.AreEqual(Fire180, d.Field(P2, Plain).OccupantOf(P1).Definition.Id);
            Assert.IsFalse(d.InHand(P1, Fire180));
            Assert.IsTrue(d.State.MainActionDone);
            Assert.AreEqual(MatchPhase.Turn, d.State.Phase);
        }

        [Test]
        public void Place_SecondCreatureOnField_StartsBattle()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            Assert.AreEqual(MatchPhase.Battle, d.State.Phase);
            Assert.AreEqual(d.Field(P1, Forge).Id, d.Last<BattleStarted>().FieldId);
        }

        [Test]
        public void Place_TwiceInOneTurn_Rejected()
        {
            var d = Begin(P1);
            d.Place(P1, Fire180, P1, Plain);

            d.Rejected(new PlaceCreature(P1, d.Id(P1, Water200), d.Field(P1, Forge).Id), RejectReason.MainActionAlreadyDone);
        }

        [Test]
        public void Place_WhereOwnCreatureStands_Rejected()
        {
            var d = Begin(P1);
            d.Place(P1, Fire180, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P2, Plain);
            d.EndTurn(P2);

            d.Rejected(new PlaceCreature(P1, d.Id(P1, Water200), d.Field(P1, Plain).Id), RejectReason.AlreadyOnField);
        }

        [Test]
        public void Place_OutOfTurn_Rejected()
        {
            var d = Begin(P1);

            d.Rejected(new PlaceCreature(P2, d.Id(P2, Fire180), d.Field(P2, Plain).Id), RejectReason.NotYourTurn);
        }

        [Test]
        public void Place_OpponentsCard_Rejected()
        {
            var d = Begin(P1);

            d.Rejected(new PlaceCreature(P1, d.Id(P2, Fire180), d.Field(P1, Plain).Id), RejectReason.CardNotInHand);
        }

        [Test]
        public void Place_Ability_Rejected()
        {
            var d = Begin(P1);

            d.Rejected(new PlaceCreature(P1, d.Id(P1, Boost50), d.Field(P1, Plain).Id), RejectReason.NotACreature);
        }

        [Test]
        public void Place_OnRemovedField_Rejected()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Ready(P1);
            d.Ready(P2);
            d.EndTurn(P2);

            d.Rejected(new PlaceCreature(P1, d.Id(P1, Water200), d.Field(P1, Forge).Id), RejectReason.FieldRemoved);
        }

        [Test]
        public void EndTurn_WithoutMainAction_WhenOneIsPossible_Rejected()
        {
            var d = Begin(P1);

            d.Rejected(new EndTurn(P1), RejectReason.MainActionRequired);
        }

        [Test]
        public void Challenge_WithCreaturesInHand_Rejected()
        {
            var d = Begin(P1);
            d.Place(P1, Fire180, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P2, Plain);
            d.EndTurn(P2);

            d.Rejected(new Challenge(P1, d.Id(P1, Fire180), d.Field(P2, Plain).Id), RejectReason.HandHasCreatures);
        }

        [Test]
        public void Challenge_MovesCreature_AndStartsBattle_OpponentActsFirst()
        {
            var d = AllCreaturesPlacedWithoutBattles();
            var source = d.Field(P1, Plain);
            var target = d.Field(P2, Plain);
            d.Challenge(P1, Fire180, P2, Plain);

            Assert.IsNull(source.OccupantOf(P1));
            Assert.AreEqual(Fire180, target.OccupantOf(P1).Definition.Id);

            var moved = d.Last<CreatureMoved>();
            Assert.AreEqual(source.Id, moved.FromFieldId);
            Assert.AreEqual(target.Id, moved.ToFieldId);

            var started = d.Last<BattleStarted>();
            Assert.AreEqual(target.Id, started.FieldId);
            Assert.AreEqual(P2, started.Host);
            Assert.AreEqual(P2, started.FirstActor);
        }

        [Test]
        public void Challenge_ToFieldWithoutOpponentCreature_Rejected()
        {
            var d = AllCreaturesPlacedWithoutBattles();

            d.Rejected(new Challenge(P1, d.Id(P1, Fire180), d.Field(P1, Temple).Id), RejectReason.NoOpponentCreature);
        }

        [Test]
        public void Challenge_ToFieldWhereOwnCreatureStands_Rejected()
        {
            var d = AllCreaturesPlacedWithoutBattles();

            d.Rejected(new Challenge(P1, d.Id(P1, Fire180), d.Field(P1, Forge).Id), RejectReason.AlreadyOnField);
        }

        [Test]
        public void Challenge_WithCreatureNotOnField_Rejected()
        {
            var d = AllCreaturesPlacedWithoutBattles();

            d.Rejected(new Challenge(P1, d.Id(P2, Fire180), d.Field(P2, Forge).Id), RejectReason.CreatureNotOnField);
        }

        [Test]
        public void EndTurn_WhenChallengeIsPossible_IsNotASkip()
        {
            var d = AllCreaturesPlacedWithoutBattles();

            d.Rejected(new EndTurn(P1), RejectReason.MainActionRequired);
        }

        /// <summary>
        /// Two creatures each. Every player puts both creatures on own fields; the Temples stay empty.
        /// Player1 is to move with an empty hand.
        /// </summary>
        private static MatchDriver AllCreaturesPlacedWithoutBattles()
        {
            var rules = RulesWithDeck(
                fields: new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary },
                creatures: new[] { Rarity.Common, Rarity.Epic },
                abilities: new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary });
            var deck = new DeckList(new[] { Plain, Forge, Temple, Fire180, Water200, Boost50, Boost40, Gambit });

            var d = Begin(P1, rules, deck, deck);
            d.Place(P1, Fire180, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P2, Plain);
            d.EndTurn(P2);
            d.Place(P1, Water200, P1, Forge);
            d.EndTurn(P1);
            d.Place(P2, Water200, P2, Forge);
            d.EndTurn(P2);
            return d;
        }
    }
}
