using System;
using Game.AI;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>One difficulty level of the AI, editable in the inspector. See <see cref="AiProfile"/>.</summary>
    [Serializable]
    public sealed class AiProfileData
    {
        [SerializeField] private string _id = "normal";
        [SerializeField] private string _displayName = "Обычный";
        [SerializeField, Min(1), Tooltip("Guesses about hidden cards per decision")] private int _samples = 3;
        [SerializeField, Min(0), Tooltip("Random spread added to action scores")] private float _noise;
        [SerializeField] private bool _expectsOpponentResponse = true;
        [SerializeField] private bool _usesReactions = true;
        [SerializeField, Min(0)] private float _reactionThreshold = 5;
        [SerializeField] private bool _prefersFirstTurn = true;
        [SerializeField, Min(0), Tooltip("Seconds before the AI acts, in the scene")] private float _thinkDelayMin = 0.6f;
        [SerializeField, Min(0)] private float _thinkDelayMax = 1.2f;
        [SerializeField] private AiWeightsData _weights = new AiWeightsData();

        public string Id => _id;
        public string DisplayName => _displayName;

        public AiProfile ToProfile() => new AiProfile
        {
            Id = _id,
            Samples = _samples,
            Noise = _noise,
            ExpectsOpponentResponse = _expectsOpponentResponse,
            UsesReactions = _usesReactions,
            ReactionThreshold = _reactionThreshold,
            PrefersFirstTurn = _prefersFirstTurn,
            ThinkDelayMin = _thinkDelayMin,
            ThinkDelayMax = Mathf.Max(_thinkDelayMin, _thinkDelayMax),
            Weights = _weights.ToWeights(),
        };
    }
}
