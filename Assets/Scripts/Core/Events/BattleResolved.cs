using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class BattleResolved : GameEvent
    {
        public BattleResolved(
            int fieldId,
            PowerBreakdown host,
            PowerBreakdown guest,
            ComparisonRule rule,
            bool tieBroken,
            PlayerId? winner)
        {
            FieldId = fieldId;
            Host = host;
            Guest = guest;
            Rule = rule;
            TieBroken = tieBroken;
            Winner = winner;
        }

        public int FieldId { get; }

        /// <summary>The host's power by the steps of the count.</summary>
        public PowerBreakdown Host { get; }

        public PowerBreakdown Guest { get; }
        public int HostPower => Host.FinalPower;
        public int GuestPower => Guest.FinalPower;
        public ComparisonRule Rule { get; }

        /// <summary>Powers were equal and an effect («Перевес») gave the win to one side.</summary>
        public bool TieBroken { get; }

        /// <summary>Null on a draw.</summary>
        public PlayerId? Winner { get; }
    }
}
