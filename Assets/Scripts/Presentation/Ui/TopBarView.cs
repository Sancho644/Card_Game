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
        public void Reflow(Game.Presentation.Menu.CollectionUi ui, RectTransform scoreFrame, RectTransform sidebar)
        {
            _showBothHands.gameObject.SetActive(false); _showBothHandsLabel.gameObject.SetActive(false);
            var image=GetComponent<UnityEngine.UI.Image>();if(image!=null)image.enabled=false;
            TacticalHudView.Place(_score.rectTransform,scoreFrame,.02f,.12f,.96f,.76f);
            _score.fontSize=24;_score.enableAutoSizing=true;_score.fontSizeMin=14;_score.fontSizeMax=24;_score.alignment=TextAlignmentOptions.Midline;
            TacticalHudView.Place(_turn.rectTransform,sidebar,.05f,.9f,.9f,.04f);
            _turn.fontSize=22;_turn.enableAutoSizing=true;_turn.fontSizeMin=14;_turn.fontSizeMax=22;_turn.alignment=TextAlignmentOptions.Midline;
        }

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
