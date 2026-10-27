using System;

namespace Game.Core.Rules
{
    /// <summary>Tunable match settings, filled from GameRulesConfig; defaults follow docs/rules.md.</summary>
    public sealed class GameRules
    {
        public GameRules(
            int winsToWin,
            int legendaryAbilitiesPerTurn,
            DeckRules deck,
            DrawRule drawRule = DrawRule.BothReturnToHand)
        {
            WinsToWin = winsToWin;
            LegendaryAbilitiesPerTurn = legendaryAbilitiesPerTurn;
            Deck = deck ?? throw new ArgumentNullException(nameof(deck));
            DrawRule = drawRule;
        }

        public int WinsToWin { get; }
        public int LegendaryAbilitiesPerTurn { get; }
        public DeckRules Deck { get; }
        public DrawRule DrawRule { get; }

        public static GameRules Default => new GameRules(3, 1, DeckRules.Default);
    }
}
