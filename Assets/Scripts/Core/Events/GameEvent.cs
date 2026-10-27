using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>
    /// Something that happened in the match. Presentation is built from events.
    /// An event with <see cref="VisibleOnlyTo"/> set must not be shown to the other player.
    /// </summary>
    public abstract class GameEvent
    {
        protected GameEvent(PlayerId? visibleOnlyTo = null)
        {
            VisibleOnlyTo = visibleOnlyTo;
        }

        public PlayerId? VisibleOnlyTo { get; }

        public bool IsVisibleTo(PlayerId player) => VisibleOnlyTo == null || VisibleOnlyTo == player;
    }
}
