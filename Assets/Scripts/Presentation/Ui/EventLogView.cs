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
        private string _headerLabel;

        public void Init(int maxLines) => _maxLines = maxLines;
        public void Reflow(Game.Presentation.Menu.CollectionUi ui,RectTransform sidebar)
        {
            TacticalHudView.Place((RectTransform)transform,sidebar,.05f,.305f,.90f,.09f);
            var header=ui.Button(sidebar,"LogHeader",_headerLabel,.05f,.397f,.90f,.026f,()=>_scroll.viewport.gameObject.SetActive(!_scroll.viewport.gameObject.activeSelf));
            ui.Decorate(header,17);
            _text.font=ui.Font;_text.fontSize=17;_text.richText=false;_text.raycastTarget=false;
            _scroll.horizontal=false;_scroll.vertical=true;_scroll.movementType=ScrollRect.MovementType.Clamped;
            var clip=_scroll.viewport.GetComponent<RectMask2D>();if(clip!=null)clip.enabled=false;
            var image=_scroll.viewport.GetComponent<UnityEngine.UI.Image>();if(image==null)image=_scroll.viewport.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=new Color(0,0,0,.01f);
            var mask=_scroll.viewport.GetComponent<UnityEngine.UI.Mask>();if(mask==null)mask=_scroll.viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;
            GetComponent<UnityEngine.UI.Image>().color=new Color(.02f,.05f,.05f,.95f);
        }
        public void SetHeader(string text)=>_headerLabel=text;

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
