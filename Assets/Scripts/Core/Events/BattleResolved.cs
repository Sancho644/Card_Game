using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class BattleResolved : GameEvent
    {
        public BattleResolved(int fieldId, int hostPower, int guestPower, PlayerId? winner)
        {
            FieldId = fieldId;
            HostPower = hostPower;
            GuestPower = guestPower;
            Winner = winner;
        }

        public int FieldId { get; }
        public int HostPower { get; }
        public int GuestPower { get; }

        /// <summary>Null on a draw.</summary>
        public PlayerId? Winner { get; }
    }
}
