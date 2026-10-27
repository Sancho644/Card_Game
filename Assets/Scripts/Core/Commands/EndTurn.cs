using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>Closes the "after" window. Without a main action it counts as a skip, allowed only if none was possible.</summary>
    public sealed class EndTurn : GameCommand
    {
        public EndTurn(PlayerId player)
            : base(player)
        {
        }
    }
}
