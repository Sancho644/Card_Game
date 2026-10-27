using Game.Core.Events;

namespace Game.Presentation.Match
{
    /// <summary>Turns engine events into animation steps. Every presenter sees every visible event.</summary>
    public interface IEventPresenter
    {
        void Present(GameEvent gameEvent, PresentationContext context);
    }
}
