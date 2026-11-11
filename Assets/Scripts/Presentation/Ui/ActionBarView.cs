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
        public void Reflow(Game.Presentation.Menu.CollectionUi ui, RectTransform sidebar)
        {
            TacticalHudView.Place((RectTransform)transform,sidebar,.05f,.015f,.90f,.215f);
            var layout=GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing=5;layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            foreach(var button in GetComponentsInChildren<Button>(true)){
                ui.Decorate(button,22);
                var element=button.GetComponent<UnityEngine.UI.LayoutElement>();if(element==null)element=button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                element.minHeight=40;element.preferredHeight=48;element.flexibleHeight=0;
            }
            ui.Select(_ready,true);
        }

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
