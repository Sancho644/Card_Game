using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;
using Game.Core.Battle;
using Game.Services.Match;
namespace Game.Presentation.Network
{
    /// <summary>Recipient-only cache. It never creates an engine or obtains the opponent's private view.</summary>
    public sealed class OnlineMatchSession : IAsyncMatchSession
    {
        private readonly PlayerId _player;
        private readonly Action<long,int,int> _send;
        private OnlineFrame _frame;
        private long _request;
        private Action<CommandResult> _pending;
        private bool _started;
        public event Action<IReadOnlyList<GameEvent>> EventsProduced;
        public OnlineMatchSession(PlayerId player,Action<long,int,int> send){_player=player;_send=send;}
        public void Start(MatchSetup unused,ulong unusedSeed){_started=true;if(_frame!=null)Emit(_frame);}
        public CommandResult Submit(GameCommand command)=>throw new InvalidOperationException("Online commands require acknowledgement");
        public void SubmitAsync(GameCommand command,Action<CommandResult> completed)
        {
            if(_pending!=null)return;
            var legal=GetLegalActions(_player);var json=OnlineCodec.Encode(command);
            var index=legal.ToList().FindIndex(c=>c.GetType()==command.GetType()&&OnlineCodec.Encode(c)==json);
            if(index<0){completed(CommandResult.Rejected(RejectReason.WrongPhase));return;}
            _pending=completed;_send(++_request,_frame.Version,index);
        }
        public void Receive(OnlineFrame frame)
        {
            if(frame?.View==null||frame.View.Viewer!=_player)throw new ArgumentException("Wrong recipient");
            if(_frame!=null&&frame.Version<_frame.Version)return;
            var newer=_frame==null||frame.Version>_frame.Version;
            if(newer)_frame=frame;
            if(frame.AcknowledgedRequest==_request&&_pending!=null){var callback=_pending;_pending=null;callback(frame.Rejection==RejectReason.None?CommandResult.Accepted(new GameEvent[0]):CommandResult.Rejected(frame.Rejection));}
            if(newer&&_started)Emit(frame);
        }
        private void Emit(OnlineFrame frame)=>EventsProduced?.Invoke(frame.Events.Select(OnlineCodec.Unpack<GameEvent>).ToArray());
        public PlayerView GetView(PlayerId viewer)=>viewer==_player?_frame?.View:throw new InvalidOperationException("Private view");
        public IReadOnlyList<GameCommand> GetLegalActions(PlayerId player)=>player==_player&&_frame!=null&&_pending==null?_frame.Actions.Select(OnlineCodec.Unpack<GameCommand>).ToArray():new GameCommand[0];
        public BattlePower GetBattlePower()=>_frame?.Power;
    }
}
