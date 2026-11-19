using System.Collections.Generic;
using System.Linq;
using Game.Core.Decks;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using UnityEditor;
using UnityEngine;

namespace Game.Data.Editor
{
    /// <summary>Checks every card database, balance config and deck in the project and logs what it finds.</summary>
    public static class CardValidationMenu
    {
        [MenuItem("Tools/Game/Validate Cards")]
        public static void ValidateAll()
        {
            var problems = FindProblems().ToArray();
            foreach (var problem in problems)
                Debug.LogError(problem);
            if (problems.Length == 0)
                Debug.Log("Cards, balance and decks: no problems found.");
        }

        public static IEnumerable<string> FindProblems()
        {
            var databases = LoadAll<CardDatabase>();
            var rules = LoadAll<GameRulesConfig>();
            var decks = LoadAll<DeckDef>();
            var variants = LoadAll<BalanceVariantDef>();
            var balances = LoadAll<BalanceConfig>();

            if (databases.Length == 0) yield return "No card database found.";
            if (rules.Length == 0) yield return "No game rules config found.";
            if (decks.Length == 0) yield return "No deck definitions found.";

            foreach (var database in databases)
            {
                var problems = database.Validate().ToArray();
                foreach (var problem in problems)
                    yield return $"[{database.name}] {problem}";
                if (database.Cards.Count == 0)
                {
                    yield return $"[{database.name}] the database has no cards";
                    continue;
                }

                // Converting a catalog with duplicate IDs or missing references may throw.
                if (problems.Length != 0) continue;

                foreach (var variant in variants)
                    foreach (var problem in variant.Validate(database))
                        yield return $"[{database.name} / {variant.name}] {problem}";

                foreach (var balance in balances)
                    foreach (var warning in new CardBalanceValidator(balance).Check(database.Cards))
                        yield return $"[{database.name} / {balance.name}] {warning}";

                var catalog = database.ToCatalog();
                foreach (var config in rules)
                {
                    var validator = new DeckValidator(catalog, config.ToRules().Deck);
                    foreach (var deck in decks)
                        foreach (var error in validator.Validate(deck.ToDeckList()).Errors)
                            yield return $"[{database.name} / {config.name} / {deck.name}] {error}";
                }
            }
        }

        private static T[] LoadAll<T>() where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToArray();
    }
}
