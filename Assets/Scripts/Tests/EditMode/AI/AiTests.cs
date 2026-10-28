using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.State;
using Game.Services.Match;
using Game.Simulation;
using NUnit.Framework;

namespace Game.AI.Tests
{
    public class AiTests
    {
        private const PlayerId P1 = PlayerId.Player1;
        private const PlayerId P2 = PlayerId.Player2;

        [Test]
        public void Ai_SubmitsOnlyOfferedActions_EngineAcceptsThem_MatchesEnd()
        {
            var cards = new FirstCardSet();
            var decks = new RandomDeckBuilder(cards.Catalog, cards.Rules.Deck);
            for (var seed = 1; seed <= 30; seed++)
            {
                var random = new XorShiftRandom((ulong)seed);
                var session = new LocalMatchSession(cards.Catalog, cards.Rules);
                var scheduler = new ManualScheduler();
                var ended = false;
                session.EventsProduced += batch => ended |= batch.OfType<MatchEnded>().Any();

                var profiles = seed % 2 == 0 ? new[] { cards.Normal, cards.Easy } : new[] { cards.Easy, cards.Normal };
                var probes = new List<OfferedActionProbe>();
                var rejections = new List<string>();
                foreach (var player in new[] { P1, P2 })
                {
                    var brain = new AiBrain(player, profiles[(int)player], cards.Catalog, cards.Rules, new XorShiftRandom((ulong)(seed * 10 + (int)player)));
                    var probe = new OfferedActionProbe(new AiController(brain, profiles[(int)player], scheduler, new XorShiftRandom(1)));
                    var host = new SessionPlayerHost(session, probe, scheduler, offersReactions: true);
                    host.CommandRejected += (command, reason) => rejections.Add($"{command.GetType().Name}: {reason}");
                    probes.Add(probe);
                }

                session.Start(new MatchSetup(decks.Build(random), decks.Build(random)), (ulong)seed);
                for (var step = 0; step < 5000 && !ended && scheduler.HasPending; step++)
                    scheduler.RunPending(1);

                Assert.IsTrue(ended, $"Match {seed} did not end.");
                CollectionAssert.IsEmpty(rejections, $"Match {seed}");
                CollectionAssert.IsEmpty(probes.SelectMany(probe => probe.Violations), $"Match {seed}");
                Assert.Greater(probes.Sum(probe => probe.Submitted), 0);
            }
        }

        [Test]
        public void Simulation_NoHungOrFailedMatches()
        {
            var cards = new FirstCardSet();
            var options = new SimulationOptions
            {
                Games = 60,
                Seed = 42,
                Player1Profile = cards.Normal,
                Player2Profile = cards.Easy,
                AlternateSeats = true,
            };

            var records = new SimulationRunner(cards.Catalog, cards.Rules).Run(options);

            Assert.AreEqual(60, records.Count);
            CollectionAssert.IsEmpty(records.Where(record => !record.Completed).Select(record => $"#{record.Index} seed {record.Seed}: {record.Error ?? "hung"}"));
        }

        [Test]
        public void SameSeed_SameMatch()
        {
            var cards = new FirstCardSet();
            var options = new SimulationOptions { Player1Profile = cards.Normal, Player2Profile = cards.Normal };
            var runner = new SimulationRunner(cards.Catalog, cards.Rules);
            var csv = new CsvReportWriter();

            var first = csv.Write(new[] { runner.RunGame(options, 3, 123456789) });
            var second = csv.Write(new[] { runner.RunGame(options, 3, 123456789) });

            Assert.AreEqual(first, second);
        }

        /// <summary>
        /// Two matches with the same public history: Player2 places the same creatures in the same way but holds
        /// other abilities and owns other field cards. Player1's AI must decide the same way in both until
        /// something hidden is revealed and the views differ.
        /// </summary>
        [Test]
        public void Decisions_DoNotDependOnHiddenCards()
        {
            var cards = new FirstCardSet();
            var player1Deck = new RandomDeckBuilder(cards.Catalog, cards.Rules.Deck).Build(new XorShiftRandom(5));
            var (deckA, deckB) = TwoDecksWithSameCreatures(cards);
            var a = new ScriptedMatch(cards, player1Deck, deckA);
            var b = new ScriptedMatch(cards, player1Deck, deckB);

            var compared = 0;
            for (var step = 0; step < 300 && !a.Ended && !b.Ended; step++)
            {
                if (a.Describe(P1) != b.Describe(P1))
                    break;

                var decisionA = a.Step();
                var decisionB = b.Step();
                Assert.AreEqual(decisionA, decisionB, $"Step {step}: Player1's AI decided differently.");
                if (decisionA != null)
                    compared++;
            }

            Assert.GreaterOrEqual(compared, 5, "Too few decisions compared before the views diverged.");
        }

