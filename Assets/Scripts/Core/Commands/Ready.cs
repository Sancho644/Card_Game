using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>The player has nothing more to do in this battle.</summary>
    public sealed class Ready : GameCommand
    {
        public Ready(PlayerId player)
            : base(player)
        {
        }
    }
}
