using System.Linq;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.Forecast;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    public sealed class FieldDeploymentTests
    {
        private static readonly PlayerId P1 = PlayerId.Player1;
        private static readonly PlayerId P2 = PlayerId.Player2;

        [Test]
        public void Fields_AppearInHand_AndLeaveItWhenPlayed()
        {
            var e = Create(); var events = e.Start();
            Assert.AreEqual(9, e.GetView(P1).Hand.Count);
            Assert.AreEqual(9, e.GetView(P2).OpponentHandCount);
            Assert.AreEqual(9, events.OfType<HandDealt>().First().Cards.Count);
            var field = Field(e, P1, Forge);
            Assert.IsTrue(e.GetView(P1).Hand.Any(c => c.InstanceId == field.Id));
            Select(e, P1, Forge);
            Assert.AreEqual(8, e.GetView(P1).Hand.Count);
            Assert.AreEqual(8, e.GetView(P2).OpponentHandCount);
            Assert.IsFalse(e.GetView(P1).Hand.Any(c => c.InstanceId == field.Id));
        }

        [Test]
        public void Setup_OffersPrivateReserveBeforeCoin()
        {
            var e = Create(); var events = e.Start();
            Assert.AreEqual(0, e.State.Fields.Count(f => f.IsActive));
            Assert.IsNull(e.State.CoinTossWinner);
            Assert.AreEqual(3, e.GetView(P1).ChoiceOptions.Count);
            Assert.IsEmpty(e.GetView(P2).ChoiceOptions);
            Assert.IsTrue(e.GetView(P2).Fields.Where(f => f.Owner == P1).All(f => f.CardId == null));
            var choice = events.OfType<ChoiceRequested>().Single();
            Assert.IsFalse(choice.IsVisibleTo(P2));
            Assert.IsEmpty(e.GetLegalActions(P2));
            Assert.AreEqual(RejectReason.NotYourTurn, e.Execute(new ChooseCard(P2, choice.Options[0].InstanceId)).Reason);
            Assert.AreEqual(RejectReason.NotAChoiceOption, e.Execute(new ChooseCard(P1, -1)).Reason);
        }

        [Test]
        public void BothSelections_ActivateOnlyChosenFields_ThenCoin()
        {
            var e = Create(); e.Start(); Select(e, P1, Forge);
            Assert.AreEqual(P2, e.GetView(P1).AwaitingPlayer);
            Assert.IsEmpty(e.GetView(P1).ChoiceOptions);
            Select(e, P2, Plain);
            Assert.AreEqual(MatchPhase.ChoosingTurnOrder, e.State.Phase);
            Assert.AreEqual(2, e.State.Fields.Count(f => f.IsActive));
            Assert.AreEqual(4, e.State.Fields.Count(f => f.IsInReserve));
            Assert.AreEqual(RejectReason.WrongPhase, e.Execute(new ChooseCard(P2, Field(e, P2, Temple).Id)).Reason);
        }

        [Test]
        public void Reserve_IsNotCreatureTarget()
        {
            var e = Begin();
            var reserve = Field(e, P1, Temple);
            var creature = e.State.Player(P1).Hand.First(c => c.Definition.Id == Fire180);
            Assert.AreEqual(RejectReason.FieldRemoved, e.Execute(new PlaceCreature(P1, creature.Id, reserve.Id)).Reason);
            Assert.IsFalse(e.GetLegalActions(P1).OfType<PlaceCreature>().Any(c => c.FieldId == reserve.Id));
        }

        [Test]
        public void RemovedField_ReplacementAtNextOwnerTurn_DoesNotSpendMainAction()
        {
            var e = Begin(); var field = Field(e, P1, Forge);
            Place(e, P1, Fire180, field.Id); Ok(e, new EndTurn(P1));
            Place(e, P2, Water200, field.Id); Ok(e, new OpenField(P1));
            Ok(e, new Ready(P1)); Ok(e, new Ready(P2));
            Assert.IsTrue(field.IsRemoved);
            Assert.AreEqual(MatchPhase.Turn, e.State.Phase);
            Ok(e, new EndTurn(P2));
            Assert.AreEqual(ChoicePurpose.ReplacementField, e.State.PendingChoice.Purpose);
            Assert.AreEqual(2, e.GetView(P1).ChoiceOptions.Count);
            Assert.IsEmpty(e.GetView(P2).ChoiceOptions);
            Assert.AreEqual(RejectReason.WrongPhase, e.Execute(new EndTurn(P1)).Reason);
            Select(e, P1, Temple);
            Assert.AreEqual(MatchPhase.Turn, e.State.Phase);
            Assert.IsFalse(e.State.MainActionDone);
            Assert.AreEqual(1, e.State.Fields.Count(f => f.Owner == P1 && f.IsActive));
            Assert.IsTrue(e.GetLegalActions(P1).OfType<PlaceCreature>().Any());
        }

        [Test]
        public void SurvivingClosedField_DoesNotOfferReplacement()
        {
            var e = Begin(); var field = Field(e, P1, Forge);
            Place(e, P1, Earth250, field.Id); Ok(e, new EndTurn(P1));
            Place(e, P2, Fire180, field.Id); Ok(e, new Ready(P1)); Ok(e, new Ready(P2));
            Ok(e, new EndTurn(P2));
            Assert.IsTrue(field.IsActive);
            Assert.IsNull(e.State.PendingChoice);
            Assert.AreEqual(MatchPhase.Turn, e.State.Phase);
        }

        [Test]
        public void Forecast_RestoresReservesAndPrivateChoices()
        {
            var e = Create(); e.Start();
            var guesses = new HiddenCardsGuess(e.State.Player(P2).Hand.Select(c => c.Definition.Id).ToList(),
                e.State.Fields.Where(f => f.Owner == P2).ToDictionary(f => f.Id, f => f.Definition.Id));
            var f = new ForecastBuilder(Catalog(), GameRules.Default).Build(e.GetView(P1), guesses, new XorShiftRandom(5));
            Assert.AreEqual(9, f.GetView(P1).Hand.Count);
            Assert.AreEqual(9, f.GetView(P2).Hand.Count);
            Select(f, P1, Forge);
            Assert.AreEqual(5, f.State.Fields.Count(x => x.IsInReserve));
            Assert.AreEqual(3, f.GetLegalActions(P2).Count);
            Select(f, P2, Plain);
            Assert.AreEqual(MatchPhase.ChoosingTurnOrder, f.State.Phase);
            Assert.AreEqual(6, e.State.Fields.Count(x => x.IsInReserve));
        }

        private static MatchEngine Create() => new MatchEngine(new MatchSetup(Deck(), Deck()), Catalog(), GameRules.Default, new XorShiftRandom(3));
        private static MatchEngine Begin()
        {
            var e = Create(); e.Start(); Select(e, P1, Forge); Select(e, P2, Plain);
            Ok(e, new ChooseTurnOrder(e.State.CoinTossWinner.Value, e.State.CoinTossWinner == P1)); return e;
        }
        private static FieldSlot Field(MatchEngine e, PlayerId p, string id) => e.State.Fields.Single(f => f.Owner == p && f.Definition.Id == id);
        private static void Select(MatchEngine e, PlayerId p, string id) => Ok(e, new ChooseCard(p, Field(e, p, id).Id));
        private static void Place(MatchEngine e, PlayerId p, string id, int field) => Ok(e, new PlaceCreature(p, e.State.Player(p).Hand.Single(c => c.Definition.Id == id).Id, field));
        private static void Ok(MatchEngine e, GameCommand c) => Assert.IsTrue(e.Execute(c).IsAccepted, c.GetType().Name);
    }
}
