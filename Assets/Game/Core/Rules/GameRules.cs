using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;

namespace Game.Core.Rules
{
    /// <summary>How a draw is settled. Open question 1 in docs/rules.md; alternatives go here.</summary>
    public enum DrawRule
    {
        /// <summary>No win; both creatures return to hand; an open field is removed, a closed one stays.</summary>
        BothReturnToHand,
    }

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

    /// <summary>Deck composition: the exact rarities required for each card type.</summary>
    public sealed class DeckRules
    {
        private readonly Dictionary<CardType, IReadOnlyList<Rarity>> _required;

        public DeckRules(IReadOnlyDictionary<CardType, IReadOnlyList<Rarity>> requiredRarities)
        {
            if (requiredRarities == null)
                throw new ArgumentNullException(nameof(requiredRarities));

            _required = requiredRarities.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<Rarity>)pair.Value.ToList());
        }

        public int DeckSize => _required.Values.Sum(rarities => rarities.Count);

        public IReadOnlyList<Rarity> RequiredRarities(CardType type) =>
            _required.TryGetValue(type, out var rarities) ? rarities : Array.Empty<Rarity>();

        /// <summary>One common, one epic and one legendary card of each type.</summary>
        public static DeckRules Default
        {
            get
            {
                var oneOfEach = new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary };
                return new DeckRules(new Dictionary<CardType, IReadOnlyList<Rarity>>
                {
                    [CardType.Field] = oneOfEach,
                    [CardType.Creature] = oneOfEach,
                    [CardType.Ability] = oneOfEach,
                });
            }
        }
    }
}
