using System;
using System.Linq;
using Game.Core.Battle;
using Game.Presentation.Cards;
using Game.Presentation.Menu;
using TMPro;
using UnityEngine;
namespace Game.Presentation.Ui
{
    public sealed class TacticalPowerView
    {
        private readonly RectTransform _root;
        private readonly UnityEngine.UI.Image _art;
        private readonly TMP_Text _name,_formula,_total;
        public TacticalPowerView(CollectionUi ui,RectTransform parent,string name,float y,Color color,Action clicked)
        {
            var button=ui.Button(parent,name,"",.045f,y,.91f,.12f,clicked);
            button.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);_root=(RectTransform)button.transform;
            var accent=ui.Panel(_root,"Owner",.012f,.08f,.012f,.84f,color);accent.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            _art=ui.Rect(_root,"Art",.04f,.1f,.20f,.8f).gameObject.AddComponent<UnityEngine.UI.Image>();_art.preserveAspect=true;_art.raycastTarget=false;
            _name=ui.Text(_root,"Name","",.27f,.53f,.68f,.35f,21);_name.enableAutoSizing=true;_name.fontSizeMin=12;_name.fontSizeMax=21;
            _formula=ui.Text(_root,"Formula","",.27f,.12f,.43f,.34f,18);_formula.enableAutoSizing=true;_formula.fontSizeMin=10;_formula.fontSizeMax=18;
            _total=ui.Text(_root,"Total","",.71f,.10f,.25f,.38f,30);_total.alignment=TextAlignmentOptions.Midline;_total.enableAutoSizing=true;_total.fontSizeMin=18;_total.fontSizeMax=30;
        }
        public void Show(CardFace face,PowerBreakdown power,string readyStatus=null)
        {
            _root.gameObject.SetActive(true);_name.text=face.Title+(readyStatus==null?"":" · "+readyStatus);_art.sprite=face.Art??face.ElementIcon;
            _formula.text=power.BasePower+Signed(power.FieldValue)+string.Concat(power.Abilities.Select(a=>Signed(a.Amount)));
            _total.text="= "+power.FinalPower;
        }
        public void Hide()=>_root.gameObject.SetActive(false);
        public static string Signed(int value)=>value<0?" - "+(-(long)value):" + "+value;
    }
}
