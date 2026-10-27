using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>The coin toss winner decides whether to move first.</summary>
    public sealed class ChooseTurnOrder : GameCommand
    {
        public ChooseTurnOrder(PlayerId player, bool goFirst)
            : base(player)
        {
            GoFirst = goFirst;
        }

        public bool GoFirst { get; }
    }
}
