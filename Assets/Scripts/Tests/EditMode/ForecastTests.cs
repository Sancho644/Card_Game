using System;
using System.Linq;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.Forecast;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>A forecast engine is rebuilt from a player view plus a guess about the hidden cards.</summary>
    public class ForecastTests
    {
        [Test]
        public void ExactGuess_BattleEndsAsInRealMatch()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(epicAbility: Intercept));
            d.Open(P1);
            d.Play(P1, Boost50);
            var forecast = Forecast(d, P1, ExactGuess(d, P1));

            d.Ready(P2);
            d.Ready(P1);
            var forecastEvents = Accept(forecast, new Ready(P2)).Concat(Accept(forecast, new Ready(P1))).ToList();

            var real = d.Last<BattleResolved>();
            var predicted = forecastEvents.OfType<BattleResolved>().Single();
            Assert.AreEqual(real.HostPower, predicted.HostPower);
            Assert.AreEqual(real.GuestPower, predicted.GuestPower);
            Assert.AreEqual(real.Winner, predicted.Winner);
        }

        [Test]
        public void ExactGuess_RestoresBattle_PowerAndLegalActions()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Play(P1, Boost50);
            d.Ready(P2);
            var forecast = Forecast(d, P1, ExactGuess(d, P1));

            Assert.AreEqual(d.Engine.GetBattlePower().HostPower.FinalPower, forecast.GetBattlePower().HostPower.FinalPower);
            Assert.AreEqual(d.Engine.GetBattlePower().GuestPower.FinalPower, forecast.GetBattlePower().GuestPower.FinalPower);
            Assert.IsTrue(forecast.State.Battle.IsReady(P2));
            Assert.AreEqual(d.Engine.GetLegalActions(P1).Count, forecast.GetLegalActions(P1).Count);
        }

        [Test]
        public void ForecastOfOpenedField_UsesItsRealValues()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Open(P1);
            var guess = new HiddenCardsGuess(OpponentHand(d, P1), d.State.Fields
                .Where(field => field.Owner == P1 && !field.IsOpen)
                .ToDictionary(field => field.Id, _ => Plain));

            var forecast = Forecast(d, P2, guess);

            Assert.AreEqual(Forge, forecast.State.Battle.Field.Definition.Id);
            Assert.AreEqual(180 + 60, forecast.GetBattlePower().HostPower.FinalPower);
        }

        [Test]
        public void ViewAndForecast_DoNotDependOnHiddenCards()
        {
            // The same public history, but Player2 holds other abilities and owns other field cards.
            var first = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck());
            var second = BattleOnP1Forge(Fire180, Water200,
                player2Deck: Deck(commonAbility: HostBoost, epicAbility: Intercept, legendaryAbility: Scout, epicField: SilenceField));
            var guess = new HiddenCardsGuess(
                Enumerable.Repeat(Boost50, first.Engine.GetView(P1).OpponentHandCount).ToList(),
                first.State.Fields.Where(field => field.Owner == P2).ToDictionary(field => field.Id, _ => Plain));

            Assert.AreEqual(Describe(first.Engine.GetView(P1)), Describe(second.Engine.GetView(P1)));
            Assert.AreEqual(
                Describe(Forecast(first, P1, guess).GetView(P2)),
                Describe(Forecast(second, P1, guess).GetView(P2)));
        }

        [Test]
        public void GuessWithWrongHandSize_Rejected()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            var guess = new HiddenCardsGuess(new[] { Boost50 }, ExactGuess(d, P1).HiddenFields);

            Assert.Throws<ArgumentException>(() => Forecast(d, P1, guess));
        }

        private static MatchEngine Forecast(MatchDriver d, PlayerId viewer, HiddenCardsGuess guess) =>
            new ForecastBuilder(Catalog(), GameRules.Default).Build(d.Engine.GetView(viewer), guess, new XorShiftRandom(7));

        private static HiddenCardsGuess ExactGuess(MatchDriver d, PlayerId viewer) =>
            new HiddenCardsGuess(
                OpponentHand(d, viewer.Opponent()),
                d.State.Fields
                    .Where(field => field.Owner != viewer && !field.IsOpen)
                    .ToDictionary(field => field.Id, field => field.Definition.Id));

        private static string[] OpponentHand(MatchDriver d, PlayerId opponent) =>
            d.State.Player(opponent).Hand.Select(card => card.Definition.Id).ToArray();

        private static System.Collections.Generic.IReadOnlyList<GameEvent> Accept(MatchEngine engine, GameCommand command)
        {
            var result = engine.Execute(command);
            Assert.IsTrue(result.IsAccepted, $"{command.GetType().Name} rejected: {result.Reason}");
            return result.Events;
        }

        /// <summary>Everything a view shows, as text, to compare two views.</summary>
        private static string Describe(PlayerView view)
        {
            var fields = view.Fields.Select(field =>
                $"{field.Id}:{field.Owner}:{field.CardId}:{field.IsOpen}:{field.IsRemoved}:{field.IsLocked}:" +
                string.Join("/", PlayerIds.All.Select(id => $"{field.Occupant(id)}@{field.PlacementOrder(id)}")));
            var battle = view.Battle == null
                ? "-"
                : $"{view.Battle.FieldId}:{view.Battle.CurrentActor}:{view.Battle.IsReady(P1)}:{view.Battle.IsReady(P2)}:" +
                  string.Join(",", view.Battle.PlayedAbilities.Select(played => $"{played.Player}{played.Card}{played.IsCancelled}"));
            var piles = PlayerIds.All.Select(id =>
                $"{id}:{view.Wins(id)}:{string.Join(",", view.Discard(id))}:{string.Join(",", view.OutOfGame(id))}");
            return string.Join("|", new[]
                {
                    view.Phase.ToString(), view.ActivePlayer.ToString(), view.MainActionDone.ToString(),
                    string.Join(",", view.Hand), view.OpponentHandCount.ToString(), battle,
                }
                .Concat(fields)
                .Concat(piles));
        }
    }
}
