using Game.Services.Collection;
using UnityEngine;
namespace Game.Presentation.App
{
    public sealed class PlayerPrefsCollectionStore : ICollectionStore
    {
        public string Read(string key) => PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
        public void Write(string key, string value) => PlayerPrefs.SetString(key, value);
        public void Save() => PlayerPrefs.Save();
    }
}
