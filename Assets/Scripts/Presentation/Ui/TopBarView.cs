using System;
using Game.Core.State;
using Game.Presentation.Match;
using Game.Presentation.Menu;
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
        private TMP_Text _ownName;
        private TMP_Text _opponentName;
        private VictoryPipGraphic[] _ownPips;
        private VictoryPipGraphic[] _opponentPips;

        public event Action<bool> ShowBothHandsChanged;
        public void Reflow(CollectionUi ui, RectTransform scoreFrame, RectTransform safe, int target)
        {
            _showBothHands.gameObject.SetActive(false); _showBothHandsLabel.gameObject.SetActive(false);
            var image=GetComponent<UnityEngine.UI.Image>();if(image!=null)image.enabled=false;
            TacticalHudView.Place(_score.rectTransform,scoreFrame,.40f,.12f,.20f,.76f);
            _score.fontSize=30;_score.enableAutoSizing=true;_score.fontSizeMin=18;_score.fontSizeMax=30;_score.alignment=TextAlignmentOptions.Midline;
            _ownName=ui.Text(scoreFrame,"OwnName","",.18f,.12f,.22f,.76f,26);
            _opponentName=ui.Text(scoreFrame,"OpponentName","",.60f,.12f,.22f,.76f,26);
            foreach(var label in new[]{_ownName,_opponentName}){
                label.alignment=TextAlignmentOptions.Midline;label.enableAutoSizing=true;label.fontSizeMin=14;label.fontSizeMax=26;
                label.textWrappingMode=TextWrappingModes.NoWrap;
            }
            _ownPips=CreatePips(ui,scoreFrame,"OwnPip",target,false);
            _opponentPips=CreatePips(ui,scoreFrame,"OpponentPip",target,true);
            TacticalHudView.Place(_turn.rectTransform,safe,.34f,.895f,.32f,.035f);
            _turn.fontSize=20;_turn.enableAutoSizing=true;_turn.fontSizeMin=14;_turn.fontSizeMax=20;_turn.alignment=TextAlignmentOptions.Midline;
        }

        public void ShowMatch(PlayerView view,PlayerNames names)
        {
            if(_ownName==null)return;
            var own=view.Wins(view.Viewer);var opponent=view.Wins(view.Viewer.Opponent());
            _ownName.text=names.Of(view.Viewer);_opponentName.text=names.Of(view.Viewer.Opponent());
            _score.text=own+" : "+opponent;
            FillPips(_ownPips,own,new Color(.12f,.92f,.85f));
            FillPips(_opponentPips,opponent,new Color(1f,.67f,.15f));
        }

        private static VictoryPipGraphic[] CreatePips(CollectionUi ui,RectTransform parent,string name,int count,bool right)
        {
            var pips=new VictoryPipGraphic[count];
            var step=.14f/Mathf.Max(1,count);
            for(var i=0;i<count;i++){
                var x=(right?.84f:.02f)+i*step;
                var rect=ui.Rect(parent,name+i,x,.24f,step*.84f,.52f);
                pips[i]=rect.gameObject.AddComponent<VictoryPipGraphic>();pips[i].raycastTarget=false;
            }
            return pips;
        }

        private static void FillPips(VictoryPipGraphic[] pips,int wins,Color lit)
        {
            for(var i=0;i<pips.Length;i++)pips[i].Show(i<wins,lit);
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
