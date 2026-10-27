using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class WinScored : GameEvent
    {
        public WinScored(PlayerId player, int totalWins)
        {
            Player = player;
            TotalWins = totalWins;
        }

        public PlayerId Player { get; }
        public int TotalWins { get; }
    }
}
