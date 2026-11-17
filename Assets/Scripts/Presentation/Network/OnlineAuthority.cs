using System;
using System.Linq;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;
using Game.Services.Match;
namespace Game.Presentation.Network
{
    /// <summary>The only machine executing the engine. Transport identity chooses the player, never the payload.</summary>
    public sealed class OnlineAuthority
    {
        private readonly IMatchSession _engine;
        private readonly long[] _lastRequest=new long[2];
        private PlayerId? _submitting;
        private long _ack;
        public int Version {get;private set;}
        public event Action<PlayerId,OnlineFrame> Updated;
        public OnlineAuthority(IMatchSession engine){_engine=engine;}
        public void Start(MatchSetup setup,ulong seed)
        {
            _engine.EventsProduced+=Publish;_engine.Start(setup,seed);
        }
        private void Publish(System.Collections.Generic.IReadOnlyList<GameEvent> events)
        {
            Version++;
            foreach(var player in PlayerIds.All)Updated?.Invoke(player,Snapshot(player,events,player==_submitting?_ack:0));
        }
        public OnlineFrame Snapshot(PlayerId player,System.Collections.Generic.IReadOnlyList<GameEvent> events=null,long ack=0,RejectReason reason=RejectReason.None)=>new OnlineFrame{
            Version=Version,AcknowledgedRequest=ack,Rejection=reason,View=_engine.GetView(player),Power=_engine.GetBattlePower(),
            Events=(events??new GameEvent[0]).Where(e=>e.IsVisibleTo(player)).Select(OnlineCodec.Pack).ToArray(),
            Actions=_engine.GetLegalActions(player).Select(OnlineCodec.Pack).ToArray()
        };
        public void Submit(PlayerId player,long request,int version,int actionIndex)
        {
            if(request<=0)return;
            if(request<=_lastRequest[(int)player]){Updated?.Invoke(player,Snapshot(player,ack:request));return;}
            _lastRequest[(int)player]=request;
            var legal=_engine.GetLegalActions(player);
            if(version!=Version||actionIndex<0||actionIndex>=legal.Count){Updated?.Invoke(player,Snapshot(player,ack:request,reason:RejectReason.WrongPhase));return;}
            _submitting=player;_ack=request;
            try {
                var result=_engine.Submit(legal[actionIndex]);
                if(!result.IsAccepted)Updated?.Invoke(player,Snapshot(player,ack:request,reason:result.Reason));
            } finally {_submitting=null;_ack=0;}
        }
    }
}
