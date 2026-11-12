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

        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, false, true)]
        public void FieldBlessing_WithoutCurrentBonus_IsKeptEvenWithDifficultyNoise(bool open, bool bonus, bool easy)
        {
            var cards = new FirstCardSet();
            var engine = FieldBlessingBattle(cards, open, bonus);
            var blessing = engine.State.Player(P1).Hand.First(c => c.Definition.Id == "ability.field-blessing").Id;
            var actions = engine.GetLegalActions(P1).Where(c => c is Ready || c is OpenField
                || c is PlayAbility play && play.AbilityId == blessing).ToList();
            Assert.IsTrue(actions.OfType<PlayAbility>().Any(), "The card remains legal for a human to play.");
            Assert.AreEqual(engine.GetBattlePower().Of(P1).FinalPower,
                engine.PreviewPowerWithAbility(P1, blessing).Of(P1).FinalPower);
            for (ulong seed = 1; seed <= 20; seed++)
            {
                var brain = new AiBrain(P1, easy ? cards.Easy : cards.Normal, cards.Catalog, cards.Rules, new XorShiftRandom(seed));
                Assert.IsFalse(brain.Decide(engine.GetView(P1), actions) is PlayAbility,
                    $"The AI wasted a zero-effect blessing, seed {seed}.");
            }
        }

        [Test]
        public void FieldBlessing_WithUsefulOpenFieldBonus_IsStillPlayed()
        {
            var cards = new FirstCardSet();
            var engine = FieldBlessingBattle(cards, open: true, bonus: true);
            var blessing = engine.State.Player(P1).Hand.First(c => c.Definition.Id == "ability.field-blessing").Id;
            var actions = engine.GetLegalActions(P1).Where(c => c is Ready
                || c is PlayAbility play && play.AbilityId == blessing).ToList();
            var profile = cards.Easy;
            profile.Noise = 0;
            var brain = new AiBrain(P1, profile, cards.Catalog, cards.Rules, new XorShiftRandom(5));
            var chosen = brain.Decide(engine.GetView(P1), actions) as PlayAbility;
            Assert.IsNotNull(chosen);
            Assert.AreEqual(blessing, chosen.AbilityId);
            Assert.IsTrue(engine.Execute(chosen).IsAccepted);
            Assert.AreEqual(150 + 40 + 50, engine.GetBattlePower().Of(P1).FinalPower);
        }

        private static MatchEngine FieldBlessingBattle(FirstCardSet cards, bool open, bool bonus)
        {
            var deck = new DeckList(new[]
            {
                "field.warm-hearth", "field.forge", "field.eternal-beacon",
                "creature.spark-fox", "creature.whirlwind-griffin", "creature.lord-of-crimson-forge",
                "ability.field-blessing", "ability.intercept", "ability.return-of-power",
            });
            var engine = new MatchEngine(new MatchSetup(deck, deck), cards.Catalog, cards.Rules, new XorShiftRandom(3));
            engine.Start();
            void Do(GameCommand command) => Assert.IsTrue(engine.Execute(command).IsAccepted, command.GetType().Name);
            foreach (var player in PlayerIds.All)
                Do(new ChooseCard(player, engine.State.Fields.First(f => f.Owner == player && f.Definition.Id == "field.warm-hearth").Id));
            Do(new ChooseTurnOrder(engine.State.CoinTossWinner.Value, engine.State.CoinTossWinner == P1));
            int Card(PlayerId player, string id) => engine.State.Player(player).Hand.First(c => c.Definition.Id == id).Id;
            var field = engine.State.Fields.First(f => f.Owner == P1 && f.Definition.Id == "field.warm-hearth").Id;
            Do(new PlaceCreature(P1, Card(P1, bonus ? "creature.spark-fox" : "creature.whirlwind-griffin"), field));
            Do(new EndTurn(P1));
            Do(new PlaceCreature(P2, Card(P2, bonus ? "creature.whirlwind-griffin" : "creature.spark-fox"), field));
            if (open) Do(new OpenField(P1));
            return engine;
        }

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

        /// <summary>
        /// «Возврат силы» after a battle on the AI's own turn, with a spent ability in the discard pile:
        /// the engine offers it and the AI plays it instead of ending the turn.
        /// </summary>
        [Test]
        public void ReturnOfPower_AfterBattleOnOwnTurn_OfferedAndPlayed()
        {
            var cards = new FirstCardSet();
            var deck = new DeckList(new[]
            {
                "field.warm-hearth", "field.forge", "field.eternal-beacon",
                "creature.spark-fox", "creature.ash-phoenix", "creature.lord-of-crimson-forge",
                "ability.surge", "ability.intercept", "ability.return-of-power",
            });
            var engine = new MatchEngine(new MatchSetup(deck, deck), cards.Catalog, cards.Rules, new XorShiftRandom(3));
            engine.Start();
            foreach (var player in PlayerIds.All)
            {
                var selected = engine.State.Fields.FirstOrDefault(f => f.Owner == player && f.Definition.Id == "field.warm-hearth")
                    ?? engine.State.Fields.First(f => f.Owner == player);
                Assert.IsTrue(engine.Execute(new ChooseCard(player, selected.Id)).IsAccepted);
            }
            int Card(PlayerId owner, string cardId) =>
                engine.State.Player(owner).Hand.Concat(engine.State.Player(owner).Discard).First(card => card.Definition.Id == cardId).Id;
            void Do(GameCommand command) =>
                Assert.IsTrue(engine.Execute(command).IsAccepted, $"{command.GetType().Name} by {command.Player} rejected");

            var coinWinner = engine.State.CoinTossWinner.Value;
            Do(new ChooseTurnOrder(coinWinner, goFirst: coinWinner == P2));
            var field = engine.State.Fields.First(slot => slot.Owner == P1 && slot.Definition.Id == "field.warm-hearth").Id;
            Do(new PlaceCreature(P2, Card(P2, "creature.spark-fox"), field));
            Do(new EndTurn(P2));
            Do(new PlaceCreature(P1, Card(P1, "creature.ash-phoenix"), field));
            Do(new Ready(P2));
            Do(new PlayAbility(P1, Card(P1, "ability.surge")));
            Do(new Ready(P2));
            Do(new Ready(P1));
            Assert.AreEqual(MatchPhase.Turn, engine.State.Phase);
            Assert.AreEqual(P1, engine.State.ActivePlayer);

            var legal = engine.GetLegalActions(P1);
            var returnPower = Card(P1, "ability.return-of-power");
            var offered = legal.OfType<PlayAbility>().Where(play => play.AbilityId == returnPower).ToList();
            Assert.AreEqual(1, offered.Count, "«Возврат силы» should be offered once, aimed at «Прилив сил».");
            Assert.AreEqual(Card(P1, "ability.surge"), offered[0].Targets.CardId);

            var brain = new AiBrain(P1, cards.Normal, cards.Catalog, cards.Rules, new XorShiftRandom(5));
            Assert.AreSame(offered[0], brain.Decide(engine.GetView(P1), legal));
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
