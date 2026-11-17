using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Core;
using Game.Core.Decks;
using Game.Core.State;
using Game.Presentation.App;
using Game.Services.Match;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
namespace Game.Presentation.Network
{
    /// <summary>Two-player room, connection lifecycle and message transport. The rules stay in OnlineAuthority.</summary>
    public sealed class OnlineRoom : MonoBehaviour, IDisposable
    {
        private const string Channel="card-game-v1";
        private AppServices _app;
        private NetworkManager _net;
        private ISession _room;
        private OnlineAuthority _authority;
        private DeckList _localDeck,_guestDeck;
        private string _fingerprint;
        private ulong? _guest;
        private bool _registered,_hello,_stopping,_launched;
        private float _pendingSince;
        private long _pendingRequest;
        private float _connectingSince;
        private bool _previousBackground;
        public event Action Changed;
        public event Action MatchReady;
        public OnlineMatchSession Match {get;private set;}
        public string Code {get;private set;}="";
        public string StatusKey {get;private set;}="online.connecting";
        public bool HasGuest {get;private set;}
        public bool HostReady {get;private set;}
        public bool GuestReady {get;private set;}
        public bool IsHost {get;private set;}
        public bool IsReady => IsHost?HostReady:GuestReady;
        public bool Started {get;private set;}
        public string Diagnostic {get;private set;}="";
        public PlayerId LocalPlayer=>IsHost?PlayerId.Player1:PlayerId.Player2;
        public void Configure(AppServices app)
        {
            _app=app;_localDeck=app.Collection.Selected.ToDeckList();
            _connectingSince=Time.realtimeSinceStartup;
            _fingerprint=OnlineCodec.Fingerprint(app.Catalog,app.Rules,Application.version);
            DontDestroyOnLoad(gameObject);
        }
        private void CreateNetwork()
        {
            var transport=gameObject.AddComponent<UnityTransport>();
            _net=gameObject.AddComponent<NetworkManager>();
            _net.NetworkConfig=new NetworkConfig{NetworkTransport=transport,EnableSceneManagement=false};
            _net.OnClientDisconnectCallback+=Disconnected;
            _net.OnTransportFailure+=TransportFailed;
        }
        public async Task Connect(bool host,string code="")
        {
            IsHost=host;
            try {
                await _app.OnlineClosing;
                if(_stopping)return;
                _previousBackground=Application.runInBackground;Application.runInBackground=true;
                _connectingSince=Time.realtimeSinceStartup;
                CreateNetwork();
                if(UnityServices.State!=ServicesInitializationState.Initialized){
                    var options=new InitializationOptions();
                    var args=Environment.GetCommandLineArgs();var at=Array.IndexOf(args,"-profile");
                    if(at>=0&&at+1<args.Length)options.SetProfile(args[at+1]);
                    await UnityServices.InitializeAsync(options);
                }
                if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if(_stopping)return;
                _room=host?await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions{MaxPlayers=2,IsPrivate=true}.WithRelayNetwork()):
                    await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant());
                if(_stopping){await _room.LeaveAsync();return;}
                Code=_room.Code;StatusKey="online.waiting";
                Match=new OnlineMatchSession(LocalPlayer,SendCommand);
                Changed?.Invoke();
            }catch(Exception error){if(!_stopping){Diagnostic=error.ToString();Debug.LogWarning("Online connection: "+error);Fail("online.connectFailed");}}
        }
        private void Update()
        {
            if(_stopping||_net==null)return;
            if(!_registered&&_net.IsListening&&_net.CustomMessagingManager!=null){_net.CustomMessagingManager.RegisterNamedMessageHandler(Channel,Receive);_registered=true;}
            if(!_hello&&_room!=null&&!IsHost&&_registered&&_net.IsConnectedClient){
                _hello=true;Send(NetworkManager.ServerClientId,new OnlinePacket{Kind="hello",Fingerprint=_fingerprint,Deck=_localDeck.CardIds.ToArray()});
            }
            if(_pendingRequest>0&&Time.realtimeSinceStartup-_pendingSince>(_app.OnlineSettings?.CommandTimeoutSeconds??20))Fail("online.commandTimeout");
            if(_room==null&&Time.realtimeSinceStartup-_connectingSince>(_app.OnlineSettings?.ConnectionTimeoutSeconds??40))Fail("online.connectFailed");
        }
        public void SetReady()
        {
            if(!HasGuest||Started||_stopping)return;
            if(IsHost){HostReady=!HostReady;PublishRoom();TryStart();}
            else Send(NetworkManager.ServerClientId,new OnlinePacket{Kind="ready",Ready=!GuestReady});
        }
        private void PublishRoom()
        {
            StatusKey="online.roomReady";Changed?.Invoke();
            if(_guest.HasValue)Send(_guest.Value,new OnlinePacket{Kind="room",HasGuest=HasGuest,HostReady=HostReady,GuestReady=GuestReady,Fingerprint=_fingerprint});
        }
        private void TryStart()
        {
            if(Started||!HostReady||!GuestReady||_guestDeck==null)return;
            Started=true;
            _authority=new OnlineAuthority(new LocalMatchSession(_app.Catalog,_app.Rules));
            _authority.Updated+=Deliver;
            var seed=((ulong)(uint)_app.Random.Next(int.MaxValue)<<31)^(uint)_app.Random.Next(int.MaxValue);
            _authority.Start(new MatchSetup(_localDeck,_guestDeck),seed);
        }
        private void Deliver(PlayerId player,OnlineFrame frame)
        {
            if(player==LocalPlayer)Accept(frame);
            else if(_guest.HasValue)Send(_guest.Value,new OnlinePacket{Kind="frame",Frame=frame});
        }
        private void Accept(OnlineFrame frame)
        {
            if(Match==null)return;
            if(frame.AcknowledgedRequest==_pendingRequest)_pendingRequest=0;
            Match.Receive(frame);
            if(_launched)return;
            Started=true;_launched=true;MatchReady?.Invoke();
        }
        private void SendCommand(long request,int version,int index)
        {
            _pendingSince=Time.realtimeSinceStartup;_pendingRequest=request;
            if(IsHost)_authority.Submit(LocalPlayer,request,version,index);
            else Send(NetworkManager.ServerClientId,new OnlinePacket{Kind="command",Request=request,Version=version,ActionIndex=index});
        }
        private void Receive(ulong sender,FastBufferReader reader)
        {
            try {
                reader.ReadValueSafe(out int length);
                if(length<1||length>OnlineCodec.MaxBytes||length>reader.Length-reader.Position)throw new ArgumentException("Packet length");
                var bytes=new byte[length];reader.ReadBytesSafe(ref bytes,length);
                var packet=OnlineCodec.Decode<OnlinePacket>(Encoding.UTF8.GetString(bytes));
                if(IsHost){
                    if(sender==NetworkManager.ServerClientId)return;
                    if(_guest.HasValue&&_guest.Value!=sender){_net.DisconnectClient(sender);return;}
                    if(packet.Kind=="hello"&&!Started){
                        if(packet.Fingerprint!=_fingerprint){Send(sender,new OnlinePacket{Kind="error",ErrorKey="online.versionMismatch"});return;}
                        if(packet.Deck==null||packet.Deck.Length!=9||!new DeckValidator(_app.Catalog,_app.Rules.Deck).Validate(new DeckList(packet.Deck)).IsValid){Send(sender,new OnlinePacket{Kind="error",ErrorKey="online.invalidDeck"});return;}
                        _guest=sender;_guestDeck=new DeckList(packet.Deck);HasGuest=true;HostReady=GuestReady=false;PublishRoom();
                    }else if(_guest==sender&&packet.Kind=="ready"&&!Started){GuestReady=packet.Ready;PublishRoom();TryStart();}
                    else if(_guest==sender&&packet.Kind=="command"&&Started)_authority.Submit(PlayerId.Player2,packet.Request,packet.Version,packet.ActionIndex);
                }else if(sender==NetworkManager.ServerClientId){
                    if(packet.Kind=="room"){
                        if(packet.Fingerprint!=_fingerprint){Fail("online.versionMismatch");return;}
                        HasGuest=packet.HasGuest;HostReady=packet.HostReady;GuestReady=packet.GuestReady;StatusKey="online.roomReady";Changed?.Invoke();
                    }else if(packet.Kind=="frame")Accept(packet.Frame);
                    else if(packet.Kind=="error")Fail(packet.ErrorKey=="online.versionMismatch"?packet.ErrorKey:"online.invalidDeck");
                }
            }catch(Exception error){Diagnostic=error.Message;Debug.LogWarning("Online packet: "+error.Message);Fail("online.protocolError");}
        }
        private void Send(ulong recipient,OnlinePacket packet)
        {
            if(_stopping||!_registered)return;
            var bytes=Encoding.UTF8.GetBytes(OnlineCodec.Encode(packet));
            if(bytes.Length>OnlineCodec.MaxBytes){Fail("online.protocolError");return;}
            using(var writer=new FastBufferWriter(bytes.Length+4,Allocator.Temp)){
                writer.WriteValueSafe(bytes.Length);writer.WriteBytesSafe(bytes);
                _net.CustomMessagingManager.SendNamedMessage(Channel,recipient,writer,NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        private void Disconnected(ulong id){if(!_stopping&&(_hello||HasGuest||Started))Fail("online.disconnected");}
        private void TransportFailed(){if(!_stopping)Fail("online.disconnected");}
        private void Fail(string key)
        {
            StatusKey=key;HasGuest=false;Changed?.Invoke();
            if(Started){_app.OnlineNotice=key;_app.Scenes.ToMainMenu();}
            Dispose();
        }
        public void Dispose()
        {
            if(_stopping)return;_stopping=true;
            _app.OnlineClosing=CloseConnection();
        }
        private async Task CloseConnection()
        {
            if(_net!=null){_net.OnClientDisconnectCallback-=Disconnected;_net.OnTransportFailure-=TransportFailed;if(_registered)_net.CustomMessagingManager?.UnregisterNamedMessageHandler(Channel);}
            try{if(_room!=null)await _room.LeaveAsync();}catch(Exception error){Debug.LogWarning("Online leave: "+error.Message);}
            finally {
                if(_net!=null)_net.Shutdown();
                Application.runInBackground=_previousBackground;
                if(this!=null)Destroy(gameObject);
            }
        }
        private void OnApplicationQuit(){Dispose();}
    }
}
