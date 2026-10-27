using Game.Core.State;

namespace Game.Core.Battle
{
    public sealed class BattleResolution
    {
        internal BattleResolution(int hostPower, int guestPower, PlayerId? winner, BattleConsequences consequences)
        {
            HostPower = hostPower;
            GuestPower = guestPower;
            Winner = winner;
            Consequences = consequences;
        }

        public int HostPower { get; }
        public int GuestPower { get; }
        public PlayerId? Winner { get; }
        public BattleConsequences Consequences { get; }
    }
}
