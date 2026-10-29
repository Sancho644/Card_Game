using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Simulation
{
    /// <summary>Aggregate numbers of a simulation, over the completed matches.</summary>
    public sealed class SimulationSummary
    {
        public SimulationSummary(SimulationOptions options, IReadOnlyList<GameRecord> records)
        {
            Options = options;
            Records = records;
            Completed = records.Where(record => record.Completed).ToList();
            Battles = Completed.SelectMany(record => record.Battles).ToList();
            Cards = BuildCardStats();
        }

        public SimulationOptions Options { get; }
        public IReadOnlyList<GameRecord> Records { get; }
        public IReadOnlyList<GameRecord> Completed { get; }
        public IReadOnlyList<BattleRecord> Battles { get; }
        public IReadOnlyList<CardStats> Cards { get; }

        public IEnumerable<GameRecord> Hung => Records.Where(record => record.Hung);
        public IEnumerable<GameRecord> Failed => Records.Where(record => record.Error != null);

        /// <summary>Completed matches with a winner; win shares are counted among these.</summary>
        public IReadOnlyList<GameRecord> DecidedMatches => Completed.Where(record => record.Winner != null).ToList();

        /// <summary>Matches won by the player who went first, among matches with a winner.</summary>
        public ShareEstimate FirstPlayerWins => Estimate(DecidedMatches, record => record.Winner == record.FirstPlayer);

        /// <summary>Drawn matches, as a share of completed matches.</summary>
        public double MatchDraws => Share(Completed, record => record.Winner == null);

        /// <summary>Completed matches decided by the creatures left on equal wins (docs/rules.md §9).</summary>
        public int MoreCreaturesWins => Completed.Count(record => record.EndReason == MatchEndReason.MoreCreatures);

        public double AverageTurns => Average(Completed, record => record.Turns);
        public double AverageBattles => Average(Completed, record => record.Battles.Count);

        public double BattleDraws => Share(Battles, battle => battle.IsDraw);
        public double Retreats => Share(Battles, battle => battle.Retreated);

        /// <summary>Battles that ended with the field open.</summary>
        public double OpenFieldBattles => Share(Battles, battle => battle.FieldOpen);

        public double OpeningsPerMatch => Average(Completed, record => record.FieldOpenings);
        public double SkipsPerTurn => Ratio(Completed.Sum(record => record.Skips), Completed.Sum(record => record.Turns));
        public double ChallengesPerTurn => Ratio(Completed.Sum(record => record.Challenges), Completed.Sum(record => record.Turns));

        /// <summary>Share of decided battles (with a winner) the host won, on any field.</summary>
        public ShareEstimate AllHostWins =>
            Estimate(Battles.Where(battle => battle.Winner != null).ToList(), battle => battle.Winner == battle.Host);

        /// <summary>Share of decided battles (with a winner) the host won, on an open or a closed field.</summary>
        public double HostWins(bool open) => Share(Decided(open), battle => battle.Winner == battle.Host);

        public int DecidedBattles(bool open) => Decided(open).Count();

        /// <summary>Share of battles with a winner won by the player who acted first in the battle.</summary>
        public double FirstActorWins =>
            Share(Battles.Where(battle => battle.Winner != null), battle => battle.Winner == battle.FirstActor);

        /// <summary>Battles won on equal final power, by how the tie was decided.</summary>
        public int EqualPowerBattles(TieBreak how) => Battles.Count(battle => battle.TieBreak == how);

        /// <summary>Wins of a profile, when the profiles differ: share of matches with a winner.</summary>
        public ShareEstimate ProfileWins(string profileId) =>
            Estimate(DecidedMatches, record => record.ProfileOf(record.Winner.Value) == profileId);

        /// <summary>Battle win share of creatures of each element, over battles that were not retreated.</summary>
        public IEnumerable<(Element element, ShareEstimate wins)> ElementWins() =>
            Sides()
                .GroupBy(side => side.battle.ElementOf(side.player))
                .OrderBy(group => group.Key)
                .Select(group => (group.Key, new ShareEstimate(group.Count(side => side.won), group.Count())));

        /// <summary>Battle win share of each creature, over battles that were not retreated.</summary>
        public IEnumerable<(string creatureId, Element element, ShareEstimate wins)> CreatureWins() =>
            Sides()
                .GroupBy(side => (side.battle.CreatureOf(side.player), side.battle.ElementOf(side.player)))
                .Select(group => (group.Key.Item1, group.Key.Item2, new ShareEstimate(group.Count(side => side.won), group.Count())));

        /// <summary>Both sides of every battle that was not retreated; a draw counts as not won.</summary>
        private IEnumerable<(BattleRecord battle, PlayerId player, bool won)> Sides() =>
            Battles.Where(battle => !battle.Retreated)
                .SelectMany(battle => PlayerIds.All.Select(side => (battle, side, battle.Winner == side)));

        private IEnumerable<BattleRecord> Decided(bool open) =>
            Battles.Where(battle => battle.Winner != null && battle.FieldOpen == open);

        private List<CardStats> BuildCardStats()
        {
            var cards = new Dictionary<string, CardStats>();
            foreach (var record in Completed)
            {
                foreach (var player in PlayerIds.All)
                {
                    foreach (var cardId in record.DeckOf(player).CardIds.Distinct())
                    {
                        if (!cards.TryGetValue(cardId, out var stats))
                            cards[cardId] = stats = new CardStats(cardId);
                        var uses = record.UsesOf(player, cardId);
                        stats.DeckGames++;
                        stats.TotalUses += uses;
                        if (uses > 0)
                            stats.GamesUsed++;
                        if (record.Winner != null)
                            stats.DecidedDeckGames++;
                        if (record.Winner == player)
                            stats.DeckWins++;
                    }
                }
            }

            return cards.Values.OrderBy(stats => stats.CardId).ToList();
        }

        private static ShareEstimate Estimate<T>(IReadOnlyCollection<T> items, System.Func<T, bool> predicate) =>
            new ShareEstimate(items.Count(predicate), items.Count);

        private static double Share<T>(IEnumerable<T> items, System.Func<T, bool> predicate)
        {
            var list = items as IReadOnlyCollection<T> ?? items.ToList();
            return list.Count == 0 ? 0 : (double)list.Count(predicate) / list.Count;
        }

        private static double Average<T>(IReadOnlyCollection<T> items, System.Func<T, int> value) =>
            items.Count == 0 ? 0 : items.Average(value);

        private static double Ratio(int part, int whole) => whole == 0 ? 0 : (double)part / whole;
    }
}
