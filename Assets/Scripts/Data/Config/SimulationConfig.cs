using System.Collections.Generic;
using Game.Data.Decks;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>Parameters of the AI-versus-AI batch run started from the editor menu (Game/Simulation).</summary>
    [CreateAssetMenu(menuName = "Game/Config/Simulation", fileName = "SimulationConfig")]
    public sealed class SimulationConfig : ScriptableObject
    {
        [Header("Main run")]
        [SerializeField, Min(1)] private int _games = 1000;
        [SerializeField, Tooltip("Master seed; each match's seed is derived from it")] private long _seed = 1;
        [SerializeField, Tooltip("Empty: a new deck for every player and match, random and/or with synergy")] private List<DeckDef> _decks = new List<DeckDef>();
        [SerializeField, Tooltip("Without fixed decks: run the main batch on random decks")] private bool _randomDecksRun = true;
        [SerializeField, Tooltip("Without fixed decks: run the main batch again on decks whose creatures suit their fields")] private bool _synergyDecksRun = true;
        [SerializeField, Range(0, 1), Tooltip("Synergy decks: chance that a creature slot takes a random creature")] private float _synergyRandomShare = 0.15f;
        [SerializeField] private string _player1ProfileId = "normal";
        [SerializeField] private string _player2ProfileId = "normal";
        [SerializeField, Min(1), Tooltip("A match longer than this is recorded as hung")] private int _turnLimit = 200;

        [Header("Strength check (0 games: skip)")]
        [SerializeField, Min(0)] private int _strengthGames = 500;
        [SerializeField] private string _strongProfileId = "normal";
        [SerializeField] private string _weakProfileId = "easy";

        [Header("Replay one match (Game/Simulation/Replay)")]
        [SerializeField, Tooltip("From the report: the strength check or the main run")] private bool _replayStrengthRun;
        [SerializeField, Tooltip("The main run on synergy decks")] private bool _replaySynergyDecks;
        [SerializeField, Min(0)] private int _replayIndex;
        [SerializeField] private long _replaySeed;

        public int Games => _games;
        public ulong Seed => (ulong)_seed;
        public IReadOnlyList<DeckDef> Decks => _decks;
        public bool RandomDecksRun => _randomDecksRun;
        public bool SynergyDecksRun => _synergyDecksRun;
        public float SynergyRandomShare => _synergyRandomShare;
        public string Player1ProfileId => _player1ProfileId;
        public string Player2ProfileId => _player2ProfileId;
        public int TurnLimit => _turnLimit;
        public int StrengthGames => _strengthGames;
        public string StrongProfileId => _strongProfileId;
        public string WeakProfileId => _weakProfileId;
        public bool ReplayStrengthRun => _replayStrengthRun;
        public bool ReplaySynergyDecks => _replaySynergyDecks;
        public int ReplayIndex => _replayIndex;
        public ulong ReplaySeed => (ulong)_replaySeed;
    }
}
