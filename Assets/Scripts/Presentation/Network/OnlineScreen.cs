using System;
using System.Threading.Tasks;
using Game.Core;
using Game.Presentation.App;
using Game.Presentation.Match;
using Game.Presentation.Menu;
using Game.Data.Presentation;
using TMPro;
using UnityEngine;
namespace Game.Presentation.Network
{
    public sealed class OnlineScreen : IDisposable
    {
        private readonly AppServices _app;
        private readonly CollectionUi _ui;
        private readonly RectTransform _root;
        private readonly TMP_Text _status,_code,_players;
        private readonly TMP_InputField _input;
        private readonly UnityEngine.UI.Button _create,_join,_ready;
        private OnlineRoom _room;
        private bool _busy,_launched;
        private string _lastError;
        private readonly Action _back;
        public OnlineScreen(Transform parent,TMP_FontAsset font,CollectionSettings style,AppServices app,Action back)
        {
            _app=app;_back=back;_ui=new CollectionUi(font,style);
            _root=_ui.Panel(parent,"OnlineRoom",0,0,1,1,new Color(0,.025f,.02f,.96f));
            var panel=_ui.Frame(_root,"Panel",.18f,.12f,.64f,.76f);
            var title=_ui.Text(panel,"Title",T("online.title"),.08f,.83f,.84f,.10f,40);title.alignment=TextAlignmentOptions.Midline;
            _status=_ui.Text(panel,"Status","",.08f,.68f,.84f,.13f,24);
            _code=_ui.Text(panel,"Code","",.08f,.55f,.62f,.1f,36);
            _ui.Button(panel,"Copy",T("online.copy"),.73f,.55f,.19f,.09f,()=>GUIUtility.systemCopyBuffer=_room?.Code??"");
            _input=_ui.Input(panel,"JoinCode",.08f,.41f,.53f,.09f,T("online.enterCode"),_=>{});
            _input.characterLimit=16;_input.contentType=TMP_InputField.ContentType.Alphanumeric;
            _create=_ui.Button(panel,"Create",T("online.create"),.08f,.28f,.39f,.09f,()=>Connect(true));
            _join=_ui.Button(panel,"Join",T("online.join"),.53f,.28f,.39f,.09f,()=>Connect(false));
            _players=_ui.Text(panel,"Players","",.08f,.17f,.55f,.09f,22);
            _ready=_ui.Button(panel,"Ready",T("online.ready"),.66f,.16f,.26f,.09f,()=>_room?.SetReady());
            _ui.Button(panel,"Back",T("online.back"),.32f,.035f,.36f,.085f,Close);
            _root.gameObject.SetActive(false);
        }
        private string T(string key)=>_app.Texts.Get(key);
        public void Show(){_root.gameObject.SetActive(true);_root.SetAsLastSibling();Refresh();}
        public void Notice(string key){_lastError=key;Show();}
        public void JoinCode(string code){_input.text=code;Show();Connect(false);}
        public async void Connect(bool host)
        {
            if(_busy||_room!=null)return;
            if(!host&&string.IsNullOrWhiteSpace(_input.text)){_status.text=T("online.enterCode");return;}
            _busy=true;
            _lastError=null;
            var go=new GameObject("OnlineConnection");_room=go.AddComponent<OnlineRoom>();_room.Configure(_app);
            _room.Changed+=Refresh;_room.MatchReady+=Launch;
            var room=_room;
            Refresh();await room.Connect(host,_input.text);if(_root==null||_room!=room)return;_busy=false;Refresh();
        }
        private void Refresh()
        {
            _create.interactable=_join.interactable=!_busy&&_room==null;
            _input.interactable=_room==null;
            _ready.interactable=_room!=null&&_room.HasGuest&&!_room.Started;
            if(_room!=null&&_room.StatusKey!="online.connecting"&&_room.StatusKey!="online.waiting"&&_room.StatusKey!="online.roomReady")_lastError=_room.StatusKey;
            _status.text=_room==null?T(_lastError??"online.hint"):T(_room.StatusKey);
            _code.text=_app.Texts.Format("online.code",_room?.Code??"-");
            _players.text=_app.Texts.Format("online.players",_room?.HostReady==true?T("online.readyState"):T("online.waitState"),_room?.GuestReady==true?T("online.readyState"):T("online.waitState"));
            _ready.GetComponentInChildren<TMP_Text>().text=T(_room?.IsReady==true?"online.notReady":"online.ready");
        }
        private void Launch()
        {
            if(_launched)return;_launched=true;
            var deck=_app.Collection.Selected.ToDeckList();
            // Setup is a local placeholder only; the network session never starts an engine with it.
            _app.PendingMatch=new MatchLaunch(_room.Match,new MatchSetup(deck,deck),0,
                MatchMode.VersusOpponent(_room.LocalPlayer,UiTextKeys.PlayerYou,UiTextKeys.PlayerOpponent),new[]{_room.LocalPlayer},_room);
            _app.Scenes.ToMatch();
        }
        private void Close(){Detach();_root.gameObject.SetActive(false);_back();}
        private void Detach(){if(_room!=null){_room.Changed-=Refresh;_room.MatchReady-=Launch;if(!_launched)_room.Dispose();}_room=null;_busy=false;}
        public void Dispose(){Detach();_ui.Dispose();if(_root!=null)UnityEngine.Object.Destroy(_root.gameObject);}
    }
}
