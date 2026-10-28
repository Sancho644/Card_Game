using System;
using System.Collections.Generic;
using Game.Core.Cards;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using Game.Data.Presentation;
using Game.Services.Matchmaking;

namespace Game.Presentation.App
{
    /// <summary>
    /// Services and data that live for the whole application, owned by the application root and handed to each
    /// scene. Later also the network connection and the player profile.
    /// </summary>
    public sealed class AppServices
    {
        public AppServices(
            CardDatabase database,
            GameRules rules,
            UiTexts texts,
            IReadOnlyList<DeckDef> decks,
            AiConfig ai,
            IMatchmakingService matchmaking,
            ISceneNavigator scenes,
            IRandom random)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Catalog = database.ToCatalog();
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            Texts = texts ?? throw new ArgumentNullException(nameof(texts));
            Decks = decks ?? throw new ArgumentNullException(nameof(decks));
            Ai = ai ?? throw new ArgumentNullException(nameof(ai));
            Matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            Scenes = scenes ?? throw new ArgumentNullException(nameof(scenes));
            Random = random ?? throw new ArgumentNullException(nameof(random));
            Difficulty = ai.DefaultProfileId;
        }

        public CardDatabase Database { get; }
        public CardCatalog Catalog { get; }
        public GameRules Rules { get; }
        public UiTexts Texts { get; }
        public IReadOnlyList<DeckDef> Decks { get; }
        public AiConfig Ai { get; }
        public IMatchmakingService Matchmaking { get; }
        public ISceneNavigator Scenes { get; }

        /// <summary>Seeds for local matches.</summary>
        public IRandom Random { get; }

        /// <summary>The player's deck chosen in the menu, kept between visits.</summary>
        public int DeckIndex { get; set; }

        /// <summary>The AI difficulty chosen in the menu.</summary>
        public string Difficulty { get; set; }

        /// <summary>The match the menu prepared for the match scene; the match scene takes it.</summary>
        public MatchLaunch PendingMatch { get; set; }
    }
}
