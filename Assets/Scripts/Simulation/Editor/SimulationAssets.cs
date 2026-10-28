using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Rules;
using Game.Data.Cards;
using Game.Data.Config;
using UnityEditor;

namespace Game.Simulation.Editor
{
    /// <summary>The project assets a simulation needs, found by type, and the options built from them.</summary>
    internal sealed class SimulationAssets
    {
        private SimulationAssets(SimulationConfig config, CardDatabase database, GameRulesConfig rules, AiConfig ai)
        {
            Config = config;
            Database = database;
            RulesConfig = rules;
            Ai = ai;
            Catalog = database.ToCatalog();
            Rules = rules.ToRules();
        }

        public SimulationConfig Config { get; }
        public CardDatabase Database { get; }
        public GameRulesConfig RulesConfig { get; }
        public AiConfig Ai { get; }
        public CardCatalog Catalog { get; }
        public GameRules Rules { get; }

        /// <summary>Null if an asset is missing; <paramref name="missing"/> names it.</summary>
        public static SimulationAssets Load(out string missing)
        {
            var config = Find<SimulationConfig>();
            var database = Find<CardDatabase>();
            var rules = Find<GameRulesConfig>();
            var ai = Find<AiConfig>();
            missing = config == null ? nameof(SimulationConfig)
                : database == null ? nameof(CardDatabase)
                : rules == null ? nameof(GameRulesConfig)
                : ai == null ? nameof(AiConfig)
                : null;
            return missing == null ? new SimulationAssets(config, database, rules, ai) : null;
        }

        /// <summary>The main runs: one with the fixed decks, or one per enabled deck mode.</summary>
        public IReadOnlyList<SimulationOptions> MainRuns()
        {
            if (Decks().Count > 0)
                return new[] { MainRun(SimulationDeckMode.Random) };

            var runs = new List<SimulationOptions>();
            if (Config.RandomDecksRun)
                runs.Add(MainRun(SimulationDeckMode.Random));
            if (Config.SynergyDecksRun)
                runs.Add(MainRun(SimulationDeckMode.Synergy));
            return runs;
        }

        public SimulationOptions MainRun(SimulationDeckMode mode) => new SimulationOptions
        {
            Name = $"Основной прогон: ИИ «{ProfileName(Config.Player1ProfileId)}» против ИИ «{ProfileName(Config.Player2ProfileId)}»" +
                   (Decks().Count == 0 ? $", колоды {DeckModeName(mode)}" : ""),
            Games = Config.Games,
            Seed = Config.Seed,
            Decks = Decks(),
            DeckMode = mode,
            SynergyRandomShare = Config.SynergyRandomShare,
            Player1Profile = Ai.ToProfile(Config.Player1ProfileId),
            Player2Profile = Ai.ToProfile(Config.Player2ProfileId),
            TurnLimit = Config.TurnLimit,
        };

        /// <summary>The stronger profile against the weaker one, seats alternating, with its own seed.</summary>
        public SimulationOptions StrengthRun() => new SimulationOptions
        {
            Name = $"Проверка силы: ИИ «{ProfileName(Config.StrongProfileId)}» против ИИ «{ProfileName(Config.WeakProfileId)}»",
            Games = Config.StrengthGames,
            Seed = Config.Seed + 1,
            Decks = Decks(),
            Player1Profile = Ai.ToProfile(Config.StrongProfileId),
            Player2Profile = Ai.ToProfile(Config.WeakProfileId),
            AlternateSeats = true,
            TurnLimit = Config.TurnLimit,
        };

        public string CardName(string id) => Database.Find(id)?.DisplayName ?? id;

        public static string DeckModeName(SimulationDeckMode mode) => mode == SimulationDeckMode.Synergy ? "с синергией" : "случайные";

        private IReadOnlyList<DeckList> Decks() => Config.Decks.Where(deck => deck != null).Select(deck => deck.ToDeckList()).ToList();

        private string ProfileName(string id) => Ai.Profiles.FirstOrDefault(profile => profile.Id == id)?.DisplayName ?? id;

        private static T Find<T>() where T : UnityEngine.Object
        {
            var guid = AssetDatabase.FindAssets("t:" + typeof(T).Name).FirstOrDefault();
            return guid == null ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
