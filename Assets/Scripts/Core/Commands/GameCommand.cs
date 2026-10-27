using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>
    /// A player's intent. Commands are plain data so they can travel over the network later;
    /// the engine validates every command and trusts nothing.
    /// </summary>
    public abstract class GameCommand
    {
        protected GameCommand(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }
}
