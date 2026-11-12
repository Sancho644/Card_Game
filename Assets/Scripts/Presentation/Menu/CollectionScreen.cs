using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.App;
using Game.Services.Collection;
using TMPro;
using UnityEngine;
using DG.Tweening;
namespace Game.Presentation.Menu
{
    /// <summary>Owns library/editor navigation; card rules and persistence remain in their services.</summary>
    public sealed class CollectionScreen : IDisposable
    {
        private const int PageSize=8;
        private readonly AppServices _app;
        private readonly CollectionSettings _style;
        private readonly CollectionUi _ui;
        private readonly CollectionCardView _cardView;
        private readonly CollectionEditorView _editor;
        private readonly CollectionLibraryView _library;
        private readonly CollectionDeckGrid _editorDeck;
        private readonly CollectionDeckGrid _libraryDeck;
        private readonly RectTransform _root;
        private readonly RectTransform _modal;
        private readonly List<GameObject> _cards=new List<GameObject>();
        private readonly List<GameObject> _deckRows=new List<GameObject>();
        private readonly List<GameObject> _modalContent=new List<GameObject>();
        private readonly Action _closed;
        private readonly Action _playAi;
        private int _editing;
        private int _page;
        private bool _browseCollection;
        private CardType? _type;
        private Rarity? _rarity;
        private Element? _element;
        private CardDef _focused;
        private Sequence _previewTween;
        private RectTransform _previewCard;
        private RectTransform _previewActions;
        private CanvasGroup _previewOrigin;
        private Vector2 _previewStart;
        private float _previewStartScale;
        private string _search="",_deckSearch="";
        public CollectionScreen(Transform parent,TMP_FontAsset font,CollectionSettings style,AppServices app,Action closed,Action playAi)
        {
            _app=app;_style=style;_closed=closed;_playAi=playAi;_ui=new CollectionUi(font,style);
            _cardView=new CollectionCardView(_ui,style,new Game.Presentation.Cards.CardFaceBuilder(app.Database,style.CardVisuals,app.Texts));
            _root=_ui.Panel(parent,"Collection",0,0,1,1,style.Background);
            if(style.BackgroundArt!=null){
                var backdrop=_ui.Rect(_root,"Backdrop",0,0,1,1);
                var image=backdrop.gameObject.AddComponent<UnityEngine.UI.Image>();
                image.sprite=style.BackgroundArt;image.raycastTarget=false;
                var fit=backdrop.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio=style.BackgroundArt.rect.width/style.BackgroundArt.rect.height;
            }
            _editor=new CollectionEditorView(_root,_ui,style,app.Texts);
            _library=new CollectionLibraryView(_root,_ui,style,app.Texts);
            _editorDeck=new CollectionDeckGrid(_editor.Deck,_ui,_cardView,app.Database,app.Texts);
            _libraryDeck=new CollectionDeckGrid(_library.Deck,_ui,_cardView,app.Database,app.Texts);
            _editor.BackClicked=ShowLibrary;_editor.CloseClicked=Close;_editor.AddClicked=Add;_editor.RemoveClicked=Remove;_editor.DetailsClicked=ShowDetails;
            _editor.PlayClicked=Play;_editor.PreviousClicked=()=>{_page--;RefreshEditor();};_editor.NextClicked=()=>{_page++;RefreshEditor();};
            _editor.Renamed=Rename;_editor.Searched=value=>{_search=value;_page=0;_focused=null;RefreshEditor();};
            _editor.TypeSelected=value=>{_type=value;_rarity=null;_page=0;_focused=null;RefreshEditor();};
            _editor.ElementSelected=value=>{_element=_element==value?(Element?)null:value;_page=0;_focused=null;RefreshEditor();};
            _editor.RarityClicked=()=>{_rarity=!_rarity.HasValue?Rarity.Common:_rarity==Rarity.Legendary?(Rarity?)null:(Rarity)((int)_rarity.Value+1);_page=0;_focused=null;RefreshEditor();};
            _editor.SourceSelected=Browse;_editor.DeleteClicked=ShowDeckActions;
            _library.CloseClicked=Close;_library.NewClicked=NewDeck;_library.EditClicked=()=>OpenEditor(_editing);_library.PlayClicked=Play;_library.UseClicked=Use;
            _library.ClearClicked=ShowDeckActions;_library.Renamed=Rename;_library.Searched=value=>{_deckSearch=value;RefreshLibrary();_library.Scroll.verticalNormalizedPosition=1;};
            _modal=_ui.Panel(_root,"Modal",0,0,1,1,new Color(0,0,0,.90f));
            _modal.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseModal);
            _modal.gameObject.SetActive(false);_root.gameObject.SetActive(false);
        }
        public void Show(){_root.gameObject.SetActive(true);OpenEditor(_app.Collection.SelectedIndex);}
        public void Dispose(){_previewTween?.Kill();if(_root!=null)UnityEngine.Object.Destroy(_root.gameObject);_ui.Dispose();}
        private string T(string key)=>_app.Texts.Get("collection."+key);
        private string L(string key)=>_app.Texts.Get("library."+key);
        private string DeckName(int index){var deck=_app.Collection.Get(index);return !string.IsNullOrEmpty(deck.Name)?deck.Name:deck.Cards.Count==0?T("emptyPreset"):L("unnamed");}
        private void OpenEditor(int index)
        {
            _editing=index;_browseCollection=false;_type=null;_rarity=null;_element=null;_focused=null;_search="";_page=0;
            _editor.Search.SetTextWithoutNotify("");_editor.Root.gameObject.SetActive(true);_library.Root.gameObject.SetActive(false);HideModalInstantly();RefreshEditor();
        }
        private void ShowLibrary(){_editor.Root.gameObject.SetActive(false);_library.Root.gameObject.SetActive(true);HideModalInstantly();RefreshLibrary();}
        private void Browse(bool collection){_browseCollection=collection;_type=null;_rarity=null;_element=null;_search="";_focused=null;_page=0;_editor.Search.SetTextWithoutNotify("");RefreshEditor();}
        private void Close(){HideModalInstantly();_root.gameObject.SetActive(false);_closed();}
        private void Rename(string name)
        {
            if(!string.IsNullOrWhiteSpace(name))_app.Collection.Rename(_editing,name);
            Refresh();
        }
        private void NewDeck(){var index=_app.Collection.Create(_app.Texts.Format("library.newName",_app.Collection.Count+1));OpenEditor(index);}
        private void Refresh(){if(_library.Root.gameObject.activeSelf)RefreshLibrary();else RefreshEditor();}
        private bool MatchesElement(CardDef card)
        {
            if(!_element.HasValue)return true;
            if(card is CreatureDef creature)return creature.Element==_element.Value;
            if(card is FieldDef field)return field.Modifiers.Any(m=>m.Element==_element.Value&&m.Value!=0);
            return false;
        }
        private void RefreshEditor()
        {
            CollectionDeckGrid.Clear(_cards);
            var preset=_app.Collection.Get(_editing);
            var source=_browseCollection?_app.Collection.OwnedCards:preset.Cards;
            var list=source.Select(_app.Database.Find)
                .Where(c=>(!_type.HasValue||c.Type==_type)&&(!_rarity.HasValue||c.Rarity==_rarity)&&MatchesElement(c)
                    &&(string.IsNullOrWhiteSpace(_search)||c.DisplayName.IndexOf(_search.Trim(),StringComparison.OrdinalIgnoreCase)>=0||(c.Description??"").IndexOf(_search.Trim(),StringComparison.OrdinalIgnoreCase)>=0))
                .OrderBy(c=>c.Type).ThenBy(c=>c.Rarity).ThenBy(c=>c.DisplayName).ToList();
            var pages=Math.Max(1,(list.Count+PageSize-1)/PageSize);_page=Mathf.Clamp(_page,0,pages-1);
            _editor.Name.SetTextWithoutNotify(preset.Name);
            _editor.Count.text=DeckName(_editing)+" · "+preset.Cards.Count+" / 9";
            _editor.Page.text=_app.Texts.Format("collection.page",_page+1,pages,list.Count);
            _editor.Previous.interactable=_page>0;_editor.Next.interactable=_page+1<pages;
            _editor.Filter.text=(_type.HasValue?T("filter."+_type.Value):T(_browseCollection?"all":"deckCards"))+
                (_rarity.HasValue?" · "+_app.Texts.Get(UiTextKeys.RarityPrefix+_rarity.Value):"")+
                (_element.HasValue?" · "+_app.Texts.Get(UiTextKeys.ElementPrefix+_element.Value):"")+" · "+_app.Texts.Format("library.cardCount",list.Count);
            _editor.Rarity.GetComponentInChildren<TMP_Text>().text=_rarity.HasValue?_app.Texts.Get(UiTextKeys.RarityPrefix+_rarity.Value):L("rarity");
            _ui.Select(_editor.AllTypes,!_type.HasValue);foreach(var tab in _editor.Types)_ui.Select(tab.Value,_type==tab.Key);
            _ui.Select(_editor.Rarity,_rarity.HasValue);
            foreach(var tab in _editor.Elements)_ui.Select(tab.Value,_element==tab.Key);
            _ui.Select(_editor.DeckTab,!_browseCollection);_ui.Select(_editor.CollectionTab,_browseCollection);
            if(list.Count==0){
                var empty=_ui.Text(_editor.Grid,"EmptyResult",L("noResults"),.05f,.25f,.9f,.5f);empty.alignment=TextAlignmentOptions.Midline;_cards.Add(empty.gameObject);
            }
            var visible=list.Skip(_page*PageSize).Take(PageSize).ToList();
            for(var i=0;i<visible.Count;i++){
                var card=visible[i];UnityEngine.UI.Button button=null;
                button=_cardView.Create(_editor.Grid,card,i,preset.Cards.Contains(card.Id),_focused==card,T("inDeck"),()=>Preview(card,(RectTransform)button.transform));
                _cards.Add(button.transform.parent.parent.parent.gameObject);
            }
            _editorDeck.Refresh(preset,_focused,(type,rarity,card,origin)=>{if(card!=null)Preview(card,origin);else ChooseSlot(type,rarity,null);});
            _editor.Focused.text=_focused==null?T("inspect"):_focused.DisplayName;
            if(_focused is CreatureDef creature)_editor.Focused.text+=" · "+_app.Texts.Get(UiTextKeys.ElementPrefix+creature.Element)+" · "+creature.Power;
            var inDeck=_focused!=null&&preset.Cards.Contains(_focused.Id);
            _editor.Add.interactable=_focused!=null&&!inDeck;_editor.Remove.interactable=inDeck;
            _editor.Details.interactable=_focused!=null;
            _editor.Play.interactable=_app.Collection.Validate(_editing).IsValid;
            _editor.Status.text=ValidationText();_editor.Status.color=_editor.Play.interactable?_style.Text:new Color(1,.72f,.42f);
        }
        private void ChooseSlot(CardType type,Rarity rarity,CardDef card)
        {
            if(card!=null){Preview(card,FindCardOrigin(card));return;}
            _type=type;_rarity=rarity;_element=null;_search="";_page=0;_focused=card;_browseCollection=true;
            _editor.Search.SetTextWithoutNotify("");RefreshEditor();
        }
        private string ValidationText()
        {
            var validation=_app.Collection.Validate(_editing);
            if(validation.IsValid)return L("ready");
            return string.Join("\n",validation.Errors.Select(error=>{
                if(error.Code==DeckErrorCode.SignatureCreatureMissing){
                    var ability=(AbilityCard)_app.Catalog.Get(error.CardId);
                    return _app.Texts.Format("collection.signature",_app.Database.Find(error.CardId).DisplayName,_app.Database.Find(ability.BoundCreatureId).DisplayName);
                }
                return L("incomplete");
            }).Distinct());
        }
        private void RefreshLibrary()
        {
            CollectionDeckGrid.Clear(_deckRows);_library.Name.SetTextWithoutNotify(_app.Collection.Get(_editing).Name);
            var indices=Enumerable.Range(0,_app.Collection.Count).Where(i=>string.IsNullOrWhiteSpace(_deckSearch)||DeckName(i).IndexOf(_deckSearch.Trim(),StringComparison.OrdinalIgnoreCase)>=0).ToList();
            _library.Count.text=_app.Texts.Format("library.deckCount",_app.Collection.Count);
            for(var row=0;row<(indices.Count+2)/3;row++){
                var holder=_ui.Rect(_library.Content,"DeckRow"+row,0,0,1,1);holder.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=230;
                _deckRows.Add(holder.gameObject);
                for(var col=0;col<3&&row*3+col<indices.Count;col++){
                    var index=indices[row*3+col];var deck=_app.Collection.Get(index);
                    var button=_ui.Button(holder,"Preset"+index,"",col/3f+.01f,.02f,.313f,.96f,()=>{_editing=index;RefreshLibrary();});
                    button.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);_ui.Select(button,index==_editing);
                    var art=deck.Cards.Select(_app.Database.Find).OfType<CreatureDef>().OrderByDescending(c=>c.Rarity).Select(c=>c.Art).FirstOrDefault(a=>a!=null);
                    if(art!=null){
                        var image=_ui.Rect(button.transform,"Cover",.035f,.31f,.93f,.64f).gameObject.AddComponent<UnityEngine.UI.RawImage>();
                        image.texture=art.texture;image.uvRect=new Rect(0,.50f,1,.48f);image.raycastTarget=false;
                    }
                    _ui.Text(button.transform,"Name",DeckName(index),.06f,.055f,.88f,.22f,22);
                    var badge=_ui.Frame(button.transform,"CountBadge",.73f,.265f,.22f,.14f);
                    var count=_ui.Text(badge,"Count",deck.Cards.Count+" / 9",.07f,.1f,.86f,.8f,18);count.alignment=TextAlignmentOptions.Midline;
                    count.color=_app.Collection.Validate(index).IsValid?_style.Text:new Color(1,.7f,.3f);
                }
            }
            if(indices.Count==0){
                var label=_ui.Text(_library.Content,"NoDecks",L("noResults"),0,0,1,1);
                label.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=80;_deckRows.Add(label.gameObject);
            }
            _libraryDeck.Refresh(_app.Collection.Get(_editing),null,(type,rarity,card,origin)=>{if(card!=null)Preview(card,origin);else{OpenEditor(_editing);ChooseSlot(type,rarity,null);}});
            _library.Status.text=_app.Collection.Get(_editing).Cards.Count+" / 9 · "+ValidationText();
            var valid=_app.Collection.Validate(_editing).IsValid;_library.Use.interactable=_library.Play.interactable=valid;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_library.Content);
        }
        private void Add(){if(_focused==null)return;_app.Collection.Replace(_editing,_focused.Id);RefreshEditor();}
        private void Remove(){if(_focused==null)return;_app.Collection.Remove(_editing,_focused.Id);_focused=null;RefreshEditor();}
        private void Use(){if(!_app.Collection.Select(_editing))return;Close();}
        private void Play(){if(!_app.Collection.Select(_editing))return;_root.gameObject.SetActive(false);_playAi();}
        private void CloseModal()
        {
            if(_previewCard==null){HideModalInstantly();return;}
            _previewTween?.Kill();_previewActions.gameObject.SetActive(false);
            _previewTween=DOTween.Sequence().SetUpdate(true).SetTarget(this)
                .Join(_previewCard.DOAnchorPos(_previewStart,_style.PreviewCloseSeconds).SetEase(Ease.InCubic))
                .Join(_previewCard.DOScale(_previewStartScale,_style.PreviewCloseSeconds).SetEase(Ease.InCubic))
                .Join(_modal.GetComponent<UnityEngine.UI.Image>().DOFade(0,_style.PreviewCloseSeconds))
                .OnComplete(()=>{HideModalInstantly();Refresh();});
        }
        private void HideModalInstantly()
        {
            _previewTween?.Kill();_previewTween=null;
            if(_previewOrigin!=null)_previewOrigin.alpha=1;
            _previewOrigin=null;_previewCard=null;_previewActions=null;
            if(_modal!=null)_modal.gameObject.SetActive(false);
        }
        private RectTransform FindCardOrigin(CardDef card) => _root.GetComponentsInChildren<UnityEngine.UI.Button>()
            .Where(b=>b.name=="Card_"+card.Id).Select(b=>(RectTransform)b.transform).FirstOrDefault();
        private void Preview(CardDef card,RectTransform origin)
        {
            _focused=card;ShowDetails(origin);
        }
        private RectTransform ModalFrame(string name,float x,float y,float w,float h)
        {
            HideModalInstantly();_modal.GetComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.90f);
            CollectionDeckGrid.Clear(_modalContent);_modal.gameObject.SetActive(true);_modal.SetAsLastSibling();
            var frame=_ui.Frame(_modal,name,x,y,w,h);_modalContent.Add(frame.gameObject);return frame;
        }
        private void ShowDeckActions()
        {
            var frame=ModalFrame("DeckActions",.68f,.70f,.28f,.20f);
            _ui.Button(frame,"ClearDeck",L("clearDeck"),.08f,.53f,.84f,.33f,()=>{_app.Collection.Delete(_editing);_focused=null;CloseModal();Refresh();});
            _ui.Button(frame,"Cancel",_app.Texts.Get(UiTextKeys.Cancel),.08f,.12f,.84f,.30f,CloseModal);
        }
        private void ShowDetails()
        {
            ShowDetails(_focused==null?null:FindCardOrigin(_focused));
        }
        private void ShowDetails(RectTransform origin)
        {
            if(_focused==null)return;
            HideModalInstantly();CollectionDeckGrid.Clear(_modalContent);_modal.gameObject.SetActive(true);_modal.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            var safe=Screen.safeArea;var area=_modal.rect;
            var safeWidth=area.width*safe.width/Mathf.Max(1,Screen.width);
            var safeHeight=area.height*safe.height/Mathf.Max(1,Screen.height);
            var height=Mathf.Min(safeHeight*_style.PreviewHeight,safeWidth*.92f/.7f);
            _previewCard=_ui.Rect(_modal,"CardPreview",.5f,.5f,0,0);_previewCard.sizeDelta=new Vector2(height*.7f,height);
            _modalContent.Add(_previewCard.gameObject);
            _cardView.Create(_previewCard,_focused,0,false,false,"",CloseModal,1,1);
            foreach(var text in _previewCard.GetComponentsInChildren<TMP_Text>()){
                text.fontSizeMin=10;text.fontSizeMax=Mathf.Max(30,text.fontSizeMax);text.overflowMode=TextOverflowModes.Ellipsis;
            }
            var end=area.min+new Vector2(area.width*safe.center.x/Mathf.Max(1,Screen.width),area.height*safe.center.y/Mathf.Max(1,Screen.height));
            _previewStart=end;_previewStartScale=.65f;
            if(origin!=null){
                var corners=new Vector3[4];origin.GetWorldCorners(corners);
                _previewStart=_modal.InverseTransformPoint(origin.TransformPoint(origin.rect.center));
                var sourceHeight=Vector3.Distance(_modal.InverseTransformPoint(corners[0]),_modal.InverseTransformPoint(corners[1]));
                _previewStartScale=Mathf.Max(.05f,sourceHeight/(height*.89f));
                _previewOrigin=origin.GetComponent<CanvasGroup>();
                if(_previewOrigin==null)_previewOrigin=origin.gameObject.AddComponent<CanvasGroup>();
                _previewOrigin.alpha=0;
            }
            _previewCard.anchoredPosition=_previewStart;_previewCard.localScale=Vector3.one*_previewStartScale;
            _previewActions=_ui.Rect(_modal,"PreviewActions",.30f,.035f,.40f,.065f);_modalContent.Add(_previewActions.gameObject);
            var inDeck=_app.Collection.Get(_editing).Cards.Contains(_focused.Id);
            _ui.Button(_previewActions,"PreviewDeckAction",inDeck?L("removeCard"):T("replace"),0,0,.64f,1,()=>{HideModalInstantly();if(inDeck)Remove();else Add();Refresh();});
            _ui.Button(_previewActions,"CloseDetails",L("close"),.68f,0,.32f,1,CloseModal);
            var dim=_modal.GetComponent<UnityEngine.UI.Image>();dim.color=new Color(0,0,0,0);
            _previewTween=DOTween.Sequence().SetUpdate(true).SetTarget(this)
                .Join(_previewCard.DOAnchorPos(end,_style.PreviewOpenSeconds).SetEase(Ease.OutCubic))
                .Join(_previewCard.DOScale(1,_style.PreviewOpenSeconds).SetEase(Ease.OutCubic))
                .Join(dim.DOFade(.90f,_style.PreviewOpenSeconds));
        }
    }
}
