using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class TurnOrderChosen : GameEvent
    {
        public TurnOrderChosen(PlayerId chooser, PlayerId firstPlayer)
        {
            Chooser = chooser;
            FirstPlayer = firstPlayer;
        }

        public PlayerId Chooser { get; }
        public PlayerId FirstPlayer { get; }
    }
}
