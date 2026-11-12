using System.Collections.Generic;
using Game.Data.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Game.Presentation.Cards
{
    /// <summary>Shared field face with live bonus values and condition text in every card surface.</summary>
    public sealed class FieldCardFaceView : MonoBehaviour
    {
        private readonly Dictionary<Graphic,Color> _colors=new Dictionary<Graphic,Color>();
        private CardFace _face;
        private CardSkin _skin;
        private TMP_FontAsset _font;
        private TMP_Text _conditionTitle;
        private TMP_Text _rules;
        public static FieldCardFaceView Create(Transform parent)
        {
            var go=new GameObject("FieldFace",typeof(RectTransform));
            var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            return go.AddComponent<FieldCardFaceView>();
        }
        public void Bind(CardFace face,CardSkin skin,TMP_FontAsset font)
        {
            gameObject.SetActive(true);
            if(_face==face&&_skin==skin)return;
            foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            _colors.Clear();_face=face;_skin=skin;_font=font;
            var style=skin.FieldStyle;
            var art=Node("Art",style.Art).gameObject.AddComponent<RawImage>();art.raycastTarget=false;
            if(face.Art!=null){
                art.texture=face.Art.texture;
                var uv=face.Art.textureRect;var tex=face.Art.texture;
                var aspect=style.WorldSize.x/style.WorldSize.y*style.Art.width/style.Art.height;
                if(uv.width/uv.height>aspect){var width=uv.height*aspect;uv.x+=(uv.width-width)/2;uv.width=width;}
                else{var height=uv.width/aspect;uv.y+=(uv.height-height)/2;uv.height=height;}
                art.uvRect=new Rect(uv.x/tex.width,uv.y/tex.height,uv.width/tex.width,uv.height/tex.height);
                Remember(art,Color.white);
            }else{
                Remember(art,face.FrameColor*new Color(.28f,.28f,.28f,1));
                Image("ArtPlaceholder",new Rect(.40f,.55f,.38f,.22f),face.ElementIcon,Color.white);
            }
            Image("Frame",new Rect(0,0,1,1),style.Frame,Color.white);
            Image("RarityGem",style.RarityIcon,face.RarityIcon,face.RarityColor);
            var rail=Node("Bonuses",style.BonusRail);
            for(var i=0;i<face.FieldBonuses.Count;i++){
                var bonus=face.FieldBonuses[i];var height=1f/face.FieldBonuses.Count;
                var panel=Node("Bonus"+i,new Rect(0,1-(i+1)*height,1,height*.96f),rail);
                Image("Panel",new Rect(0,0,1,1),style.BonusPanel,Color.white,panel);
                Image("Icon",new Rect(.20f,.53f,.60f,.39f),bonus.Icon,Color.white,panel);
                var element=Text("Element",new Rect(.05f,.29f,.90f,.22f),bonus.Element,style.Ink,panel);
                var value=Text("Value",new Rect(.05f,.06f,.90f,.23f),(bonus.Value>0?"+":"")+bonus.Value,style.Ink,panel);
                element.textWrappingMode=value.textWrappingMode=TextWrappingModes.NoWrap;
            }
            Text("Title",style.Title,face.Title,style.Ink);
            var colon=(face.Body??"").IndexOf(':');var hasTitle=colon>0&&colon<45;
            _conditionTitle=Text("ConditionTitle",style.ConditionTitle,hasTitle?face.Body.Substring(0,colon):"",style.Ink);
            _rules=Text("Rules",style.Rules,hasTitle?face.Body.Substring(colon+1).Trim():face.Body,style.Ink);
            Text("Type",style.Type,face.TypeLabel,style.Ink);
            Text("Rarity",style.Rarity,face.Rarity,style.Ink);
        }
        public void SetCancelled(bool cancelled)
        {
            foreach(var text in new[]{_conditionTitle,_rules})if(text!=null)
                text.fontStyle=cancelled?text.fontStyle|FontStyles.Strikethrough:text.fontStyle&~FontStyles.Strikethrough;
        }
        public void SetDimming(float brightness)
        {
            foreach(var pair in _colors)if(pair.Key!=null){var c=pair.Value;pair.Key.color=new Color(c.r*brightness,c.g*brightness,c.b*brightness,c.a);}
        }
        private RectTransform Node(string name,Rect area,Transform parent=null)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent!=null?parent:transform,false);
            rect.anchorMin=area.min;rect.anchorMax=area.max;rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
        }
        private void Image(string name,Rect area,Sprite sprite,Color color,Transform parent=null)
        {
            if(sprite==null)return;var image=Node(name,area,parent).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite=sprite;image.raycastTarget=false;image.preserveAspect=name=="Icon"||name=="RarityGem"||name=="ArtPlaceholder";Remember(image,color);
        }
        private TMP_Text Text(string name,Rect area,string value,Color color,Transform parent=null)
        {
            var text=Node(name,area,parent).gameObject.AddComponent<TextMeshProUGUI>();text.font=_font;text.text=value??"";text.richText=false;
            text.enableAutoSizing=true;text.fontSizeMin=1;text.fontSizeMax=250;text.alignment=TextAlignmentOptions.Midline;
            text.overflowMode=TextOverflowModes.Ellipsis;text.raycastTarget=false;Remember(text,color);return text;
        }
        private void Remember(Graphic graphic,Color color){graphic.color=color;_colors.Add(graphic,color);}
    }
}
