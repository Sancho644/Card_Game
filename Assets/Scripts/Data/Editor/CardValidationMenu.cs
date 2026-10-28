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
            var problems = 0;
            foreach (var database in LoadAll<CardDatabase>())
            {
                foreach (var problem in database.Validate())
                {
                    Debug.LogError($"[{database.name}] {problem}", database);
                    problems++;
                }

                foreach (var variant in LoadAll<BalanceVariantDef>())
                {
                    foreach (var problem in variant.Validate(database))
                    {
                        Debug.LogError($"[{variant.name}] {problem}", variant);
                        problems++;
                    }
                }

                foreach (var balance in LoadAll<BalanceConfig>())
                {
                    foreach (var warning in new CardBalanceValidator(balance).Check(database.Cards))
                    {
                        Debug.LogWarning($"[{balance.name}] {warning}", balance);
                        problems++;
                    }
                }

                foreach (var rules in LoadAll<GameRulesConfig>())
                {
                    var catalog = database.ToCatalog();
                    var validator = new DeckValidator(catalog, rules.ToRules().Deck);
                    foreach (var deck in LoadAll<DeckDef>())
                    {
                        foreach (var error in validator.Validate(deck.ToDeckList()).Errors)
                        {
                            Debug.LogError($"[{deck.name}] {error}", deck);
                            problems++;
                        }
                    }
                }
            }

            if (problems == 0)
                Debug.Log("Cards, balance and decks: no problems found.");
        }

        private static T[] LoadAll<T>() where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToArray();
    }
}
