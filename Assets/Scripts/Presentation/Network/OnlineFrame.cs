using Game.Core.State;
using Game.Core.Battle;
using Game.Core.Commands;
namespace Game.Presentation.Network
{
    public sealed class OnlineFrame
    {
        public int Version;
        public long AcknowledgedRequest;
        public RejectReason Rejection;
        public PlayerView View;
        public BattlePower Power;
        public OnlineValue[] Events;
        public OnlineValue[] Actions;
    }
}