        private static (DeckList, DeckList) TwoDecksWithSameCreatures(FirstCardSet cards)
        {
            var builder = new RandomDeckBuilder(cards.Catalog, cards.Rules.Deck);
            var validator = new DeckValidator(cards.Catalog, cards.Rules.Deck);
            var first = builder.Build(new XorShiftRandom(100)).CardIds;
            for (ulong seed = 101; seed < 1000; seed++)
            {
                var other = builder.Build(new XorShiftRandom(seed)).CardIds;
                // Fields 0-2, creatures 3-5, abilities 6-8: keep the creatures in their places, so instance ids match.
                var mixed = other.Take(3).Concat(first.Skip(3).Take(3)).Concat(other.Skip(6)).ToList();
                var differs = Enumerable.Range(0, 9).Count(i => (i < 3 || i > 5) && mixed[i] != first[i]) >= 4;
                if (differs && validator.Validate(new DeckList(mixed)).IsValid)
                    return (new DeckList(first), new DeckList(mixed));
            }

            Assert.Fail("No second deck found.");
            return default;
        }

        /// <summary>Player1 is the AI; Player2 follows a fixed script that never reveals its hidden cards.</summary>
        private sealed class ScriptedMatch
        {
            private readonly MatchEngine _engine;
            private readonly AiBrain _brain;

            public ScriptedMatch(FirstCardSet cards, DeckList player1Deck, DeckList player2Deck)
            {
                _engine = new MatchEngine(new MatchSetup(player1Deck, player2Deck), cards.Catalog, cards.Rules, new XorShiftRandom(9));
                _brain = new AiBrain(P1, cards.Normal, cards.Catalog, cards.Rules, new XorShiftRandom(77));
                Observe(_engine.Start());
            }

            public bool Ended => _engine.State.Phase == MatchPhase.Ended;

            /// <summary>Makes one move. Returns Player1's decision as text, or null if Player2 moved.</summary>
            public string Step()
            {
                var view = _engine.GetView(P1);
                if (view.AwaitingPlayer == P1)
                {
                    var legal = _engine.GetLegalActions(P1);
                    var command = _brain.Decide(view, legal);
                    Execute(command);
                    return $"{legal.ToList().IndexOf(command)}/{legal.Count} {command.GetType().Name}";
                }

                Execute(Player2Move(_engine.GetLegalActions(P2)));
                return null;
            }

            public string Describe(PlayerId viewer)
            {
                var view = _engine.GetView(viewer);
                var fields = view.Fields.Select(field =>
                    $"{field.Id}:{field.CardId}:{field.IsOpen}:{field.IsRemoved}:{field.IsLocked}:{field.Occupant(P1)}:{field.Occupant(P2)}");
                var battle = view.Battle == null ? "-" : $"{view.Battle.CurrentActor}{view.Battle.IsReady(P1)}{view.Battle.IsReady(P2)}" +
                                                         string.Join(",", view.Battle.PlayedAbilities.Select(played => played.Card));
                return string.Join("|", new[]
                    {
                        view.Phase.ToString(), view.AwaitingPlayer.ToString(), string.Join(",", view.Hand), view.OpponentHandCount.ToString(),
                        string.Join(",", view.Discard(P2)), string.Join(",", view.OutOfGame(P2)), battle,
                    }
                    .Concat(fields));
            }

            /// <summary>Order chosen by the coin winner going second, the first placement, else a challenge, else ready or end turn.</summary>
            private static GameCommand Player2Move(IReadOnlyList<GameCommand> legal) =>
                legal.OfType<ChooseTurnOrder>().FirstOrDefault(order => !order.GoFirst)
                ?? legal.OfType<PlaceCreature>().FirstOrDefault(place => place.SignatureAbilityId == null)
                ?? legal.OfType<Challenge>().FirstOrDefault(challenge => challenge.SignatureAbilityId == null)
                ?? legal.OfType<Ready>().FirstOrDefault()
                ?? legal.OfType<EndTurn>().FirstOrDefault()
                ?? legal.First();

            private void Execute(GameCommand command)
            {
                var result = _engine.Execute(command);
                Assert.IsTrue(result.IsAccepted, $"{command.GetType().Name} rejected: {result.Reason}");
                Observe(result.Events);
            }

            private void Observe(IEnumerable<GameEvent> events) =>
                _brain.Observe(events.Where(gameEvent => gameEvent.IsVisibleTo(P1)).ToList());
        }
    }
}
