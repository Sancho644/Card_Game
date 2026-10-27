using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>Buttons for actions without a card: end turn, ready, open the field, cancel a selection.</summary>
    public sealed class ActionBarView : MonoBehaviour
    {
        [SerializeField] private Button _endTurn;
        [SerializeField] private Button _ready;
        [SerializeField] private Button _openField;
        [SerializeField] private Button _cancel;

        public void Init(string endTurn, string ready, string openField, string cancel)
        {
            UiButtons.SetLabel(_endTurn, endTurn);
            UiButtons.SetLabel(_ready, ready);
            UiButtons.SetLabel(_openField, openField);
            UiButtons.SetLabel(_cancel, cancel);
            Hide();
        }

        /// <summary>Shows the buttons whose actions are given; a null action hides its button.</summary>
        public void Show(Action endTurn, Action ready, Action openField)
        {
            UiButtons.Bind(_endTurn, endTurn);
            UiButtons.Bind(_ready, ready);
            UiButtons.Bind(_openField, openField);
        }

        public void ShowCancel(Action cancel) => UiButtons.Bind(_cancel, cancel);

        public void Hide()
        {
            Show(null, null, null);
            ShowCancel(null);
        }
    }
}
