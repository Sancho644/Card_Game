using System.Collections.Generic;
using UnityEngine;

namespace Game.Data.Presentation
{
    /// <summary>
    /// Every player-facing interface string, by key (<see cref="UiTextKeys"/>). One place to translate later.
    /// Templates use string.Format placeholders: {0}, {1}...
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Config/UI Texts", fileName = "UiTexts")]
    public sealed class UiTexts : ScriptableObject
    {
        [SerializeField] private List<UiTextEntry> _entries = new List<UiTextEntry>();

        private Dictionary<string, string> _byKey;

        public IReadOnlyList<UiTextEntry> Entries => _entries;

        public string Get(string key)
        {
            if (_byKey == null)
            {
                _byKey = new Dictionary<string, string>();
                foreach (var entry in _entries)
                    _byKey[entry.Key] = entry.Text;
            }

            return _byKey.TryGetValue(key, out var text) ? text : $"[{key}]";
        }

        public string Format(string key, params object[] args) => string.Format(Get(key), args);

        private void OnValidate() => _byKey = null;
    }
}
