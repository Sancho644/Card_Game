using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>The on-screen event log; keeps the newest lines.</summary>
    public sealed class EventLogView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private ScrollRect _scroll;

        private readonly LinkedList<string> _lines = new LinkedList<string>();
        private int _maxLines = 60;

        public void Init(int maxLines) => _maxLines = maxLines;

        public void Add(string line)
        {
            _lines.AddLast(line);
            while (_lines.Count > _maxLines)
                _lines.RemoveFirst();
            Refresh();
        }

        public void Clear()
        {
            _lines.Clear();
            Refresh();
        }

        private void Refresh()
        {
            _text.text = string.Join("\n", _lines);
            Canvas.ForceUpdateCanvases();
            _scroll.verticalNormalizedPosition = 0f;
        }
    }
}
