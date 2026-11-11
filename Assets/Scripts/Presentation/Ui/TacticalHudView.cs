using System;
using System.Linq;
using Game.Core.Battle;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using Game.Presentation.Match;
using Game.Presentation.Menu;
using Game.Presentation.Table;
using TMPro;
using UnityEngine;
namespace Game.Presentation.Ui
{
    /// <summary>Public battle information only; commands remain in HumanDecisionUi.</summary>
    public sealed class TacticalHudView : MonoBehaviour
    {
        private CollectionUi _ui;
        private TacticalPowerView _own,_opponent;
        private TMP_Text _phase,_fieldText;
        private RectTransform _sidebar;
        private UnityEngine.UI.Button _field;
        private UiTexts _texts;
        private CardFaceBuilder _faces;
        private DialogView _details;
        private PlayerView _view;
        private BattlePower _power;
        private Camera _camera;
        private TableLayout _layout;
        private TMP_Text[] _pileLabels;
        private RectTransform _safe;
        public void Initialize(TacticalHudSettings settings,Camera camera,TableLayout layout,TopBarView top,
            ActionBarView actions,TextPanelView rule,TextPanelView prompt,EventLogView log,DialogView details,
            CardFaceBuilder faces,PlayerNames names,UiTexts texts,int target)
        {
            _camera=camera;_layout=layout;_texts=texts;_faces=faces;_details=details;
            _safe=(RectTransform)top.transform.parent;
            _ui=new CollectionUi(top.GetComponentInChildren<TMP_Text>(true).font,settings.Style);
            var share=settings.SidebarShare;camera.rect=new Rect(0,0,1-share,1);
            _sidebar=_ui.Frame(_safe,"TacticalSidebar",1-share,0,share,1);_sidebar.SetAsFirstSibling();
            var title=_ui.Text(_sidebar,"Phase","",.05f,.947f,.9f,.04f,30);title.alignment=TextAlignmentOptions.Midline;_phase=title;
            var score=_ui.Frame(_safe,"ScoreFrame",.01f,.92f,1-share-.02f,.075f);score.SetAsFirstSibling();
            top.Reflow(_ui,score,_sidebar);
            var goal=_ui.Text(score,"Goal",texts.Format("hud.goal",target),.1f,-.35f,.8f,.35f,16);goal.alignment=TextAlignmentOptions.Midline;
            _own=new TacticalPowerView(_ui,_sidebar,"OwnPower",.77f,new Color(.1f,.85f,.75f),()=>PowerDetails(false));
            _opponent=new TacticalPowerView(_ui,_sidebar,"OpponentPower",.64f,new Color(1,.65f,.15f),()=>PowerDetails(true));
            _field=_ui.Button(_sidebar,"BattleField","",.045f,.47f,.91f,.155f,FieldDetails);
            _field.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);
            _fieldText=_ui.Text(_field.transform,"FieldText","",.06f,.08f,.88f,.84f,20);
            _fieldText.enableAutoSizing=true;_fieldText.fontSizeMin=13;_fieldText.fontSizeMax=20;
            Place((RectTransform)rule.transform,_sidebar,.05f,.425f,.90f,.04f);
            var badge=rule.GetComponentInChildren<TMP_Text>(true);badge.fontSize=17;badge.enableAutoSizing=true;badge.fontSizeMin=12;badge.fontSizeMax=17;
            Place(badge.rectTransform,rule.transform,.02f,.05f,.96f,.9f);
            Place((RectTransform)log.transform,_sidebar,.05f,.305f,.90f,.11f);
            log.SetHeader(texts.Get("hud.log"));log.Reflow(_ui,_sidebar);
            Place((RectTransform)prompt.transform,_sidebar,.05f,.235f,.90f,.06f);
            var hint=prompt.GetComponentInChildren<TMP_Text>(true);hint.fontSize=18;hint.enableAutoSizing=true;hint.fontSizeMin=12;hint.fontSizeMax=18;
            Place(hint.rectTransform,prompt.transform,.03f,.08f,.94f,.84f);hint.alignment=TextAlignmentOptions.Midline;hint.textWrappingMode=TextWrappingModes.Normal;
            actions.Reflow(_ui,_sidebar);
            _pileLabels=new TMP_Text[6];
            for(var i=0;i<6;i++){
                var label=_ui.Text(_safe,"PileLabel"+i,"",0,0,0,0,14);label.alignment=TextAlignmentOptions.Midline;
                label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);label.rectTransform.sizeDelta=new Vector2(150,24);
                _pileLabels[i]=label;
            }
            HideInfo();
        }
        public void Show(PlayerView view,BattlePower power)
        {
            _view=view;_power=power;
            var phase=view.Battle!=null?"hud.battle":view.Phase==MatchPhase.AwaitingChoice?"hud.choice":view.Phase==MatchPhase.ChoosingTurnOrder?"hud.coin":view.Phase==MatchPhase.Ended?"hud.ended":"hud.turn";
            _phase.text=_texts.Get(phase);
            if(view.Battle==null||power==null){_own.Hide();_opponent.Hide();_field.gameObject.SetActive(false);return;}
            var field=view.Field(view.Battle.FieldId);
            var own=field.Occupant(view.Viewer);var other=field.Occupant(view.Viewer.Opponent());
            if(own!=null)_own.Show(_faces.Build(own.CardId),power.Of(view.Viewer),view.Battle.IsReady(view.Viewer)?_texts.Get(UiTextKeys.BattleReady):null);else _own.Hide();
            if(other!=null)_opponent.Show(_faces.Build(other.CardId),power.Of(view.Viewer.Opponent()),view.Battle.IsReady(view.Viewer.Opponent())?_texts.Get(UiTextKeys.BattleReady):null);else _opponent.Hide();
            _field.gameObject.SetActive(true);
            // Even the owner's closed field contributes no stats until opened. Unknown ids never reach the builder.
            if(!field.IsOpen){_fieldText.text=_texts.Get("hud.closed");return;}
            var face=_faces.Build(field.CardId);
            _fieldText.text=face.Title+"\n"+face.Stats+"\n"+_texts.Get(view.Battle.IsConditionCancelled?"hud.cancelled":"hud.open");
        }
        public void HideInfo(){_view=null;_power=null;_own.Hide();_opponent.Hide();_field.gameObject.SetActive(false);_phase.text="";}
        public static void Place(RectTransform rect,Transform parent,float x,float y,float w,float h)
        {rect.SetParent(parent,false);rect.anchorMin=new Vector2(x,y);rect.anchorMax=new Vector2(x+w,y+h);rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;}
        private void FieldDetails()
        {
            var field=_view?.Field(_view.Battle.FieldId);if(field==null||!field.IsOpen)return;
            var face=_faces.Build(field.CardId);_details.Show(face.Title,face.Stats+"\n\n"+face.Body,_texts.Get("library.close"),()=>{});
        }
        private void PowerDetails(bool opponent)
        {
            if(_view?.Battle==null||_power==null)return;
            var owner=opponent?_view.Viewer.Opponent():_view.Viewer;var p=_power.Of(owner);
            var card=_view.Field(_view.Battle.FieldId).Occupant(owner);if(card==null)return;
            var lines=_texts.Format("hud.base",p.BasePower)+"\n"+_texts.Format("hud.fieldValue",p.FieldValue);
            foreach(var item in p.Abilities)lines+="\n"+(item.Source==null?_texts.Get("hud.effect"):_faces.NameOf(item.Source.CardId))+TacticalPowerView.Signed(item.Amount);
            lines+="\n\n"+_texts.Format("hud.total",p.FinalPower);
            _details.Show(_faces.NameOf(card.CardId),lines,_texts.Get("library.close"),()=>{});
        }
        private void LateUpdate()
        {
            if(_pileLabels==null)return;
            for(var i=0;i<6;i++){
                var visible=_view!=null;_pileLabels[i].gameObject.SetActive(visible);if(!visible)continue;
                var side=i<3?TableSide.Bottom:TableSide.Top;var owner=i<3?_view.Viewer:_view.Viewer.Opponent();var kind=(PileKind)(i%3);
                var count=kind==PileKind.Discard?_view.Discard(owner).Count:kind==PileKind.OutOfGame?_view.OutOfGame(owner).Count:_view.Fields.Count(f=>f.Owner==owner&&f.IsRemoved);
                _pileLabels[i].text=_texts.Get("hud.pile."+kind)+" · "+count;
                var pose=_layout.Pile(side,kind,0,true);var point=_camera.WorldToScreenPoint(pose.Position+new Vector3(0,0,-1.3f));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_safe,point,null,out var local);_pileLabels[i].rectTransform.anchoredPosition=local;
            }
        }
        private void OnDestroy()=>_ui?.Dispose();
    }
}
