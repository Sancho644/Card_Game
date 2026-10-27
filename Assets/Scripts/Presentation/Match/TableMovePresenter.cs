using System;
using System.Collections.Generic;
using Game.Core.Events;

namespace Game.Presentation.Match
{
    /// <summary>Events that move or turn cards: the table is brought to the new state at that point.</summary>
    public sealed class TableMovePresenter : IEventPresenter
    {
        private static readonly HashSet<Type> MovingEvents = new HashSet<Type>
        {
            typeof(FieldsDealt),
            typeof(HandDealt),
            typeof(CreaturePlaced),
            typeof(CreatureMoved),
            typeof(FieldOpened),
            typeof(CreatureOut),
            typeof(CreatureReturnedToHand),
            typeof(FieldRemoved),
            typeof(AbilityDiscarded),
            typeof(AbilityReturnedToHand),
            typeof(BattleRetreated),
        };

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (MovingEvents.Contains(gameEvent.GetType()))
                context.EnqueueSync();
        }
    }
}
