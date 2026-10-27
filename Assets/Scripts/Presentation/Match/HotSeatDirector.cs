using Game.Core.Events;
using Game.Core.State;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Whose eyes the table is shown through. In hot-seat it changes with every change of the deciding
    /// player, behind a hand-off screen. "Show both hands" is a debug switch.
    /// </summary>
    public sealed class HotSeatDirector
    {
        public PlayerId Viewer { get; set; } = PlayerId.Player1;

        public bool ShowBothHands { get; set; }

        public bool IsVisible(GameEvent gameEvent) => ShowBothHands || gameEvent.IsVisibleTo(Viewer);

        public void Reset() => Viewer = PlayerId.Player1;
    }
}
