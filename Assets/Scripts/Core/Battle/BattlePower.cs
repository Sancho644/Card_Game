using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>
    /// The current battle's powers as they would be counted if both players were ready now, and the rule
    /// that would compare them. Public information: a closed field adds nothing and abilities are played openly.
    /// </summary>
    public sealed class BattlePower
    {
        public BattlePower(int fieldId, PlayerId host, PowerBreakdown hostPower, PowerBreakdown guestPower, ComparisonRule rule)
        {
            FieldId = fieldId;
            Host = host;
            HostPower = hostPower;
            GuestPower = guestPower;
            Rule = rule;
        }

        public int FieldId { get; }
        public PlayerId Host { get; }
        public PlayerId Guest => Host.Opponent();
        public PowerBreakdown HostPower { get; }
        public PowerBreakdown GuestPower { get; }
        public ComparisonRule Rule { get; }

        public PowerBreakdown Of(PlayerId player) => player == Host ? HostPower : GuestPower;
    }
}
