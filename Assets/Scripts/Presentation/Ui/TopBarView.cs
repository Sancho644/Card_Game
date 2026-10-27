using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>Score, whose turn it is and the debug switch "show both hands".</summary>
    public sealed class TopBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _score;
        [SerializeField] private TMP_Text _turn;
        [SerializeField] private Toggle _showBothHands;
        [SerializeField] private TMP_Text _showBothHandsLabel;

        public event Action<bool> ShowBothHandsChanged;

        public void Init(string showBothHandsLabel)
        {
            _showBothHandsLabel.text = showBothHandsLabel;
            _showBothHands.onValueChanged.RemoveAllListeners();
            _showBothHands.onValueChanged.AddListener(value => ShowBothHandsChanged?.Invoke(value));
        }

        public void Show(string score, string turn)
        {
            _score.text = score;
            _turn.text = turn;
        }
    }
}
