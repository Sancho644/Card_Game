using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game.Core.Events;
using UnityEditor;
using UnityEngine;

namespace Game.Simulation.Editor
{
    /// <summary>
    /// Editor menu for AI-versus-AI simulations. Parameters come from the SimulationConfig asset; reports go
    /// to SimulationReports/ in the project root, outside Assets.
    /// </summary>
    internal static class SimulationMenu
    {
        private const string ReportFolder = "SimulationReports";

        [MenuItem("Game/Simulation/Run")]
        private static void Run()
        {
            var assets = Load();
            if (assets == null)
                return;

            var runner = new SimulationRunner(assets.Catalog, assets.Rules);
            var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var writer = new TextReportWriter(assets.CardName);
            var report = new StringBuilder();
            try
            {
                foreach (var options in assets.MainRuns())
                {
                    var main = RunWithProgress(runner, options);
                    if (main == null)
                        return;
                    WriteFile($"sim-{stamp}-games-{options.DeckMode.ToString().ToLowerInvariant()}.csv", new CsvReportWriter().Write(main.Records));
                    report.Append(writer.WriteMain(main));
                }

                if (assets.Config.StrengthGames > 0)
                {
                    var strength = RunWithProgress(runner, assets.StrengthRun());
                    if (strength == null)
                        return;
                    WriteFile($"sim-{stamp}-strength.csv", new CsvReportWriter().Write(strength.Records));
                    report.Append(writer.WriteStrength(strength));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var path = WriteFile($"sim-{stamp}-report.txt", report.ToString());
            Debug.Log($"Simulation report: {path}\n{report}");
        }

        [MenuItem("Game/Simulation/Replay")]
        private static void Replay()
        {
            var assets = Load();
            if (assets == null)
                return;

            var config = assets.Config;
            var options = config.ReplayStrengthRun ? assets.StrengthRun() : assets.MainRun(config.ReplaySynergyDecks ? SimulationDeckMode.Synergy : SimulationDeckMode.Random);
            var log = new StringBuilder();
            var record = new SimulationRunner(assets.Catalog, assets.Rules)
                .RunGame(options, config.ReplayIndex, config.ReplaySeed, batch => Dump(batch, log));

            var summary = $"Match #{record.Index}, seed {record.Seed}: ended {record.Ended}, hung {record.Hung}, " +
                          $"winner {record.Winner?.ToString() ?? "-"}, turns {record.Turns}, error {record.Error ?? "-"}";
            var path = WriteFile($"replay-{record.Seed}.txt", summary + "\n\n" + log);
            Debug.Log($"{summary}\nEvent log: {path}");
        }

        private static SimulationAssets Load()
        {
            var assets = SimulationAssets.Load(out var missing);
            if (assets == null)
                Debug.LogError($"Simulation: no {missing} asset in the project.");
            return assets;
        }

        /// <summary>Null if cancelled.</summary>
        private static SimulationSummary RunWithProgress(SimulationRunner runner, SimulationOptions options)
        {
            var cancelled = false;
            var records = runner.Run(options, (done, total) =>
            {
                cancelled = EditorUtility.DisplayCancelableProgressBar(options.Name, $"{done} / {total}", (float)done / total);
                return !cancelled;
            });
            if (cancelled)
            {
                Debug.LogWarning($"Simulation cancelled after {records.Count} matches.");
                return null;
            }

            return new SimulationSummary(options, records);
        }

        private static void Dump(IReadOnlyList<GameEvent> batch, StringBuilder log)
        {
            foreach (var gameEvent in batch)
            {
                var visibility = gameEvent.VisibleOnlyTo == null ? "" : $" [only {gameEvent.VisibleOnlyTo}]";
                var fields = gameEvent.GetType().GetProperties()
                    .Where(property => property.Name != nameof(GameEvent.VisibleOnlyTo))
                    .Select(property => $"{property.Name}={Format(property.GetValue(gameEvent))}");
                log.AppendLine($"{gameEvent.GetType().Name}{visibility}: {string.Join(", ", fields)}");
            }
        }

        private static string Format(object value) =>
            value is System.Collections.IEnumerable list && !(value is string)
                ? "[" + string.Join(" ", list.Cast<object>()) + "]"
                : value?.ToString() ?? "-";

        private static string WriteFile(string name, string content)
        {
            var folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ReportFolder);
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, name);
            File.WriteAllText(path, content, new UTF8Encoding(true));
            return path;
        }
    }
}
