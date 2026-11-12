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
        private RectTransform _drawer;
        private BattleWindowSkin _skin;
        public void Configure(BattleWindowSkin skin) => _skin = skin;

        public void Init(int maxLines) => _maxLines = maxLines;
        public void Reflow(Game.Presentation.Menu.CollectionUi ui,RectTransform safe)
        {
            _drawer=_skin != null ? _skin.Frame(safe,"LogDrawer",.015f,.078f,.34f,.40f) : ui.Frame(safe,"LogDrawer",.015f,.078f,.34f,.40f);
            TacticalHudView.Place((RectTransform)transform,_drawer,.045f,.06f,.91f,.74f);
            if (_skin != null)
            {
                _skin.Icon(_drawer,"JournalEmblem",BattleEmblem.Journal,.035f,.81f,.14f,.17f);
                var title=ui.Text(_drawer,"JournalTitle",_headerLabel,.19f,.83f,.72f,.12f);
                _skin.Label(title,30,TextAlignmentOptions.Left);title.color=_skin.Settings.Gold;
            }
            UnityEngine.UI.Button header=null;
            header=ui.Button(safe,"LogHeader",_headerLabel,.015f,.02f,.17f,.065f,()=>{
                var expanded=!_drawer.gameObject.activeSelf;
                _drawer.gameObject.SetActive(expanded);ui.Select(header,expanded);
                if(expanded)Refresh();
            });
            ui.Decorate(header,24);
            _text.font=ui.Font;_text.fontSize=20;_text.richText=false;_text.raycastTarget=false;
            _scroll.horizontal=false;_scroll.vertical=true;_scroll.movementType=ScrollRect.MovementType.Clamped;
            var clip=_scroll.viewport.GetComponent<RectMask2D>();if(clip!=null)clip.enabled=false;
            var image=_scroll.viewport.GetComponent<UnityEngine.UI.Image>();if(image==null)image=_scroll.viewport.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=new Color(0,0,0,.01f);
            var mask=_scroll.viewport.GetComponent<UnityEngine.UI.Mask>();if(mask==null)mask=_scroll.viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;
            GetComponent<UnityEngine.UI.Image>().enabled=false;
            _drawer.gameObject.SetActive(false);
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
