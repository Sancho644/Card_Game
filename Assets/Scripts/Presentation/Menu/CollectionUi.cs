using System;
using Game.Data.Presentation;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Menu
{
    /// <summary>Anchored controls using the menu's canvas and font. No additional input system.</summary>
    public sealed class CollectionUi : IDisposable
    {
        private readonly TMP_FontAsset _font;
        private readonly CollectionSettings _style;
        private readonly Sprite _panel;
        private readonly Sprite _button;
        public CollectionUi(TMP_FontAsset font, CollectionSettings style)
        {
            _font=font;_style=style;
            _panel=Slice(style.PanelArt,style.PanelCrop,.045f,.065f);
            _button=Slice(style.ButtonArt,style.ButtonCrop,.065f,.15f);
        }
        public void Dispose(){if(_panel!=null)UnityEngine.Object.Destroy(_panel);if(_button!=null)UnityEngine.Object.Destroy(_button);}
        private static Sprite Slice(Sprite source,Rect crop,float horizontal,float vertical)
        {
            if(source==null)return null;
            var tex=source.texture;var rect=new Rect(crop.x*tex.width,crop.y*tex.height,crop.width*tex.width,crop.height*tex.height);
            return Sprite.Create(tex,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(rect.width*horizontal,rect.height*vertical,rect.width*horizontal,rect.height*vertical));
        }
        private void Apply(UnityEngine.UI.Image image,Sprite sprite)
        {
            image.sprite=sprite;if(sprite!=null){image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=sprite==_button?_style.ButtonSliceDensity:_style.SliceDensity;}
        }
        public RectTransform Frame(Transform parent,string name,float x,float y,float w,float h)
        {
            var rect=Panel(parent,name,x,y,w,h,_panel==null?_style.Panel:Color.white);
            Apply(rect.GetComponent<UnityEngine.UI.Image>(),_panel);return rect;
        }
        public RectTransform Plaque(Transform parent,string name,float x,float y,float w,float h)
        {
            var rect=Panel(parent,name,x,y,w,h,_button==null?_style.Panel:Color.white);
            var image=rect.GetComponent<UnityEngine.UI.Image>();
            Apply(image,_button);image.raycastTarget=false;
            return rect;
        }
        public void Select(UnityEngine.UI.Button button,bool selected)
        {
            button.GetComponent<UnityEngine.UI.Image>().color=selected?new Color(1,.85f,.5f):Color.white;
            var outline=button.GetComponent<UnityEngine.UI.Outline>();if(outline==null)outline=button.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor=new Color(1,.65f,.15f,.8f);outline.effectDistance=new Vector2(2,-2);outline.enabled=selected;
        }
        public TMP_FontAsset Font => _font;
        public void Decorate(UnityEngine.UI.Button button, float size)
        {
            var image=button.GetComponent<UnityEngine.UI.Image>();
            if(image!=null){Apply(image,_button);image.color=Color.white;button.targetGraphic=image;}
            var label=button.GetComponentInChildren<TMP_Text>(true);
            if(label!=null){label.font=_font;label.color=_style.Text;label.enableAutoSizing=true;label.fontSizeMin=12;label.fontSizeMax=size;label.textWrappingMode=TextWrappingModes.NoWrap;}
        }
        public RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(x + w, y + h);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
        public RectTransform Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rect = Rect(parent, name, x, y, w, h);
            rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = color;
            return rect;
        }
        public TMP_Text Text(Transform parent, string name, string value, float x, float y, float w, float h, float size = 0)
        {
            var rect = Rect(parent, name, x, y, w, h);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _font; text.fontSize = size == 0 ? _style.FontSize : size;
            text.color = _style.Text; text.text = value; text.richText = false;
            text.raycastTarget = false; text.alignment = TextAlignmentOptions.MidlineLeft;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }
        public UnityEngine.UI.Button Button(Transform parent, string name, string label, float x, float y, float w, float h, Action clicked)
        {
            var rect = Panel(parent, name, x, y, w, h, _button==null?_style.Button:Color.white);
            Apply(rect.GetComponent<UnityEngine.UI.Image>(),_button);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            var text = Text(rect, "Label", label, .04f, .04f, .92f, .92f);
            text.alignment = TextAlignmentOptions.Midline;
            text.enableAutoSizing=true;text.fontSizeMin=12;text.fontSizeMax=_style.SmallSize;
            text.textWrappingMode=TextWrappingModes.NoWrap;
            button.onClick.AddListener(() => clicked());
            return button;
        }
        public TMP_InputField Input(Transform parent, string name, float x, float y, float w, float h, string placeholder, Action<string> changed, bool live=false)
        {
            var rect = Panel(parent, name, x, y, w, h, _button==null?_style.Panel:Color.white);
            Apply(rect.GetComponent<UnityEngine.UI.Image>(),_button);
            var viewport = Rect(rect, "Viewport", .03f, .05f, .94f, .9f);
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var text = Text(viewport, "Text", "", 0, 0, 1, 1);
            var hint = Text(viewport, "Placeholder", placeholder, 0, 0, 1, 1);
            hint.color = _style.Muted;
            var input = rect.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = viewport; input.textComponent = (TextMeshProUGUI)text;
            input.placeholder = hint; input.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.customCaretColor = true; input.caretColor = _style.Text;
            if(live)input.onValueChanged.AddListener(value=>changed(value));
            else input.onEndEdit.AddListener(value => changed(value));
            return input;
        }
        public RectTransform Scroll(Transform parent,string name,float x,float y,float w,float h,out UnityEngine.UI.ScrollRect scroll)
        {
            var rect=Rect(parent,name,x,y,w,h);scroll=rect.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            var viewport=Panel(rect,"Viewport",0,0,1,1,new Color(0,0,0,.01f));
            var mask=viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;
            var content=Rect(viewport,"Content",0,1,1,0);content.pivot=new Vector2(.5f,1);
            var layout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;layout.spacing=12;layout.padding=new RectOffset(8,8,8,8);
            var fit=content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fit.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            viewport.anchorMax=new Vector2(.975f,1);
            var track=Panel(rect,"Scrollbar",.98f,0,.015f,1,new Color(.025f,.06f,.06f));
            var scrollbar=track.gameObject.AddComponent<UnityEngine.UI.Scrollbar>();
            var handle=Panel(track,"Handle",0,0,1,1,new Color(.65f,.45f,.22f));
            scrollbar.handleRect=handle;scrollbar.targetGraphic=handle.GetComponent<UnityEngine.UI.Image>();scrollbar.direction=UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHide;
            return content;
        }
    }
}
