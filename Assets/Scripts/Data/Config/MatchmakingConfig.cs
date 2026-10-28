using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>The local matchmaking stub: how long the pretend search for an opponent lasts.</summary>
    [CreateAssetMenu(menuName = "Game/Config/Matchmaking", fileName = "MatchmakingConfig")]
    public sealed class MatchmakingConfig : ScriptableObject
    {
        [SerializeField, Min(0)] private float _searchSecondsMin = 1.5f;
        [SerializeField, Min(0)] private float _searchSecondsMax = 3f;

        public float SearchSecondsMin => _searchSecondsMin;
        public float SearchSecondsMax => Mathf.Max(_searchSecondsMin, _searchSecondsMax);
    }
}
