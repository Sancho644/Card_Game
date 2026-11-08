using System.Collections.Generic;
using Game.Services.Collection;
namespace Game.Presentation.Tests
{
    internal sealed class MemoryCollectionStore : ICollectionStore
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        public string Read(string key) => _values.TryGetValue(key, out var value) ? value : null;
        public void Write(string key, string value) => _values[key] = value;
        public void Save() { }
    }
}
