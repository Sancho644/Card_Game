using Game.Core.Cards;
using Game.Core.Rules;
using Game.Data.Cards;
using Game.Data.Config;
using UnityEditor;

namespace Game.AI.Tests
{
    /// <summary>The real first card set and rules from the project assets: the AI is tested on the cards it will play.</summary>
    internal sealed class FirstCardSet
    {
        public FirstCardSet()
        {
            Catalog = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset").ToCatalog();
            Rules = AssetDatabase.LoadAssetAtPath<GameRulesConfig>("Assets/Data/Config/GameRules.asset").ToRules();
            var ai = AssetDatabase.LoadAssetAtPath<AiConfig>("Assets/Data/Config/AiConfig.asset");
            Normal = ai.ToProfile("normal");
            Easy = ai.ToProfile("easy");
        }

        public CardCatalog Catalog { get; }
        public GameRules Rules { get; }
        public AiProfile Normal { get; }
        public AiProfile Easy { get; }
    }
}
