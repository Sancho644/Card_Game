using System.Linq;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Match;
using Game.Presentation.Menu;
using Game.Presentation.Table;
using TMPro;
using UnityEngine;
namespace Game.Presentation.Ui
{
    /// <summary>Compact screen-edge HUD; commands remain in HumanDecisionUi.</summary>
    public sealed class TacticalHudView : MonoBehaviour
    {
        private CollectionUi _ui;
        private TopBarView _top;
        private PlayerNames _names;
        private UiTexts _texts;
        private PlayerView _view;
        private Camera _camera;
        private TableLayout _layout;
        private TMP_Text[] _pileLabels;
        private RectTransform _safe;
        public void Initialize(TacticalHudSettings settings,Camera camera,TableLayout layout,TopBarView top,
            ActionBarView actions,TextPanelView rule,TextPanelView prompt,EventLogView log,
            PlayerNames names,UiTexts texts,int target)
        {
            _camera=camera;_layout=layout;_texts=texts;_top=top;_names=names;
            _safe=(RectTransform)top.transform.parent;
            _ui=new CollectionUi(top.GetComponentInChildren<TMP_Text>(true).font,settings.Style);
            camera.rect=new Rect(0,0,1,1);
            var score=_ui.Plaque(_safe,"ScoreFrame",.22f,.934f,.56f,.06f);score.SetAsFirstSibling();
            top.Reflow(_ui,score,_safe,target);
            Place((RectTransform)rule.transform,_safe,.34f,.83f,.32f,.04f);
            var badge=rule.GetComponentInChildren<TMP_Text>(true);badge.fontSize=17;badge.enableAutoSizing=true;badge.fontSizeMin=12;badge.fontSizeMax=17;
            Place(badge.rectTransform,rule.transform,.02f,.05f,.96f,.9f);
            log.SetHeader(texts.Get("hud.log"));log.Reflow(_ui,_safe);
            Place((RectTransform)prompt.transform,_safe,.30f,.225f,.40f,.045f);
            var hint=prompt.GetComponentInChildren<TMP_Text>(true);hint.fontSize=18;hint.enableAutoSizing=true;hint.fontSizeMin=12;hint.fontSizeMax=18;
            Place(hint.rectTransform,prompt.transform,.03f,.08f,.94f,.84f);hint.alignment=TextAlignmentOptions.Midline;hint.textWrappingMode=TextWrappingModes.Normal;
            actions.Reflow(_ui,_safe);
            // Pile names belong to the table: world-space UI obeys card depth, including a lifted hand card.
            var pileCanvas=new GameObject("PileLabels",typeof(RectTransform),typeof(Canvas));
            pileCanvas.transform.SetParent(transform,false);
            var canvas=pileCanvas.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;
            _pileLabels=new TMP_Text[6];
            for(var i=0;i<6;i++){
                var label=_ui.Text(pileCanvas.transform,"PileLabel"+i,"",0,0,0,0,14);label.alignment=TextAlignmentOptions.Midline;
                label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);label.rectTransform.sizeDelta=new Vector2(150,24);
                _pileLabels[i]=label;
            }
            HideInfo();
        }
        public void Show(PlayerView view)
        {
            _view=view;
            _top.ShowMatch(view,_names);
        }
        public void HideInfo()=>_view=null;
        public static void Place(RectTransform rect,Transform parent,float x,float y,float w,float h)
        {rect.SetParent(parent,false);rect.anchorMin=new Vector2(x,y);rect.anchorMax=new Vector2(x+w,y+h);rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;}
        private void LateUpdate()
        {
            if(_pileLabels==null)return;
            for(var i=0;i<6;i++){
                var visible=_view!=null;_pileLabels[i].gameObject.SetActive(visible);if(!visible)continue;
                var side=i<3?TableSide.Bottom:TableSide.Top;var owner=i<3?_view.Viewer:_view.Viewer.Opponent();var kind=(PileKind)(i%3);
                var count=kind==PileKind.Discard?_view.Discard(owner).Count:kind==PileKind.OutOfGame?_view.OutOfGame(owner).Count:_view.Fields.Count(f=>f.Owner==owner&&f.IsRemoved);
                _pileLabels[i].text=_texts.Get("hud.pile."+kind)+" · "+count;
                var pose=_layout.Pile(side,kind,0,true);var position=pose.Position+new Vector3(0,0,-1.3f);
                var depth=Vector3.Dot(position-_camera.transform.position,_camera.transform.forward);
                var screenHeight=_camera.orthographic?2*_camera.orthographicSize:2*depth*Mathf.Tan(_camera.fieldOfView*Mathf.Deg2Rad*.5f);
                var pixelsPerUnit=_safe.GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
                var rect=_pileLabels[i].rectTransform;
                rect.SetPositionAndRotation(position,_camera.transform.rotation);
                rect.localScale=Vector3.one*(screenHeight/Mathf.Max(1,_camera.pixelHeight)*pixelsPerUnit);
            }
        }
        private void OnDestroy()=>_ui?.Dispose();
    }
}
