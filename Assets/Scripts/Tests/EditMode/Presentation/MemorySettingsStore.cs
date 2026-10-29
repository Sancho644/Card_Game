using System.Collections.Generic;
using Game.Services.Settings;

namespace Game.Presentation.Tests
{
    /// <summary>Settings store in memory, so tests do not touch the device storage.</summary>
    internal sealed class MemorySettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, int> _values = new Dictionary<string, int>();

        public int Saves { get; private set; }

        public int GetInt(string key, int fallback) => _values.TryGetValue(key, out var value) ? value : fallback;

        public void SetInt(string key, int value) => _values[key] = value;

        public void Save() => Saves++;
    }
}
