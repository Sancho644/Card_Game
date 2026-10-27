using System;
using UnityEngine;

namespace Game.Data.Presentation
{
    [Serializable]
    public struct UiTextEntry
    {
        public string Key;

        [TextArea(1, 3)]
        public string Text;

        public UiTextEntry(string key, string text)
        {
            Key = key;
            Text = text;
        }
    }
}
