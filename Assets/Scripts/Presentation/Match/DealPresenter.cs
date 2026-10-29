using Game.Core.Events;
using Game.Data.Presentation;

namespace Game.Presentation.Match
{
    /// <summary>Dealt fields and hands fly onto the table one card after another.</summary>
    public sealed class DealPresenter : IEventPresenter
    {
        private readonly AnimationSettings _animation;

        public DealPresenter(AnimationSettings animation)
        {
            _animation = animation;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (gameEvent is FieldsDealt || gameEvent is HandDealt)
                context.EnqueueDeal(_animation.DealInterval);
        }
    }
}
