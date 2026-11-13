using System.Collections.Generic;
using DG.Tweening;
using Game.Data.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Cards
{
    /// <summary>Shared layered creature face for world cards, zoom and collection. All labels remain live.</summary>
    public sealed class CreatureCardFaceView : MonoBehaviour
    {
        private readonly Dictionary<Graphic,Color> _colors = new Dictionary<Graphic,Color>();
        private CardFace _face;
        private CardSkin _skin;
        private TMP_FontAsset _font;
        private TMP_Text _power;
        public TMP_Text PowerText => _power;

        public static CreatureCardFaceView Create(Transform parent)
        {
            var go = new GameObject("CreatureFace",typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            return go.AddComponent<CreatureCardFaceView>();
        }
        public void Bind(CardFace face,CardSkin skin,TMP_FontAsset font)
        {
            gameObject.SetActive(true);
            if(_face==face&&_skin==skin)return;
            foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            _colors.Clear();_face=face;_skin=skin;_font=font;
            var style=skin.CreatureStyle;
            var art=Node("Art",style.Art).gameObject.AddComponent<RawImage>();
            art.raycastTarget=false;
            if(face.Art!=null)
            {
                art.texture=face.Art.texture;
                var uv=face.Art.textureRect;var tex=face.Art.texture;
                // Cover the artwork window with a central crop, preserving its proportions.
                var aspect=(style.WorldSize.x/style.WorldSize.y)*style.Art.width/style.Art.height;
                var source=uv.width/uv.height;
                if(source>aspect){var width=uv.height*aspect;uv.x+=(uv.width-width)/2;uv.width=width;}
                else {var height=uv.width/aspect;uv.y+=(uv.height-height)/2;uv.height=height;}
                art.uvRect=new Rect(uv.x/tex.width,uv.y/tex.height,uv.width/tex.width,uv.height/tex.height);
                Remember(art,Color.white);
            }
            else {Remember(art,face.FrameColor*new Color(.3f,.3f,.3f,1));Image("ArtIcon",new Rect(.25f,.43f,.5f,.27f),face.ElementIcon,Color.white);}
            Image("Frame",style.FrameArea,face.CreatureFrame != null ? face.CreatureFrame : style.Frame,Color.white);
            Image("ElementIcon",style.ElementIcon,face.ElementIcon,Color.white);
            _power=Text("Power",style.Power,face.Power?.ToString()??"",style.Ink);
            Text("Element",style.Element,face.Stats,style.Ink);
            Text("Title",style.Title,face.Title,style.Ink);
            Text("Type",style.Type,face.TypeLabel,style.Ink);
            Text("Rarity",style.Rarity,face.Rarity,face.RarityColor);
            if(!string.IsNullOrEmpty(face.Body))
            {
                var rules=Node("RulesBackground",style.Rules).gameObject.AddComponent<Image>();
                rules.raycastTarget=false;Remember(rules,new Color(.04f,.06f,.07f,.88f));
                var text=Text("Rules",style.Rules,face.Body,style.Ink);text.alignment=TextAlignmentOptions.MidlineLeft;
            }
        }
        public void SetPower(int? value) {if(_power!=null)_power.text=(value??_face.Power??0).ToString();}
        public Tween FlashPower(Color color,float seconds)
        {
            if(_power==null)return null;
            var original=_skin.CreatureStyle.Ink;
            return DOTween.Sequence().Append(_power.DOColor(color,seconds*.25f)).Append(_power.DOColor(original,seconds*.75f)).SetTarget(this);
        }
        public void SetDimming(float brightness)
        {
            foreach(var pair in _colors)if(pair.Key!=null){var c=pair.Value;pair.Key.color=new Color(c.r*brightness,c.g*brightness,c.b*brightness,c.a);}
        }
        private RectTransform Node(string name,Rect area)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(transform,false);
            rect.anchorMin=new Vector2(area.xMin,area.yMin);rect.anchorMax=new Vector2(area.xMax,area.yMax);
            rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
        }
        private void Image(string name,Rect area,Sprite sprite,Color color)
        {
            if(sprite==null)return;
            var image=Node(name,area).gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.raycastTarget=false;
            image.preserveAspect=name!="Frame";Remember(image,color);
        }
        private TMP_Text Text(string name,Rect area,string value,Color color)
        {
            var text=Node(name,area).gameObject.AddComponent<TextMeshProUGUI>();text.font=_font;text.text=value;text.richText=false;
            // Center the visible glyphs, independent of the font's ascender/descender metrics.
            text.enableAutoSizing=true;text.fontSizeMin=1;text.fontSizeMax=250;text.alignment=TextAlignmentOptions.Midline;
            text.overflowMode=TextOverflowModes.Ellipsis;text.raycastTarget=false;Remember(text,color);return text;
        }
        private void OnDestroy() => DOTween.Kill(this);
        private void Remember(Graphic graphic,Color color){graphic.color=color;_colors.Add(graphic,color);}
    }
}
