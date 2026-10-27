using System.Collections.Generic;
using Game.Core.Cards;
using Game.Core.Rules;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>Match settings from docs/rules.md, tunable in the inspector.</summary>
    [CreateAssetMenu(menuName = "Game/Config/Game Rules", fileName = "GameRules")]
    public sealed class GameRulesConfig : ScriptableObject
    {
        [Header("Match")]
        [SerializeField, Min(1)] private int _winsToWin = 3;
        [SerializeField, Min(0)] private int _legendaryAbilitiesPerTurn = 1;
        [SerializeField] private DrawRule _drawRule = DrawRule.BothReturnToHand;

        [Header("Deck: rarities required for each card type")]
        [SerializeField] private List<Rarity> _fieldRarities = OneOfEach();
        [SerializeField] private List<Rarity> _creatureRarities = OneOfEach();
        [SerializeField] private List<Rarity> _abilityRarities = OneOfEach();

        [Header("Timers, seconds (docs/rules.md §12)")]
        [SerializeField, Min(1)] private float _turnOrderChoiceSeconds = 10;
        [SerializeField, Min(1)] private float _turnSeconds = 45;
        [SerializeField, Min(1)] private float _battleDecisionSeconds = 15;

        public float TurnOrderChoiceSeconds => _turnOrderChoiceSeconds;
        public float TurnSeconds => _turnSeconds;
        public float BattleDecisionSeconds => _battleDecisionSeconds;

        /// <summary>Plain engine rules. Timers stay here: the engine knows nothing about time.</summary>
        public GameRules ToRules() =>
            new GameRules(
                _winsToWin,
                _legendaryAbilitiesPerTurn,
                new DeckRules(new Dictionary<CardType, IReadOnlyList<Rarity>>
                {
                    [CardType.Field] = _fieldRarities,
                    [CardType.Creature] = _creatureRarities,
                    [CardType.Ability] = _abilityRarities,
                }),
                _drawRule);

        private static List<Rarity> OneOfEach() => new List<Rarity> { Rarity.Common, Rarity.Epic, Rarity.Legendary };
    }
}
