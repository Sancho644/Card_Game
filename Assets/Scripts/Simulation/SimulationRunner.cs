using System;
using System.Collections.Generic;
using System.Linq;
using Game.AI;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;
using Game.Services.Match;

namespace Game.Simulation
{
    /// <summary>
    /// Plays AI-versus-AI matches without a scene: the same session and controller hosts as the game,
    /// with a scheduler that runs every decision at once. Every match is reproducible from its seed.
    /// </summary>
    public sealed class SimulationRunner
    {
        private readonly CardCatalog _catalog;
        private readonly GameRules _rules;
        private readonly RandomDeckBuilder _randomDecks;

        public SimulationRunner(CardCatalog catalog, GameRules rules)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _randomDecks = new RandomDeckBuilder(catalog, rules.Deck);
        }

        /// <param name="progress">Called after every match with (done, total); returning false stops the run.</param>
        public IReadOnlyList<GameRecord> Run(SimulationOptions options, Func<int, int, bool> progress = null)
        {
            var master = new XorShiftRandom(options.Seed);
            var records = new List<GameRecord>();
            for (var i = 0; i < options.Games; i++)
            {
                records.Add(RunGame(options, i, NextSeed(master)));
                if (progress != null && !progress(i + 1, options.Games))
                    break;
            }

            return records;
        }

        /// <summary>One match; the same options, index and seed always give the same match.</summary>
        /// <param name="onEvents">Receives every event batch, e.g. to dump a replayed match.</param>
        public GameRecord RunGame(SimulationOptions options, int index, ulong seed, Action<IReadOnlyList<GameEvent>> onEvents = null)
        {
            var random = new XorShiftRandom(seed);
            var swap = options.AlternateSeats && index % 2 == 1;
            var profiles = swap
                ? new[] { options.Player2Profile, options.Player1Profile }
                : new[] { options.Player1Profile, options.Player2Profile };
            var decks = new[] { PickDeck(options, random), PickDeck(options, random) };
            var engineSeed = NextSeed(random);

            var record = new GameRecord(index, seed, decks[0], decks[1], profiles[0].Id, profiles[1].Id);
            var session = new LocalMatchSession(_catalog, _rules);
            var scheduler = new ManualScheduler();
            var collector = new MatchStatsCollector(session, _catalog, record);
            if (onEvents != null)
                session.EventsProduced += onEvents;
            var hosts = PlayerIds.All
                .Select(id => CreateHost(session, scheduler, record, id, profiles[(int)id], NextSeed(random), NextSeed(random)))
                .ToList();
            try
            {
                session.Start(new MatchSetup(decks[0], decks[1]), engineSeed);
                Play(options, scheduler, record);
            }
            catch (Exception exception)
            {
                record.Error = exception.GetType().Name + ": " + exception.Message;
            }
            finally
            {
                foreach (var host in hosts)
                    host.Dispose();
                collector.Dispose();
            }

            return record;
        }

        private static void Play(SimulationOptions options, ManualScheduler scheduler, GameRecord record)
        {
            while (!record.Ended && record.Error == null)
            {
                if (record.Turns > options.TurnLimit || !scheduler.HasPending)
                {
                    record.Hung = true;
                    return;
                }

                scheduler.RunPending(1);
            }
        }

        private SessionPlayerHost CreateHost(
            IMatchSession session, ManualScheduler scheduler, GameRecord record, PlayerId player, AiProfile profile, ulong aiSeed, ulong coinSeed)
        {
            var brain = new AiBrain(player, profile, _catalog, _rules, new XorShiftRandom(aiSeed));
            var ai = new AiController(brain, profile, scheduler, new XorShiftRandom(aiSeed ^ 0x5DEECE66DUL));
            var host = new SessionPlayerHost(session, new SimulationSeat(ai, new XorShiftRandom(coinSeed), record), scheduler, offersReactions: true);
            host.CommandRejected += (command, reason) =>
                record.Error = record.Error ?? $"{player} {command.GetType().Name} rejected: {reason}";
            return host;
        }

        private DeckList PickDeck(SimulationOptions options, IRandom random)
        {
            if (options.Decks.Count > 0)
                return options.Decks[random.Next(options.Decks.Count)];
            return options.DeckMode == SimulationDeckMode.Synergy
                ? new SynergyDeckBuilder(_catalog, _rules.Deck, options.SynergyRandomShare).Build(random)
                : _randomDecks.Build(random);
        }

        private static ulong NextSeed(IRandom random) =>
            ((ulong)(uint)random.Next(int.MaxValue) << 31) ^ (uint)random.Next(int.MaxValue);
    }
}
