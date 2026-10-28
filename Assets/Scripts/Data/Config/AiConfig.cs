using System.Collections.Generic;
using System.Linq;
using Game.AI;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>The AI's difficulty levels and their weights.</summary>
    [CreateAssetMenu(menuName = "Game/Config/AI", fileName = "AiConfig")]
    public sealed class AiConfig : ScriptableObject
    {
        [SerializeField] private List<AiProfileData> _profiles = new List<AiProfileData>();
        [SerializeField] private string _defaultProfileId = "normal";

        public IReadOnlyList<AiProfileData> Profiles => _profiles;
        public string DefaultProfileId => _defaultProfileId;

        /// <summary>The profile with this id; the default one if the id is unknown.</summary>
        public AiProfile ToProfile(string id)
        {
            var data = _profiles.FirstOrDefault(profile => profile.Id == id)
                       ?? _profiles.FirstOrDefault(profile => profile.Id == _defaultProfileId)
                       ?? _profiles.FirstOrDefault();
            return data == null ? new AiProfile() : data.ToProfile();
        }
    }
}
