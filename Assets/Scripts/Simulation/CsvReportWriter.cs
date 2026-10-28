using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Game.Core.Battle;
using Game.Core.State;

namespace Game.Simulation
{
    /// <summary>One CSV row per match: seed, decks, profiles, result and counters.</summary>
    public sealed class CsvReportWriter
    {
        private const string Header =
            "index,seed,status,profile_p1,profile_p2,deck_p1,deck_p2,first,winner,end_reason,turns,battles,battle_draws,equal_power,retreats,field_openings,skips,challenges,error";

        public string Write(IReadOnlyList<GameRecord> records)
        {
            var text = new StringBuilder().AppendLine(Header);
            foreach (var record in records)
            {
                text.AppendLine(string.Join(",", new[]
                {
                    record.Index.ToString(CultureInfo.InvariantCulture),
                    record.Seed.ToString(CultureInfo.InvariantCulture),
                    Status(record),
                    record.ProfileOf(PlayerId.Player1),
                    record.ProfileOf(PlayerId.Player2),
                    Quote(string.Join(" ", record.DeckOf(PlayerId.Player1).CardIds)),
                    Quote(string.Join(" ", record.DeckOf(PlayerId.Player2).CardIds)),
                    record.FirstPlayer?.ToString() ?? "",
                    record.Winner?.ToString() ?? (record.Completed ? "draw" : ""),
                    record.Ended ? record.EndReason.ToString() : "",
                    record.Turns.ToString(CultureInfo.InvariantCulture),
                    record.Battles.Count.ToString(CultureInfo.InvariantCulture),
                    record.Battles.Count(battle => battle.IsDraw).ToString(CultureInfo.InvariantCulture),
                    record.Battles.Count(battle => battle.TieBreak != TieBreak.None).ToString(CultureInfo.InvariantCulture),
                    record.Battles.Count(battle => battle.Retreated).ToString(CultureInfo.InvariantCulture),
                    record.FieldOpenings.ToString(CultureInfo.InvariantCulture),
                    record.Skips.ToString(CultureInfo.InvariantCulture),
                    record.Challenges.ToString(CultureInfo.InvariantCulture),
                    Quote(record.Error ?? ""),
                }));
            }

            return text.ToString();
        }

        private static string Status(GameRecord record) =>
            record.Error != null ? "error" : record.Hung ? "hung" : "ok";

        private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
