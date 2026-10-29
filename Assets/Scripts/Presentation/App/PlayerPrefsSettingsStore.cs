using Game.Services.Settings;
using UnityEngine;

namespace Game.Presentation.App
{
    /// <summary>Settings store on the device, through Unity's PlayerPrefs.</summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public int GetInt(string key, int fallback) => PlayerPrefs.GetInt(key, fallback);

        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public void Save() => PlayerPrefs.Save();
    }
}
